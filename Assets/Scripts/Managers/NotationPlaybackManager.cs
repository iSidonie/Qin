using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class NotationPlaybackManager : MonoBehaviour
{
    public event System.Action<int, int> OnNotationChanged; // ֪ͨ��ǰ���ֱ䶯

    private int currentIndex = -1;
    private float notationStartTime = 0;
    private float notationEndTime = 0;
    private List<NotationAudioData> notations;

    private static NotationPlaybackManager _instance;
    public static NotationPlaybackManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NotationPlaybackManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(NotationPlaybackManager));
                    _instance = obj.AddComponent<NotationPlaybackManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    void OnEnable()
    {
        AudioPlayer.AudioTimeUpdated += UpdateCurrentNotation;
        ABLoopManager.Instance.OnLoopEnd += (value1, value2) => PlayAtNotation(value1);
        EventManager.OnTrackDataLoaded += LoadNotations;
    }

    void OnDisable()
    {
        AudioPlayer.AudioTimeUpdated -= UpdateCurrentNotation;
        ABLoopManager.Instance.OnLoopEnd -= (value1, value2) => PlayAtNotation(value1);
        EventManager.OnTrackDataLoaded -= LoadNotations;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    

    /// <summary>
    /// ������Ƶ�ͼ������ݡ�
    /// </summary>
    private void LoadNotations()
    {
        notations = new List<NotationAudioData>();
        
        // �������м��ֵ��б�
        var aduioSections = DataManager.Instance.GetAudioSections();
        foreach (var section in aduioSections)
        {
            notations.AddRange(section.notations);
        }
        notationStartTime = 0.0f;
        notationEndTime = notations[0].time; ;
    }

    /// <summary>
    /// ���µ�ǰ���ڲ��ŵļ��֡�
    /// </summary>
    private void UpdateCurrentNotation(float currentTime, float totalTime)
    {
        if (notations != null && !isInNotationTimeRange(currentTime))
        {
            currentIndex = ++currentIndex == notations.Count? -1: currentIndex;

            UpdateNotationTimeRange(totalTime);
        
            if (currentIndex >= notations.Count || !isInNotationTimeRange(currentTime))
            {
                currentIndex = FindNotationIndex(currentTime);
                UpdateNotationTimeRange(totalTime);
            }
            
            OnNotationChanged?.Invoke(currentIndex, currentIndex >= 0? notations[currentIndex].notationIndex: -1); // �����¼�
        }
    }

    /// <summary>
    /// ���µ�ǰ���ֵ�ʱ�䷶Χ��
    /// </summary>
    private void UpdateNotationTimeRange(float totalTime)
    {
        if (currentIndex == -1)
        {
            notationStartTime = 0.0f;
            notationEndTime = notations[0].time;
        }
        else if (currentIndex == notations.Count - 1)
        {
            notationStartTime = notations[currentIndex].time;
            notationEndTime = totalTime;
        }
        else
        {
            notationStartTime = notations[currentIndex].time;
            notationEndTime = notations[currentIndex + 1].time;
        }
    }

    /// <summary>
    /// ��ǰʱ���Ƿ��ڵ�ǰ����ʱ�䷶Χ�ڡ�
    /// </summary>
    private bool isInNotationTimeRange(float currentTime)
    {
        // Debug.Log($"start {notationStartTime}, end {notationEndTime}");
        return currentTime >= notationStartTime && currentTime < notationEndTime;
    }
    
    /// <summary>
    /// ��ת���ŵ�ָ�����֡�
    /// </summary>
    public void PlayAtNotation(int id)
    {
        if (id >= 0 && id < notations.Count)
        {
            var notation = notations[id];
            // ʹ�� AudioPlayer �� SeekToTime ����������ת����
            AudioPlayer.Instance.SeekToTime(notation.time);

            // ���µ�ǰ����
            currentIndex = id;
            Debug.Log($"PlayAtNotation {currentIndex}");

            OnNotationChanged?.Invoke(currentIndex, notations[currentIndex].notationIndex); // �����¼�
        }
    }

    /// <summary>
    /// ʹ�ö��ַ����ҵ�ǰʱ���Ӧ�ļ���������
    /// </summary>
    private int FindNotationIndex(float time)
    {
        Debug.Log($"FindNotationIndex {currentIndex}");

        if (notations == null || notations.Count == 0) return -1;

        // ���ȼ��ʱ���Ƿ�С����Сֵ
        if (time < notations[0].time) return -1;

        int left = 0;
        int right = notations.Count - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (notations[mid].time == time)
            {
                return mid;
            }
            else if (notations[mid].time < time)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        // �������һ��С�ڵ��ڵ�ǰʱ��ļ�������
        return Mathf.Max(0, left - 1);
    }
}
