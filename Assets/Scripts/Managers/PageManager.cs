using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PageManager : MonoBehaviour
{
    private static PageManager _instance;
    public static PageManager Instance => _instance;

    public RectTransform content; // ScrollView �� Content
    public GameObject pagePrefab; // ҳ���Ԥ����
    public ScrollRect scrollView; // ScrollRect ���
    public float scrollDuration = 0.3f; // ��������ʱ��
    
    private List<GameObject> pages = new List<GameObject>(); // ��ǰ�����ҳ
    private int currentPage = 0; // ��ǰ��ʾ��ҳ
    private int totalPageCount = 0; // ��ҳ��

    private bool isSliding = false; // �Ƿ����ڻ�����

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
        // �����¼�
        EventManager.OnTrackDataLoaded += InitializePages;
    }

    private void OnDisable()
    {
        // ȡ�������¼�
        EventManager.OnTrackDataLoaded -= InitializePages;
    }

    /// <summary>
    /// ��ʼ��ҳ�滺�档
    /// </summary>
    private void InitializePages()
    {
        totalPageCount = DataManager.Instance.GetPositionPages().Count;
        // ��յ�ǰҳ��
        foreach (var page in pages)
        {
            Destroy(page);
        }
        pages.Clear();

        // ������ҳ
        for (int i = 0; i < 3; i++)
        {
            var page = Instantiate(pagePrefab, content);
            pages.Add(page);
            // page.SetActive(false); // ��ʼʱ����
            UpdatePageContent(page, i - 1);
        }

        AdjustContentSize();

        // ȷ����ǰҳ���м�
        PositionPages();
    }

    /// <summary>
    /// ����ҳ�����ݡ�
    /// </summary>
    private void UpdatePageContent(GameObject page, int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= totalPageCount)
        {
            page.SetActive(false);
            return;
        }
        
        ContentManager.Instance.UpdatePageContent(page, pageIndex);
        page.SetActive(true);
    }

    /// <summary>
    /// ����ҳ��λ�á�
    /// </summary>
    private void PositionPages()
    {
        float viewportWidth = scrollView.GetComponent<RectTransform>().rect.width;

        pages[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-viewportWidth, 0);
        pages[1].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        pages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(viewportWidth, 0);
    }


    /// <summary>
    /// �л���ĳһҳ
    /// </summary>
    public void SlideToPage(int pageIndex)
    {
        if (pageIndex == currentPage + 1)
        {
            currentPage = pageIndex;
            ShiftPagesLeft();
        }
        else if (pageIndex == currentPage - 1)
        {
            currentPage = pageIndex;
            ShiftPagesRight();
        }
        else if (pageIndex != currentPage)
        {
            currentPage = pageIndex;

            for (int i = 0; i < 3; i++)
            {
                UpdatePageContent(pages[i], currentPage + i - 1);
            }
        }
    }

    /// <summary>
    /// ��������һҳ��
    /// </summary>
    public void SlideToPreviousPage()
    {
        if (currentPage == 0)
        {
            ResetPagePosition();
            return;
        };

        isSliding = true;

        currentPage--;
        Debug.Log(currentPage);

        // ��������
        float verticalPos = scrollView.normalizedPosition.y;
        scrollView.DOHorizontalNormalizedPos(0f, scrollDuration).OnComplete(() =>
        {
            ShiftPagesRight();
            isSliding = false;
            scrollView.normalizedPosition = new Vector2(0.5f, verticalPos);
        });
    }

    /// <summary>
    /// ��������һҳ��
    /// </summary>
    public void SlideToNextPage()
    {
        if (currentPage == totalPageCount - 1)
        {
            ResetPagePosition();
            return;
        };

        isSliding = true;

        currentPage++;
        Debug.Log(currentPage);

        // ��������
        float verticalPos = scrollView.normalizedPosition.y;
        scrollView.DOHorizontalNormalizedPos(1f, scrollDuration).OnComplete(() =>
        {
            ShiftPagesLeft();
            isSliding = false;
            scrollView.normalizedPosition = new Vector2(0.5f, verticalPos);
        });
    }

    /// <summary>
    /// ��λҳ��λ�á�
    /// </summary>
    public void ResetPagePosition()
    {
        isSliding = true;
        scrollView.DOHorizontalNormalizedPos(0.5f, scrollDuration).OnComplete(() =>
        {
            isSliding = false;
        });
    }

    /// <summary>
    /// ҳ�����ơ�
    /// </summary>
    private void ShiftPagesLeft()
    {
        GameObject tempPage = pages[0];
        pages[0] = pages[1];
        pages[1] = pages[2];
        UpdatePageContent(tempPage, currentPage + 1);

        pages[2] = tempPage;

        PositionPages();
    }

    /// <summary>
    /// ҳ�����ơ�
    /// </summary>
    private void ShiftPagesRight()
    {
        GameObject tempPage = pages[2];
        pages[2] = pages[1];
        pages[1] = pages[0];
        UpdatePageContent(tempPage, currentPage - 1);

        pages[0] = tempPage;

        PositionPages();
    }

    public void AdjustContentSize()
    {
        if (pages.Count == 0) return;

        // ��������ҳ��Ŀ��һ��
        RectTransform firstPageRect = pages[0].GetComponent<RectTransform>();
        if (firstPageRect == null) return;

        float contentWidth = pages.Count * firstPageRect.sizeDelta.x;
        float contentHeight = firstPageRect.sizeDelta.y;

        content.sizeDelta = new Vector2(contentWidth, contentHeight);
    }
}
