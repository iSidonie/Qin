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

    // �����ֵ�
    private Dictionary<int, NotationAudioData> audioCache = new Dictionary<int, NotationAudioData>();
    private Dictionary<int, NotationPositionData> positionCache = new Dictionary<int, NotationPositionData>();
    private Dictionary<int, List<int>> parentMap = new Dictionary<int, List<int>>();
    private Dictionary<int, List<int>> positionToAudioMap = new Dictionary<int, List<int>>();
    
    // �洢 sections �� pages ��Ϣ
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
    /// ���ز����� Audio.json �� Position.json �ļ���
    /// </summary>
    public void LoadData(string trackName, string performer)
    {
        StartCoroutine(LoadAudioData(trackName, performer));
        StartCoroutine(LoadPositionData(trackName, performer));
        // Debug.Log("Data loaded successfully.");
    }

    /// <summary>
    /// ���� Audio.json �ļ���������
    /// </summary>
    private IEnumerator LoadAudioData(string trackName, string performer)
    {
        string jsonFileName = $"json/{trackName}_{performer}_Audio.json";
        string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        //// ��׿��Ҫ���⴦��·��
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

            // ���� Audio ����
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
    /// ���� Position.json �ļ���������
    /// </summary>
    private IEnumerator LoadPositionData(string trackName, string performer)
    {
        string jsonFileName = $"json/{trackName}_{performer}_Position.json";
        string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        //// ��׿��Ҫ���⴦��·��
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
            
            // ���� Position ����
            positionPages = positionData.pages;
            foreach (var page in positionPages)
            {
                foreach (var notation in page.notations)
                {
                    positionCache[notation.id] = notation;

                    // ���� parentMap����¼���� Continuation Notation
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
    /// ��ȡ Audio ���ݡ�
    /// </summary>
    public NotationAudioData GetAudioDataById(int id)
    {
        return audioCache.TryGetValue(id, out var audioData) ? audioData : null;
    }

    /// <summary>
    /// ��ȡ Position ���ݡ�
    /// </summary>
    public NotationPositionData GetPositionDataById(int id)
    {
        return positionCache.TryGetValue(id, out var positionData) ? positionData : null;
    }

    /// <summary>
    /// ��ȡ Main Notation ������ Continuation Notations��
    /// </summary>
    public List<int> GetSubNotations(int parentId)
    {
        return parentMap.TryGetValue(parentId, out var subList) ? subList : new List<int>();
    }

    /// <summary>
    /// ��ȡ������Ƶ�ֶ����ݡ�
    /// </summary>
    public List<SectionData> GetAudioSections()
    {
        return audioSections;
    }

    /// <summary>
    /// ��ȡ����ҳ�����ݡ�
    /// </summary>
    public List<PageData> GetPositionPages()
    {
        return positionPages;
    }

    /// <summary>
    /// ��ȡ���м�������Ƶ�ж�Ӧ��λ�á�
    /// </summary>
    public List<int> GetPositionToAudioNotation(int id)
    {
        return positionToAudioMap[id];
    }
}
