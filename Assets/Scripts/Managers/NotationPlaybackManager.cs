using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class NotationPlaybackManager : MonoBehaviour
{
    public event System.Action<int, int> OnNotationChanged; // 通知当前减字变动

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
    /// 加载音频和减字数据。
    /// </summary>
    private void LoadNotations()
    {
        notations = new List<NotationAudioData>();
        
        // 加载所有减字到列表
        var aduioSections = DataManager.Instance.GetAudioSections();
        foreach (var section in aduioSections)
        {
            notations.AddRange(section.notations);
        }
        notationStartTime = 0.0f;
        notationEndTime = notations[0].time; ;
    }

    /// <summary>
    /// 更新当前正在播放的减字。
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
            
            OnNotationChanged?.Invoke(currentIndex, currentIndex >= 0? notations[currentIndex].notationIndex: -1); // 触发事件
        }
    }

    /// <summary>
    /// 更新当前减字的时间范围。
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
    /// 当前时间是否在当前减字时间范围内。
    /// </summary>
    private bool isInNotationTimeRange(float currentTime)
    {
        // Debug.Log($"start {notationStartTime}, end {notationEndTime}");
        return currentTime >= notationStartTime && currentTime < notationEndTime;
    }
    
    /// <summary>
    /// 跳转播放到指定减字。
    /// </summary>
    public void PlayAtNotation(int id)
    {
        if (id >= 0 && id < notations.Count)
        {
            var notation = notations[id];
            // 使用 AudioPlayer 的 SeekToTime 方法进行跳转播放
            AudioPlayer.Instance.SeekToTime(notation.time);

            // 更新当前索引
            currentIndex = id;
            Debug.Log($"PlayAtNotation {currentIndex}");

            OnNotationChanged?.Invoke(currentIndex, notations[currentIndex].notationIndex); // 触发事件
        }
    }

    /// <summary>
    /// 使用二分法查找当前时间对应的减字索引。
    /// </summary>
    private int FindNotationIndex(float time)
    {
        Debug.Log($"FindNotationIndex {currentIndex}");

        if (notations == null || notations.Count == 0) return -1;

        // 首先检查时间是否小于最小值
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

        // 返回最后一个小于等于当前时间的减字索引
        return Mathf.Max(0, left - 1);
    }
}
