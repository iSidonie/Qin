using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ScrollManager : MonoBehaviour
{
    private static ScrollManager _instance;
    public static ScrollManager Instance => _instance;

    public ScrollRect scrollView;         // 关联 Scroll View
    public float scrollSpeedFactor; // 自动滚动速度因子
    public float edgeThreshold;    // 距离边缘多少像素触发滚动

    // public float scrollDuration = 0.5f; // 滚动动画时长
    public float userInteractionDelay = 3f; // 用户交互后的延迟时间

    private RectTransform viewportRect;  // Scroll View 的 Viewport
    private Tweener scrollTweener; // 滚动动画控制器
    private bool isUserInteracting = false;
    private bool isAutoScrolling = false;
    private bool isDragging = false;

    public float doubleClickThreshold = 0.2f; // 双击的时间间隔阈值
    private float lastClickTime = -1f;

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

        NotationSelectHandler.Instance.OnNotationStartDragging += (value) => SetUserInteraction();
        NotationSelectHandler.Instance.OnNotationEndDragging += SetUserInteraction;
        NotationSelectHandler.Instance.OnNotationSelecting += (value) => SetUserInteraction();
    }

    void Start()
    {
        if (scrollView != null)
        {
            viewportRect = scrollView.viewport;
            edgeThreshold = viewportRect.rect.height * 0.15f;
            scrollView.onValueChanged.AddListener(OnScrollValueChanged);// 监听滚动事件
        }
        else
        {
            Debug.LogError("Scroll View is not assigned!");
        }
    }

    void Update()
    {
        if (isDragging)
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

    /// <summary>
    /// 滚动事件处理。
    /// </summary>
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
        isDragging = true;
    }

    /// <summary>
    /// 停止拖动选择逻辑。
    /// </summary>
    public void StopDragSelection()
    {
        isDragging = false;
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

        // 根据距离边缘的程度调整滚动速度
        if (verticalScroll != 0f)
        {
            Debug.Log(verticalScroll);
            float normalizedScrollSpeed = Mathf.Clamp(verticalScroll, -1f, 1f);
            scrollView.verticalNormalizedPosition += normalizedScrollSpeed * scrollSpeedFactor * Time.deltaTime;
            scrollView.verticalNormalizedPosition = Mathf.Clamp(scrollView.verticalNormalizedPosition, 0f, 1f);
        }
    }

    /// <summary>
    /// 双击空白区域触发的操作。
    /// </summary>
    private void OnDoubleClickEmptySpace()
    {
        Debug.Log("Double-clicked on empty space");

        // 调用播放/暂停功能
        AudioPlayer.Instance.TogglePlayPause();
    }

    public void SetEnable(bool isEnable)
    {
        if (scrollView != null)
        {
            scrollView.enabled = isEnable; // 禁用滚动
        }
    }
}
