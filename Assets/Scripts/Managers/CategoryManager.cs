using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;

public class CategoryManager : MonoBehaviour
{
    public static CategoryManager Instance { get; private set; }

    private CategoryRoot categoryRoot;
    private bool isLoaded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        StartCoroutine(LoadCategoryData("category.json"));
    }

    private IEnumerator LoadCategoryData(string jsonFilePath)
    {
        yield return LocalFileManager.Instance.LoadJsonAsync(jsonFilePath, OnJsonLoaded);

        EventManager.OnCategoryLoaded?.Invoke();
    }

    private void OnJsonLoaded(string jsonContent)
    {
        if (!string.IsNullOrEmpty(jsonContent))
        {
            categoryRoot = JsonUtility.FromJson<CategoryRoot>(jsonContent);
            Debug.Log("Category data loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load Category data: JSON content is null or empty.");
        }
    }

    public bool IsLoaded() => isLoaded;

    public List<CategoryData> GetCategories()
    {
        return categoryRoot?.categories;
    }

    public List<LevelData> GetLevels(string categoryId)
    {
        var category = categoryRoot?.categories.Find(c => c.categoryId == categoryId);
        return category?.levels;
    }

    public List<TrackData> GetTracks(string categoryId, string levelId)
    {
        var levels = GetLevels(categoryId);
        var level = levels?.Find(l => l.levelId == levelId);
        return level?.tracks;
    }

    public TrackData GetTrackDataById(string trackId)
    {
        var tracks = GetTracks(trackId.Substring(0, 2), trackId.Substring(3, 2));
        foreach (var track in tracks){

            if (track.id == trackId)
            {
                return track;
            }
        }
        return null;
    }
}