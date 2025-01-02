using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public GameObject levelButtonPrefab; // 级别按钮预制体
    public Transform contentParent;      // Scroll View 的 Content

    string currentCategory = "00";
    private TrackData defaultTrack; // 当前默认选中的 Track ID

    private void OnEnable()
    {
        // 订阅事件
        EventManager.OnCategoryLoaded += InitCategory;
    }

    private void OnDisable()
    {
        // 取消订阅事件
        EventManager.OnCategoryLoaded -= InitCategory;
    }

    private void InitCategory()
    {
        // 设置默认选中的 Track
        SetDefaultTrack();

        LoadCategory();
    }

    private void LoadCategory()
    {
        currentCategory = defaultTrack.id.Substring(0, 2);

        var levels = CategoryManager.Instance.GetLevels(currentCategory);
        foreach (var level in levels)
        {
            CreateLevel(level);
        }
    }

    public void CreateLevel(LevelData level)
    {
        // 创建级别按钮
        GameObject levelButton = Instantiate(levelButtonPrefab, contentParent);
        //levelButton.transform.SetParent(contentParent, false);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());

        levelButton.GetComponentInChildren<TextMeshProUGUI>().text = level.name;

        // 设置折叠/展开逻辑
        LevelButtonController controller = levelButton.GetComponent<LevelButtonController>();
        controller.InitializeTracks(level.tracks);
        
        if (level.id == defaultTrack.id.Substring(3, 2))
        {
            controller.ToggleExpand();
            controller.SimulateTrackClick(defaultTrack.id);
        }
    }

    private void SetDefaultTrack()
    {
        var defaultTrackId = PlayerPrefs.GetString("SelectedTrackId", null);

        if (!string.IsNullOrEmpty(defaultTrackId))
        {
            defaultTrack = CategoryManager.Instance.GetTrackDataById(defaultTrackId);
            if(defaultTrack != null)
            {
                return;
            }
        }

        var levels = CategoryManager.Instance.GetLevels(currentCategory);
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("No levels available for selection.");
            return;
        }

        var firstLevel = levels[0];
        if (firstLevel.tracks == null || firstLevel.tracks.Count == 0)
        {
            Debug.LogWarning("No tracks available in the first level.");
            return;
        }
        
        defaultTrack = firstLevel.tracks[0];

        Debug.Log($"Default track set: {defaultTrack.name}");
    }
}
