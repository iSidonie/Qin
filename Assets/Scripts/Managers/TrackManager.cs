using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public GameObject levelButtonPrefab; // ����ťԤ����
    public Transform contentParent;      // Scroll View �� Content

    string currentCategory = "00";
    private TrackData defaultTrack; // ��ǰĬ��ѡ�е� Track ID

    private void OnEnable()
    {
        // �����¼�
        EventManager.OnCategoryLoaded += InitCategory;
    }

    private void OnDisable()
    {
        // ȡ�������¼�
        EventManager.OnCategoryLoaded -= InitCategory;
    }

    private void InitCategory()
    {
        // ����Ĭ��ѡ�е� Track
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
        // ��������ť
        GameObject levelButton = Instantiate(levelButtonPrefab, contentParent);
        //levelButton.transform.SetParent(contentParent, false);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());

        levelButton.GetComponentInChildren<TextMeshProUGUI>().text = level.name;

        // �����۵�/չ���߼�
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
