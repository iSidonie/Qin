using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum NotationState
{   
    Hidden,      // 隐藏状态（透明度为 0）
    Selected,    // 手动选择状态（默认颜色，透明度为 1）
    Highlighted  // 播放高亮状态（高亮颜色，透明度为 1）
}

public class NotationViewManager : MonoBehaviour
{
    private static NotationViewManager _instance;
    public static NotationViewManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NotationViewManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(NotationViewManager));
                    _instance = obj.AddComponent<NotationViewManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    public RectTransform notationContainer;
    public Image mainNotationPrefab;
    public Image continuationNotationPrefab;

    private Dictionary<int, Image> notationImages = new Dictionary<int, Image>();
    private int currentHighlightedId = -1;
    private int rangeStart = 0;
    private int rangeEnd = -1;
    
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
        NotationPlaybackManager.Instance.OnNotationChanged += (value1, value2) => UpdateHighlight(value2);
        NotationSelectionManager.SelectedChanged += UpdateSelection;
    }

    private void OnDisable()
    {
        NotationPlaybackManager.Instance.OnNotationChanged += (value1, value2) => UpdateHighlight(value2);
        NotationSelectionManager.SelectedChanged -= UpdateSelection;
    }

    void Start()
    {
        DisplayNotations();
    }

    /// <summary>
    /// 显示谱面上的所有减字。
    /// </summary>
    private void DisplayNotations()
    {
        string trackName = "秋风词";
        string performer = "龚一";
        
        var positionPages = DataManager.Instance.GetPositionPages();
        foreach (var page in positionPages)
        {
            foreach (var notation in page.notations)
            {
                CreateNotationUI(notation);
            }
        }

        ScrollToCenter(0, 0f);
    }

    /// <summary>
    /// 创建单个减字 UI 元素。
    /// </summary>
    private void CreateNotationUI(NotationPositionData notation)
    {
        Image prefab = notation.type == "Main" ? mainNotationPrefab : continuationNotationPrefab;
        Image notationImage = Instantiate(prefab, notationContainer);
        RectTransform rectTransform = notationImage.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = new Vector2(notation.x * ImageResizer.Instance.GetScale(), 
                                                  notation.y * ImageResizer.Instance.GetScale());
        notationImages[notation.id] = notationImage;

        // 初始化为隐藏状态
        SetNotationState(notation.id, NotationState.Hidden);
        
        if (notation.type == "Main")
        {
            // 动态添加 NotationInteractionHandler
            NotationInteractionHandler interactionHandler = notationImage.gameObject.AddComponent<NotationInteractionHandler>();
            interactionHandler.notationId = notation.id; // 传递当前减字 ID
        }
    }

    /// <summary>
    /// 设置减字的状态。
    /// </summary>
    public void SetNotationState(int id, NotationState state)
    {
        if (!notationImages.ContainsKey(id))
            return;

        Image image = notationImages[id];

        switch (state)
        {
            case NotationState.Hidden:
                if (id != currentHighlightedId)
                {
                    image.color = new Color(36 / 255f, 41 / 255f, 41 / 255f, 0 / 255f);
                }
                break;
            case NotationState.Selected:
                if (id != currentHighlightedId)
                {
                    image.color = new Color(36 / 255f, 41 / 255f, 41 / 255f, 255 / 255f);
                }
                break;
            case NotationState.Highlighted:
                image.color = new Color(176 / 255f, 80 / 255f, 55 / 255f, 255 / 255f);
                break;
        }
    }

    /// <summary>
    /// 更新高亮状态。
    /// </summary>
    public void UpdateHighlight(int newHighlightedId)
    {
        var preHighlightId = currentHighlightedId;
        currentHighlightedId = newHighlightedId;
        // 取消之前的高亮
        SetNotationState(preHighlightId, 
        (preHighlightId >= rangeStart && preHighlightId <= rangeEnd) ? NotationState.Selected: NotationState.Hidden);

        // 更新为新的高亮状态
        SetNotationState(currentHighlightedId, NotationState.Highlighted);

        ScrollToCenter(currentHighlightedId);
    }

    public void ScrollToCenter(int id, float time = 0.5f)
    {
        if (id >= 0 && id < notationImages.Count)
        {
            ScrollManager.Instance.ScrollToCenter(notationImages[id].GetComponent<RectTransform>(), time);
        }
    }

    /// <summary>
    /// 更新选择状态。
    /// </summary>
    public void UpdateSelection(int start, int end)
    {
        for (int i = rangeStart; i <= rangeEnd; i++)
        {
            SetNotationState(i, NotationState.Hidden);
        }
        for (int i = start; i <= end; i++)
        {
            SetNotationState(i, NotationState.Selected);
        }

        rangeStart = start;
        rangeEnd = end;
    }
}
