using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum NotationState
{
    Hidden,      // ����״̬��͸����Ϊ 0��
    Selected,    // �ֶ�ѡ��״̬��Ĭ����ɫ��͸����Ϊ 1��
    Highlighted  // ���Ÿ���״̬��������ɫ��͸����Ϊ 1��
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
        DisplayNotations();
    }

    /// <summary>
    /// ��ʾ�����ϵ����м��֡�
    /// </summary>
    private void DisplayNotations()
    {
        string trackName = "����";
        string performer = "��һ";
        
        var positionPages = DataManager.Instance.GetPositionPages();
        foreach (var page in positionPages)
        {
            foreach (var notation in page.notations)
            {
                CreateNotationUI(notation);
            }
        }
    }

    /// <summary>
    /// ������������ UI Ԫ�ء�
    /// </summary>
    private void CreateNotationUI(NotationPositionData notation)
    {
        Image prefab = notation.type == "Main" ? mainNotationPrefab : continuationNotationPrefab;
        Image notationImage = Instantiate(prefab, notationContainer);
        RectTransform rectTransform = notationImage.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = new Vector2(notation.x * ImageResizer.Instance.GetScale(), 
                                                  notation.y * ImageResizer.Instance.GetScale());
        notationImages[notation.id] = notationImage;

        // ��ʼ��Ϊ����״̬
        SetNotationState(notation.id, NotationState.Hidden);
        
        if (notation.type == "Main")
        {
            //// ��ȡ������ ExButton ���
            //ExButton exButton = notationImage.GetComponent<ExButton>();
            //if (exButton == null)
            //{
            //    exButton = notationImage.gameObject.AddComponent<ExButton>();
            //}

            // ��̬���� NotationInteractionHandler
            NotationInteractionHandler interactionHandler = notationImage.gameObject.AddComponent<NotationInteractionHandler>();
            interactionHandler.notationId = notation.id; // ���ݵ�ǰ���� ID
        }
    }

    /// <summary>
    /// ���ü��ֵ�״̬��
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
                    SetImageAlpha(image, 1f);
                    image.color = new Color(36 / 255f, 41 / 255f, 41 / 255f, 255 / 255f);
                }
                break;
            case NotationState.Highlighted:
                SetImageAlpha(image, 1f);
                image.color = new Color(176 / 255f, 80 / 255f, 55 / 255f, 255 / 255f);
                break;
        }
    }

    /// <summary>
    /// ���� Image ��͸���ȡ�
    /// </summary>
    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    /// <summary>
    /// ���¸���״̬��
    /// </summary>
    public void UpdateHighlight(int newHighlightedId)
    {
        // ȡ��֮ǰ�ĸ���
        var preHighlightedId = currentHighlightedId;
        currentHighlightedId = newHighlightedId;
        if (preHighlightedId != -1 && preHighlightedId != newHighlightedId)
        {
            SetNotationState(preHighlightedId, 
                NotationSelectionManager.Instance.QueryNotationState(preHighlightedId) ? NotationState.Selected: NotationState.Hidden);
        }

        // ����Ϊ�µĸ���״̬
        SetNotationState(currentHighlightedId, NotationState.Highlighted);
        //Debug.Log(currentHighlightedId);
        ScrollManager.Instance.ScrollToCenter(notationImages[currentHighlightedId].GetComponent<RectTransform>());
    }
}