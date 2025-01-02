using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrackItemController : MonoBehaviour
{
    public Image background; // 背景 Image，用于改变选中状态颜色
    public TextMeshProUGUI trackNameText;
    public TextMeshProUGUI artistNameText;
    public RectTransform content; // 引用 Content 容器的 RectTransform
    private TrackData trackData;
    private bool isDownload;
    private bool isSelected = false; // 当前是否被选中
    private static TrackItemController currentSelected;

    private static readonly Color selectedColor = new Color((255 * 253)/(255 * 255f), (250 * 250)/ (255 * 255f), (247 * 239)/ (255 * 255f)); // 选中状态颜色
    private static readonly Color defaultColor = new Color(255 / 255f, 250 / 255f, 239 / 255f); // 默认状态颜色

    public void SetTrackData(TrackData data)
    {
        trackData = data;
        trackNameText.text = data.name;
        artistNameText.text = data.artist;
    }

    public void OnDownloadClick()
    {
        Debug.Log($"Downloading track: {trackData.name}");
        // 执行下载逻辑
    }

    public void OnDeleteClick()
    {
        Debug.Log($"Deleting track: {trackData.name}");
        // 执行删除逻辑
    }

    public void OnItemClick()
    {
        //// 更新选中状态，通知父级

        // 如果已经是选中状态，不做处理
        if (isSelected) return;

        // 取消之前选中的项
        if (currentSelected != null)
        {
            currentSelected.Deselect();
        }

        // 设置当前项为选中状态
        Select();

        EventManager.OnTrackSelected?.Invoke(trackData); // 通知全局事件系统
    }

    /// <summary>
    /// 设置当前项为选中状态
    /// </summary>
    private void Select()
    {
        //Debug.Log("change selectedColor");
        isSelected = true;
        background.color = selectedColor; // 改变背景颜色
        currentSelected = this; // 更新全局选中项

        // 打印或处理选中数据
        Debug.Log($"Selected Track: {trackNameText.text} by {artistNameText.text}");
    }

    /// <summary>
    /// 取消选中状态
    /// </summary>
    private void Deselect()
    {
        isSelected = false;
        background.color = defaultColor; // 恢复默认颜色
    }

    public void RefreshContentLayout()
    {
        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            //// 强制刷新 TMP 内容
            //trackNameText.ForceMeshUpdate();
            //artistNameText.ForceMeshUpdate();
        }
    }

    public TrackData GetTrackData()
    {
        return trackData; 
    }
}