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

    public string sheetFileName = "";
    
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
        EventManager.OnTrackSelected += LoadTrackData;
    }

    private void OnDisable()
    {
        // ȡ�������¼�
        EventManager.OnTrackSelected -= LoadTrackData;
    }

    private void LoadTrackData(TrackData trackData)
    {
        sheetFileName = trackData.sheetFile; 

        Debug.Log($"Loading track Json: {trackData.name}");
        // ������Ƶ����������
        StartCoroutine(LoadJsonData(trackData.positionFile, trackData.audioFile));
    }

    private IEnumerator LoadJsonData(string positionPath, string audioPath)
    {
        Debug.Log($"Loading track Json: {positionPath}, {audioPath}");

        yield return LocalFileManager.Instance.LoadJsonAsync(positionPath, ParsePositionData);
        yield return LocalFileManager.Instance.LoadJsonAsync(audioPath, ParseAudioData);

        EventManager.OnTrackDataLoaded?.Invoke();
    }

    /// <summary>
    /// ���� Audio.json �ļ���������
    /// </summary>
    private void ParseAudioData(string json)
    {
        AudioDataStructure audioData = JsonUtility.FromJson<AudioDataStructure>(json);

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

    /// <summary>
    /// ���� Position.json �ļ���������
    /// </summary>
    private void ParsePositionData(string json)
    {
        PositionDataStructure positionData = JsonUtility.FromJson<PositionDataStructure>(json);
            
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
    /// ��ȡ����ҳ�����ݡ�
    /// </summary>
    public List<NotationPositionData> GetPositionByPage(int pageNumber)
    {
        return (pageNumber >= 0 && pageNumber < positionPages.Count)? 
        positionPages[pageNumber].notations: new List<NotationPositionData>();
    }

    /// <summary>
    /// ��ȡ����ҳ�����ݡ�
    /// </summary>
    public int GetPageByPosition(int id)
    {
        return positionCache[id].page;
    }

    /// <summary>
    /// ��ȡ���м�������Ƶ�ж�Ӧ��λ�á�
    /// </summary>
    public List<int> GetPositionToAudioNotation(int id)
    {
        return positionToAudioMap[id];
    }

    public string GetSheetFileName()
    {
        return sheetFileName;
    }
}
