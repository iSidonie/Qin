using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ScrollManager : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private static ScrollManager _instance;
    public static ScrollManager Instance => _instance;

    public ScrollRect scrollView;         // ���� Scroll View
    public float scrollSpeedFactor; // �Զ������ٶ�����
    public float edgeThreshold;    // �����Ե�������ش�������
    
    public float userInteractionDelay = 3f; // �û���������ӳ�ʱ��

    public EventTrigger eventTrigger;

    private RectTransform viewportRect;  // Scroll View �� Viewport
    private Tweener scrollTweener; // ��������������
    private bool isUserInteracting = false;
    private bool isAutoScrolling = false;
    private bool isSelectDragging = false;
    private string scrollDirection = "None"; // ��ǰ�������� ("Horizontal" �� "Vertical")
    
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
            scrollView.onValueChanged.AddListener(OnScrollValueChanged);// ���������¼�

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
    /// ƽ��������ָ��Ԫ�����롣
    /// </summary>
    public void ScrollToCenter(RectTransform target, float time = 0.5f)
    {
        if (isUserInteracting || target == null || scrollView == null)
            return;

        // ��ʼ�Զ�����
        isAutoScrolling = true;

        // ����Ŀ��λ��
        float targetPosY = CalculateScrollPosition(target);

        // ƽ��������Ŀ��λ��
        SmoothScrollToPosition(targetPosY, time);
    }

    /// <summary>
    /// ����Ŀ��Ԫ���� ScrollView �еĹ�һ��λ�á�
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
    /// ʹ�� DoTween ƽ��������Ŀ��λ�á�
    /// </summary>
    private void SmoothScrollToPosition(float targetPosY, float time)
    {
        // �����δ��ɵĹ���������ֹͣ��
        scrollTweener?.Kill();

        // �����µĹ�������
        scrollTweener = scrollView.DOVerticalNormalizedPos(targetPosY, time)
            .SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                isAutoScrolling = false;
            });
    }

    // <summary>
    // �����¼�����
    // </summary>
    private void OnScrollValueChanged(Vector2 position)
    {
        // ��������Զ������������¼�
        if (isAutoScrolling)
            return;
        
        SetUserInteraction();
    }

    public void SetUserInteraction()
    {
        // �û����ڽ���
        isUserInteracting = true;

        // ֹͣ�Զ�����
        scrollTweener?.Kill();

        // �ӳٻָ��Զ���������
        CancelInvoke(nameof(ResetUserInteraction));
        Invoke(nameof(ResetUserInteraction), userInteractionDelay);
    }

    /// <summary>
    /// �ָ��Զ������߼���
    /// </summary>
    public void ResetUserInteraction()
    {
        isUserInteracting = false;
    }

    /// <summary>
    /// �����϶�ѡ���߼���
    /// </summary>
    public void StartDragSelection()
    {
        isSelectDragging = true;
    }

    /// <summary>
    /// ֹͣ�϶�ѡ���߼���
    /// </summary>
    public void StopDragSelection()
    {
        isSelectDragging = false;

        // ����Ƿ���Ҫ��ҳ
        CheckForPageFlip();
    }

    /// <summary>
    /// ��鲢�Զ����� Scroll View��
    /// </summary>
    private void AutoScrollIfNeeded()
    {
        if (viewportRect == null) return;

        // ��ȡ�������� Viewport �ľֲ�����
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewportRect, Input.mousePosition, null, out Vector2 localMousePosition);
        
        // ���� Pivot = (0, 1) ���� Y ����
        float adjustedY = localMousePosition.y;

        // ��ʼ����������
        float verticalScroll = 0f;
        float horizontalScroll = 0f;

        // �������Ƿ��ڶ�����Ե
        if (adjustedY > viewportRect.rect.height / 2 - edgeThreshold)
        {
            verticalScroll = (adjustedY - (viewportRect.rect.height / 2 - edgeThreshold)) / edgeThreshold;
        }
        // �������Ƿ��ڵײ���Ե
        else if (adjustedY < edgeThreshold - viewportRect.rect.height / 2)
        {
            verticalScroll = (adjustedY - (edgeThreshold - viewportRect.rect.height / 2)) / edgeThreshold;
        }
        // �������Ƿ�������Ե (ˮƽ)
        else if (localMousePosition.x < -viewportRect.rect.width / 2 + edgeThreshold)
        {
            horizontalScroll = (localMousePosition.x - (-viewportRect.rect.width / 2 + edgeThreshold)) / edgeThreshold;
        }
        // �������Ƿ����Ҳ��Ե (ˮƽ)
        else if (localMousePosition.x > viewportRect.rect.width / 2 - edgeThreshold)
        {
            horizontalScroll = (localMousePosition.x - (viewportRect.rect.width / 2 - edgeThreshold)) / edgeThreshold;
        }

        // ���ݾ����Ե�ĳ̶ȵ��������ٶ�
        if (verticalScroll != 0f)
        {
            float normalizedScrollSpeed = Mathf.Clamp(verticalScroll, -1f, 1f);
            scrollView.verticalNormalizedPosition += normalizedScrollSpeed * scrollSpeedFactor * Time.deltaTime;
            scrollView.verticalNormalizedPosition = Mathf.Clamp(scrollView.verticalNormalizedPosition, 0f, 1f);
        }

        // ����ˮƽ�����ٶȲ����� ScrollView ˮƽλ��
        if (horizontalScroll != 0f)
        {
            float normalizedHorizontalSpeed = Mathf.Clamp(horizontalScroll, -1f, 1f);
            scrollView.horizontalNormalizedPosition += normalizedHorizontalSpeed * scrollSpeedFactor * Time.deltaTime;
            scrollView.horizontalNormalizedPosition = Mathf.Clamp(scrollView.horizontalNormalizedPosition, 0f, 1f);
        }
    }

    /// <summary>
    /// ����Ƿ���Ҫ������ҳ��
    /// </summary>
    private void CheckForPageFlip()
    {
        // ���ˮƽ�����������
        if (scrollView.horizontalNormalizedPosition <= 0.4f)
        {
            PageManager.Instance.SlideToPreviousPage();
        }
        // ���ˮƽ���������ұ�
        else if (scrollView.horizontalNormalizedPosition >= 0.6f)
        {
            PageManager.Instance.SlideToNextPage();
        }
        else
        {
            PageManager.Instance.ResetPagePosition();
        }
    }

    // �м䷽������ BaseEventData ת��Ϊ PointerEventData 
    private void OnBeginDragBase(BaseEventData baseEventData)
    {
        OnBeginDrag((PointerEventData)baseEventData);
    }

    private void OnEndDragBase(BaseEventData baseEventData)
    {
        OnEndDrag((PointerEventData)baseEventData);
    }

    /// <summary>
    /// �û���ʼ�϶�ʱ���������򲢽��ö�Ӧ�ᡣ
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // �����϶����������
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
    /// �û�ֹͣ�϶�ʱ�ָ��ᡣ
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (scrollDirection == "Horizontal")
        {
            dragEndPosition = eventData.position;
            float dragDistance = dragEndPosition.x - dragStartPosition.x;
            float viewportWidth = scrollView.viewport.rect.width;
            Debug.Log(dragDistance);

            if (dragDistance > viewportWidth * 0.1f) // �ж��Ƿ����ҷ�ҳ
            {
                // Debug.Log("�ҷ���SlideToNextPage");
                PageManager.Instance.SlideToPreviousPage();
            }
            else if (dragDistance < -viewportWidth * 0.1f) // �ж��Ƿ�����ҳ
            {
                // Debug.Log("�󷭣�SlideToNextPage");
                PageManager.Instance.SlideToNextPage();
            }
            else
            {
                PageManager.Instance.ResetPagePosition(); // �������벻�㣬��λ
            }
        }

        // �϶�����ʱ�ָ���������
        ResetScrollDirection();
    }

    /// <summary>
    /// ���ù�������
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
    /// ���ù��������������ɹ�����
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
            scrollView.enabled = isEnable; // ���ù���
        }
    }
}
