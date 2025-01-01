using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

public class LevelButtonController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform arrowImage; // 箭头图标的 RectTransform
    public RectTransform songList; // 曲目列表容器
    public GameObject trackItemPrefab; // 曲目项预制体

    [Header("Animation Settings")]
    public float expandDuration = 0.5f; // 展开/折叠动画时长
    public float itemHeight = 100f; // 每个曲目项的高度
    public float spacing = 5f; // 曲目项之间的间距
    public float collapsedHeight = 0f; // 折叠时的高度

    private bool isExpanded = false; // 当前展开状态
    private int lastActivatedIndex = -1; // 上次激活的子项索引

    /// <summary>
    /// 切换展开/折叠状态
    /// </summary>
    public void ToggleExpand()
    {
        isExpanded = !isExpanded;

        // 动画旋转箭头
        arrowImage.DORotate(Vector3.forward * (isExpanded ? -90 : 0), expandDuration);

        // 动态计算展开后的高度
        float expandedHeight = songList.childCount * itemHeight + (songList.childCount - 1) * spacing;

        if (isExpanded)
        {
            // 动态展开 SongList
            songList.DOSizeDelta(new Vector2(songList.sizeDelta.x, expandedHeight), expandDuration)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(UpdateVisibleItems)
                .OnComplete(() =>
                 {
                     // 动画结束后强制重建整个布局
                     LayoutRebuilder.ForceRebuildLayoutImmediate(songList);
                 });

        }
        else
        {
            // 折叠 SongList
            songList.DOSizeDelta(new Vector2(songList.sizeDelta.x, 0), expandDuration)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(HideVisibleItems) // 每次更新时逐步隐藏子项
                .OnComplete(() =>
                {
                    foreach (Transform item in songList)
                    {
                        item.gameObject.SetActive(false); // 隐藏所有子项
                    }
                    lastActivatedIndex = -1; // 重置激活索引
                });
        }

        // 强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(songList);
    }

    /// <summary>
    /// 每次容器高度更新时，激活新的子项
    /// </summary>
    private void UpdateVisibleItems()
    {
        float currentHeight = songList.sizeDelta.y;

        // 根据当前高度计算应激活的子项数量
        int visibleItemCount = Mathf.FloorToInt((currentHeight + spacing) / (itemHeight + spacing));

        // 激活尚未显示的子项
        for (int i = lastActivatedIndex + 1; i < visibleItemCount && i < songList.childCount; i++)
        {
            Transform item = songList.GetChild(i);
            item.gameObject.SetActive(true); // 显示子项

            // 刷新子项的 Content 布局
            TrackItemController controller = item.GetComponent<TrackItemController>();
            if (controller != null)
            {
                controller.RefreshContentLayout();
            }

            // 透明度渐变动画
            CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(1, 0.3f);

            lastActivatedIndex = i; // 更新激活索引
        }
    }

    /// <summary>
    /// 每次容器高度更新时，隐藏可见的子项
    /// </summary>
    private void HideVisibleItems()
    {
        float currentHeight = songList.sizeDelta.y;

        // 根据当前高度计算应隐藏的子项数量
        int visibleItemCount = Mathf.CeilToInt((currentHeight + spacing) / (itemHeight + spacing));

        // 逐步隐藏超出高度范围的子项
        for (int i = lastActivatedIndex; i >= visibleItemCount && i >= 0; i--)
        {
            Transform item = songList.GetChild(i);

            // 透明度渐变动画
            CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(0, 0.01f).OnComplete(() =>
            {
                item.gameObject.SetActive(false); // 动画结束后禁用子项
            });

            lastActivatedIndex = i - 1; // 更新激活索引
        }
    }

    /// <summary>
    /// 动态生成曲目项
    /// </summary>
    public void AddTrackItem(TrackData track)
    {
        // 实例化曲目项预制体
        GameObject trackItem = Instantiate(trackItemPrefab, songList);
        trackItem.SetActive(false); // 初始隐藏

        // 设置 UI 信息
        TrackItemController controller = trackItem.GetComponent<TrackItemController>();
        controller.SetTrackData(track);

        //// 设置下载/删除按钮逻辑
        //var button = trackItem.GetComponentInChildren<Button>();
        //if (button != null)
        //{
        //    button.onClick.AddListener(() => OnTrackActionClicked(trackName, artist));
        //}
    }

    public void SimulateTrackClick(string trackId)
    {
        foreach (Transform item in songList)
        {
            var trackController = item.GetComponent<TrackItemController>();
            if (trackController != null && trackController.GetTrackData().id == trackId)
            {
                trackController.OnItemClick(); // 模拟点击
                trackController.RefreshContentLayout();
                Debug.Log($"Simulated click on track: {trackId}");
                return;
            }
        }

        Debug.LogWarning($"Track with ID {trackId} not found.");
    }

    ///// <summary>
    ///// 曲目项的下载/删除按钮点击事件
    ///// </summary>
    //private void OnTrackActionClicked(string trackName, string artist)
    //{
    //    Debug.Log($"Button clicked for track: {trackName}, artist: {artist}");
    //    // 实现下载或删除逻辑，例如切换按钮状态或触发相关事件
    //}

    /// <summary>
    /// 初始化曲目列表
    /// </summary>
    public void InitializeTracks(List<TrackData> tracks)
    {
        // 清空当前列表
        foreach (RectTransform child in songList)
        {
            Destroy(child.gameObject);
        }

        // 添加新曲目
        foreach (var track in tracks)
        {
            AddTrackItem(track);
        }

        // 初始化时折叠
        songList.sizeDelta = new Vector2(songList.sizeDelta.x, collapsedHeight);
        isExpanded = false;
        lastActivatedIndex = -1;
    }
}
