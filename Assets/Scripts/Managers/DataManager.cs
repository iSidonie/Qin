using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DataManager : MonoBehaviour
{
    private static DataManager _instance;
    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DataManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(DataManager));
                    _instance = obj.AddComponent<DataManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    // 缓存字典
    private Dictionary<int, NotationAudioData> audioCache = new Dictionary<int, NotationAudioData>();
    private Dictionary<int, NotationPositionData> positionCache = new Dictionary<int, NotationPositionData>();
    private Dictionary<int, List<int>> parentMap = new Dictionary<int, List<int>>();
    private Dictionary<int, List<int>> positionToAudioMap = new Dictionary<int, List<int>>();
    
    // 存储 sections 和 pages 信息
    private List<SectionData> audioSections;
    private List<PageData> positionPages;
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadData("QiuFengCi", "GongYi");
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 加载并解析 Audio.json 和 Position.json 文件。
    /// </summary>
    public void LoadData(string trackName, string performer)
    {
        StartCoroutine(LoadAudioData(trackName, performer));
        StartCoroutine(LoadPositionData(trackName, performer));
        // Debug.Log("Data loaded successfully.");
    }

    /// <summary>
    /// 加载 Audio.json 文件并解析。
    /// </summary>
    private IEnumerator LoadAudioData(string trackName, string performer)
    {
        string jsonFileName = $"json/{trackName}_{performer}_Audio.json";
        string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        //// 安卓需要特殊处理路径
        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    jsonPath = "jar:file://" + jsonPath;
        //}

        UnityWebRequest uwr = UnityWebRequest.Get(jsonPath);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            string jsonContent = uwr.downloadHandler.text;
            AudioDataStructure audioData = JsonUtility.FromJson<AudioDataStructure>(jsonContent);

            // 缓存 Audio 数据
            audioSections = audioData.sections;
            foreach (var section in audioSections)
            {
                foreach (var notation in section.notations)
                {
                    audioCache[notation.id] = notation;

                    if (!positionToAudioMap.ContainsKey(notation.notationIndex))
                    {
                        positionToAudioMap[notation.notationIndex] = new List<int>();
                    }
                    positionToAudioMap[notation.notationIndex].Add(notation.id);
                }
            }
        }
        else
        {
            Debug.LogError($"Audio JSON file not found: {jsonPath}");
        }
    }

    /// <summary>
    /// 加载 Position.json 文件并解析。
    /// </summary>
    private IEnumerator LoadPositionData(string trackName, string performer)
    {
        string jsonFileName = $"json/{trackName}_{performer}_Position.json";
        string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        //// 安卓需要特殊处理路径
        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    jsonPath = "jar:file://" + jsonPath;
        //}

        UnityWebRequest uwr = UnityWebRequest.Get(jsonPath);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            string jsonContent = uwr.downloadHandler.text;
            PositionDataStructure positionData = JsonUtility.FromJson<PositionDataStructure>(jsonContent);
            
            // 缓存 Position 数据
            positionPages = positionData.pages;
            foreach (var page in positionPages)
            {
                foreach (var notation in page.notations)
                {
                    positionCache[notation.id] = notation;

                    // 构建 parentMap，记录所有 Continuation Notation
                    if (notation.type == "Continuation" && notation.parentId != -1)
                    {
                        if (!parentMap.ContainsKey(notation.parentId))
                        {
                            parentMap[notation.parentId] = new List<int>();
                        }
                        parentMap[notation.parentId].Add(notation.id);
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Position JSON file not found: {jsonPath}");
        }
    }

    /// <summary>
    /// 获取 Audio 数据。
    /// </summary>
    public NotationAudioData GetAudioDataById(int id)
    {
        return audioCache.TryGetValue(id, out var audioData) ? audioData : null;
    }

    /// <summary>
    /// 获取 Position 数据。
    /// </summary>
    public NotationPositionData GetPositionDataById(int id)
    {
        return positionCache.TryGetValue(id, out var positionData) ? positionData : null;
    }

    /// <summary>
    /// 获取 Main Notation 的所有 Continuation Notations。
    /// </summary>
    public List<int> GetSubNotations(int parentId)
    {
        return parentMap.TryGetValue(parentId, out var subList) ? subList : new List<int>();
    }

    /// <summary>
    /// 获取所有音频分段数据。
    /// </summary>
    public List<SectionData> GetAudioSections()
    {
        return audioSections;
    }

    /// <summary>
    /// 获取所有页面数据。
    /// </summary>
    public List<PageData> GetPositionPages()
    {
        return positionPages;
    }

    /// <summary>
    /// 获取谱中减字在音频中对应的位置。
    /// </summary>
    public List<int> GetPositionToAudioNotation(int id)
    {
        return positionToAudioMap[id];
    }
}
