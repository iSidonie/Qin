using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PageManager : MonoBehaviour
{
    private static PageManager _instance;
    public static PageManager Instance => _instance;

    public RectTransform content; // ScrollView 的 Content
    public GameObject pagePrefab; // 页面的预制体
    public ScrollRect scrollView; // ScrollRect 组件
    public float scrollDuration = 0.3f; // 滑动动画时间
    
    private List<GameObject> pages = new List<GameObject>(); // 当前缓存的页
    private int currentPage = 0; // 当前显示的页
    private int totalPageCount = 0; // 总页数

    private bool isSliding = false; // 是否正在滑动中

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
        // 订阅事件
        EventManager.OnTrackDataLoaded += InitializePages;
    }

    private void OnDisable()
    {
        // 取消订阅事件
        EventManager.OnTrackDataLoaded -= InitializePages;
    }

    /// <summary>
    /// 初始化页面缓存。
    /// </summary>
    private void InitializePages()
    {
        totalPageCount = DataManager.Instance.GetPositionPages().Count;
        // 清空当前页面
        foreach (var page in pages)
        {
            Destroy(page);
        }
        pages.Clear();

        // 创建三页
        for (int i = 0; i < 3; i++)
        {
            var page = Instantiate(pagePrefab, content);
            pages.Add(page);
            // page.SetActive(false); // 初始时隐藏
            UpdatePageContent(page, i - 1);
        }

        AdjustContentSize();

        // 确保当前页在中间
        PositionPages();
    }

    /// <summary>
    /// 更新页的内容。
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
    /// 设置页面位置。
    /// </summary>
    private void PositionPages()
    {
        float viewportWidth = scrollView.GetComponent<RectTransform>().rect.width;

        pages[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-viewportWidth, 0);
        pages[1].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        pages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(viewportWidth, 0);
    }


    /// <summary>
    /// 切换到某一页
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
    /// 滑动到上一页。
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

        // 动画滑动
        float verticalPos = scrollView.normalizedPosition.y;
        scrollView.DOHorizontalNormalizedPos(0f, scrollDuration).OnComplete(() =>
        {
            ShiftPagesRight();
            isSliding = false;
            scrollView.normalizedPosition = new Vector2(0.5f, verticalPos);
        });
    }

    /// <summary>
    /// 滑动到下一页。
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

        // 动画滑动
        float verticalPos = scrollView.normalizedPosition.y;
        scrollView.DOHorizontalNormalizedPos(1f, scrollDuration).OnComplete(() =>
        {
            ShiftPagesLeft();
            isSliding = false;
            scrollView.normalizedPosition = new Vector2(0.5f, verticalPos);
        });
    }

    /// <summary>
    /// 复位页面位置。
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
    /// 页面左移。
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
    /// 页面右移。
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

        // 假设所有页面的宽高一致
        RectTransform firstPageRect = pages[0].GetComponent<RectTransform>();
        if (firstPageRect == null) return;

        float contentWidth = pages.Count * firstPageRect.sizeDelta.x;
        float contentHeight = firstPageRect.sizeDelta.y;

        content.sizeDelta = new Vector2(contentWidth, contentHeight);
    }
}
