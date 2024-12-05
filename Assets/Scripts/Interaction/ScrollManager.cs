using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ScrollManager : MonoBehaviour
{
    private static ScrollManager _instance;
    public static ScrollManager Instance => _instance;

    public ScrollRect scrollView;         // ���� Scroll View
    public float scrollSpeedFactor; // �Զ������ٶ�����
    public float edgeThreshold;    // �����Ե�������ش�������

    public float scrollDuration = 0.5f; // ��������ʱ��
    public float userInteractionDelay = 3f; // �û���������ӳ�ʱ��

    private RectTransform viewportRect;  // Scroll View �� Viewport
    private Tweener scrollTweener; // ��������������
    private bool isUserInteracting = false;
    private bool isAutoScrolling = false;
    private bool isDragging = false;

    public float doubleClickThreshold = 0.2f; // ˫����ʱ������ֵ
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

    void Start()
    {
        if (scrollView != null)
        {
            viewportRect = scrollView.viewport;
            edgeThreshold = viewportRect.rect.height * 0.15f;
            scrollView.onValueChanged.AddListener(OnScrollValueChanged);// ���������¼�
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

        //// ������������
        //if (Input.GetMouseButtonDown(0))
        //{
        //    HandleMouseClick();
        //}
    }

    /// <summary>
    /// ƽ��������ָ��Ԫ�����롣
    /// </summary>
    public void ScrollToCenter(RectTransform target)
    {
        if (isUserInteracting || target == null || scrollView == null)
            return;

        // ��ʼ�Զ�����
        isAutoScrolling = true;

        // ����Ŀ��λ��
        float targetPosY = CalculateScrollPosition(target);

        // ƽ��������Ŀ��λ��
        SmoothScrollToPosition(targetPosY);
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
    private void SmoothScrollToPosition(float targetPosY)
    {
        // �����δ��ɵĹ���������ֹͣ��
        scrollTweener?.Kill();

        // �����µĹ�������
        scrollTweener = scrollView.DOVerticalNormalizedPos(targetPosY, scrollDuration)
            .SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                isAutoScrolling = false;
            });
    }

    /// <summary>
    /// �����¼�����
    /// </summary>
    private void OnScrollValueChanged(Vector2 position)
    {
        // ��������Զ������������¼�
        if (isAutoScrolling)
            return;

        SetUserInteraction();
    }

    private void SetUserInteraction()
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
    private void ResetUserInteraction()
    {
        isUserInteracting = false;
    }

    /// <summary>
    /// �����϶�ѡ���߼���
    /// </summary>
    public void StartDragSelection()
    {
        isDragging = true;
    }

    /// <summary>
    /// ֹͣ�϶�ѡ���߼���
    /// </summary>
    public void StopDragSelection()
    {
        isDragging = false;
    }

    /// <summary>
    /// ��鲢�Զ����� Scroll View��
    /// </summary>
    private void AutoScrollIfNeeded()
    {
        if (viewportRect == null) return;

        // ��ȡ�������� Viewport �ľֲ�����
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewportRect, Input.mousePosition, null, out Vector2 localMousePosition);
        
        //// ���� Pivot = (0, 1) ���� Y ����
        //float adjustedY = localMousePosition.y + viewportRect.rect.height;
        // ���� Pivot = (0, 1) ���� Y ����
        float adjustedY = localMousePosition.y;

        // ��ʼ����������
        float verticalScroll = 0f;

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

        // ���ݾ����Ե�ĳ̶ȵ��������ٶ�
        if (verticalScroll != 0f)
        {
            Debug.Log(verticalScroll);
            float normalizedScrollSpeed = Mathf.Clamp(verticalScroll, -1f, 1f);
            scrollView.verticalNormalizedPosition += normalizedScrollSpeed * scrollSpeedFactor * Time.deltaTime;
            scrollView.verticalNormalizedPosition = Mathf.Clamp(scrollView.verticalNormalizedPosition, 0f, 1f);
        }
    }
    
    private void HandleMouseClick()
    {
        // ����Ƿ����� UI Ԫ��
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ����� UI �ϣ���ִ�пհ��������
            return;
        }

        // ���˫��
        float currentTime = Time.time;
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            OnDoubleClickEmptySpace();
            lastClickTime = -1f; // ���ã���ֹ��δ���
        }
        else
        {
            lastClickTime = currentTime;
        }
    }

    /// <summary>
    /// ˫���հ����򴥷��Ĳ�����
    /// </summary>
    private void OnDoubleClickEmptySpace()
    {
        Debug.Log("Double-clicked on empty space");

        // ���ò���/��ͣ����
        AudioPlayer.Instance.TogglePlayPause();
    }
}
