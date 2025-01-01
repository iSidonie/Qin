using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ScrollManager : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private static ScrollManager _instance;
    public static ScrollManager Instance => _instance;

    public ScrollRect scrollView;         // 关联 Scroll View
    public float scrollSpeedFactor; // 自动滚动速度因子
    public float edgeThreshold;    // 距离边缘多少像素触发滚动
    
    public float userInteractionDelay = 3f; // 用户交互后的延迟时间

    public EventTrigger eventTrigger;

    private RectTransform viewportRect;  // Scroll View 的 Viewport
    private Tweener scrollTweener; // 滚动动画控制器
    private bool isUserInteracting = false;
    private bool isAutoScrolling = false;
    private bool isSelectDragging = false;
    private string scrollDirection = "None"; // 当前滚动方向 ("Horizontal" 或 "Vertical")
    
    private Vector2 dragStartPosition;
    private Vector2 dragEndPosition;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
        
    private void OnEnable()
    {
        NotationSelectHandler.Instance.OnNotationStartDragging += (value) => StartDragSelection();
        NotationSelectHandler.Instance.OnNotationEndDragging += StopDragSelection;

        NotationSelectHandler.Instance.OnNotationStartDragging += (value) => SetUserInteraction();
        NotationSelectHandler.Instance.OnNotationEndDragging += SetUserInteraction;
        NotationSelectHandler.Instance.OnNotationSelecting += (value) => SetUserInteraction();
    }

    private void OnDisable()
    {
        NotationSelectHandler.Instance.OnNotationStartDragging -= (value) => StartDragSelection();
        NotationSelectHandler.Instance.OnNotationEndDragging -= StopDragSelection;

        NotationSelectHandler.Instance.OnNotationStartDragging -= (value) => SetUserInteraction();
        NotationSelectHandler.Instance.OnNotationEndDragging -= SetUserInteraction;
        NotationSelectHandler.Instance.OnNotationSelecting -= (value) => SetUserInteraction();
    }

    void Start()
    {
        if (scrollView != null)
        {
            viewportRect = scrollView.viewport;
            edgeThreshold = viewportRect.rect.height * 0.15f;
            scrollView.onValueChanged.AddListener(OnScrollValueChanged);// 监听滚动事件

            if (eventTrigger == null)
            {
                eventTrigger = GetComponent<EventTrigger>();
            }
            AddEventTriggerListener(EventTriggerType.BeginDrag, OnBeginDragBase);
            AddEventTriggerListener(EventTriggerType.EndDrag, OnEndDragBase);
        }
        else
        {
            Debug.LogError("Scroll View is not assigned!");
        }
    }
    private void AddEventTriggerListener(EventTriggerType eventType, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        eventTrigger.triggers.Add(entry);
    }

    void Update()
    {
        if (isSelectDragging)
        {
            AutoScrollIfNeeded();
        }
    }

    /// <summary>
    /// 平滑滚动到指定元素中央。
    /// </summary>
    public void ScrollToCenter(RectTransform target, float time = 0.5f)
    {
        if (isUserInteracting || target == null || scrollView == null)
            return;

        // 开始自动滚动
        isAutoScrolling = true;

        // 计算目标位置
        float targetPosY = CalculateScrollPosition(target);

        // 平滑滚动到目标位置
        SmoothScrollToPosition(targetPosY, time);
    }

    /// <summary>
    /// 计算目标元素在 ScrollView 中的归一化位置。
    /// </summary>
    private float CalculateScrollPosition(RectTransform target)
    {
        Vector2 viewportSize = scrollView.viewport.rect.size;
        Vector2 contentSize = scrollView.content.rect.size;

        Vector2 targetCenter = scrollView.content.InverseTransformPoint(target.position);

        float normalizedY = Mathf.Clamp01((targetCenter.y + contentSize.y / 2 - viewportSize.y / 2) / (contentSize.y - viewportSize.y));

        return normalizedY;
    }

    /// <summary>
    /// 使用 DoTween 平滑滚动到目标位置。
    /// </summary>
    private void SmoothScrollToPosition(float targetPosY, float time)
    {
        // 如果有未完成的滚动动画，停止它
        scrollTweener?.Kill();

        // 创建新的滚动动画
        scrollTweener = scrollView.DOVerticalNormalizedPos(targetPosY, time)
            .SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                isAutoScrolling = false;
            });
    }

    // <summary>
    // 滚动事件处理。
    // </summary>
    private void OnScrollValueChanged(Vector2 position)
    {
        // 如果正在自动滚动，忽略事件
        if (isAutoScrolling)
            return;
        
        SetUserInteraction();
    }

    public void SetUserInteraction()
    {
        // 用户正在交互
        isUserInteracting = true;

        // 停止自动滚动
        scrollTweener?.Kill();

        // 延迟恢复自动滚动功能
        CancelInvoke(nameof(ResetUserInteraction));
        Invoke(nameof(ResetUserInteraction), userInteractionDelay);
    }

    /// <summary>
    /// 恢复自动滚动逻辑。
    /// </summary>
    public void ResetUserInteraction()
    {
        isUserInteracting = false;
    }

    /// <summary>
    /// 启动拖动选择逻辑。
    /// </summary>
    public void StartDragSelection()
    {
        isSelectDragging = true;
    }

    /// <summary>
    /// 停止拖动选择逻辑。
    /// </summary>
    public void StopDragSelection()
    {
        isSelectDragging = false;

        // 检查是否需要翻页
        CheckForPageFlip();
    }

    /// <summary>
    /// 检查并自动滚动 Scroll View。
    /// </summary>
    private void AutoScrollIfNeeded()
    {
        if (viewportRect == null) return;

        // 获取鼠标相对于 Viewport 的局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewportRect, Input.mousePosition, null, out Vector2 localMousePosition);
        
        // 根据 Pivot = (0, 1) 调整 Y 坐标
        float adjustedY = localMousePosition.y;

        // 初始化滚动变量
        float verticalScroll = 0f;
        float horizontalScroll = 0f;

        // 检查鼠标是否在顶部边缘
        if (adjustedY > viewportRect.rect.height / 2 - edgeThreshold)
        {
            verticalScroll = (adjustedY - (viewportRect.rect.height / 2 - edgeThreshold)) / edgeThreshold;
        }
        // 检查鼠标是否在底部边缘
        else if (adjustedY < edgeThreshold - viewportRect.rect.height / 2)
        {
            verticalScroll = (adjustedY - (edgeThreshold - viewportRect.rect.height / 2)) / edgeThreshold;
        }
        // 检查鼠标是否在左侧边缘 (水平)
        else if (localMousePosition.x < -viewportRect.rect.width / 2 + edgeThreshold)
        {
            horizontalScroll = (localMousePosition.x - (-viewportRect.rect.width / 2 + edgeThreshold)) / edgeThreshold;
        }
        // 检查鼠标是否在右侧边缘 (水平)
        else if (localMousePosition.x > viewportRect.rect.width / 2 - edgeThreshold)
        {
            horizontalScroll = (localMousePosition.x - (viewportRect.rect.width / 2 - edgeThreshold)) / edgeThreshold;
        }

        // 根据距离边缘的程度调整滚动速度
        if (verticalScroll != 0f)
        {
            float normalizedScrollSpeed = Mathf.Clamp(verticalScroll, -1f, 1f);
            scrollView.verticalNormalizedPosition += normalizedScrollSpeed * scrollSpeedFactor * Time.deltaTime;
            scrollView.verticalNormalizedPosition = Mathf.Clamp(scrollView.verticalNormalizedPosition, 0f, 1f);
        }

        // 调整水平滚动速度并更新 ScrollView 水平位置
        if (horizontalScroll != 0f)
        {
            float normalizedHorizontalSpeed = Mathf.Clamp(horizontalScroll, -1f, 1f);
            scrollView.horizontalNormalizedPosition += normalizedHorizontalSpeed * scrollSpeedFactor * Time.deltaTime;
            scrollView.horizontalNormalizedPosition = Mathf.Clamp(scrollView.horizontalNormalizedPosition, 0f, 1f);
        }
    }

    /// <summary>
    /// 检查是否需要触发翻页。
    /// </summary>
    private void CheckForPageFlip()
    {
        // 如果水平滚动到最左边
        if (scrollView.horizontalNormalizedPosition <= 0.4f)
        {
            PageManager.Instance.SlideToPreviousPage();
        }
        // 如果水平滚动到最右边
        else if (scrollView.horizontalNormalizedPosition >= 0.6f)
        {
            PageManager.Instance.SlideToNextPage();
        }
        else
        {
            PageManager.Instance.ResetPagePosition();
        }
    }

    // 中间方法，将 BaseEventData 转换为 PointerEventData 
    private void OnBeginDragBase(BaseEventData baseEventData)
    {
        OnBeginDrag((PointerEventData)baseEventData);
    }

    private void OnEndDragBase(BaseEventData baseEventData)
    {
        OnEndDrag((PointerEventData)baseEventData);
    }

    /// <summary>
    /// 用户开始拖动时检测滚动方向并禁用对应轴。
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 根据拖动方向禁用轴
        float deltaX = Mathf.Abs(eventData.delta.x);
        float deltaY = Mathf.Abs(eventData.delta.y);

        if (deltaX > deltaY)
        {
            SetScrollDirection("Horizontal");
        }
        else if (deltaY > deltaX)
        {
            SetScrollDirection("Vertical");
        }

        dragStartPosition = eventData.position;
    }

    /// <summary>
    /// 用户停止拖动时恢复轴。
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (scrollDirection == "Horizontal")
        {
            dragEndPosition = eventData.position;
            float dragDistance = dragEndPosition.x - dragStartPosition.x;
            float viewportWidth = scrollView.viewport.rect.width;
            Debug.Log(dragDistance);

            if (dragDistance > viewportWidth * 0.1f) // 判断是否向右翻页
            {
                // Debug.Log("右翻，SlideToNextPage");
                PageManager.Instance.SlideToPreviousPage();
            }
            else if (dragDistance < -viewportWidth * 0.1f) // 判断是否向左翻页
            {
                // Debug.Log("左翻，SlideToNextPage");
                PageManager.Instance.SlideToNextPage();
            }
            else
            {
                PageManager.Instance.ResetPagePosition(); // 滑动距离不足，复位
            }
        }

        // 拖动结束时恢复滚动方向
        ResetScrollDirection();
    }

    /// <summary>
    /// 设置滚动方向。
    /// </summary>
    private void SetScrollDirection(string direction)
    {
        scrollDirection = direction;

        if (direction == "Horizontal")
        {
            scrollView.horizontal = true;
            scrollView.vertical = false;
        }
        else if (direction == "Vertical")
        {
            scrollView.horizontal = false;
            scrollView.vertical = true;
        }
    }

    /// <summary>
    /// 重置滚动方向，允许自由滚动。
    /// </summary>
    private void ResetScrollDirection()
    {
        scrollDirection = "None";
        scrollView.horizontal = true;
        scrollView.vertical = true;
    }

    public void SetEnable(bool isEnable)
    {
        if (scrollView != null)
        {
            scrollView.enabled = isEnable; // 禁用滚动
        }
    }
}
