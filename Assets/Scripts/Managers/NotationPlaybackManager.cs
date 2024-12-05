using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class NotationPlaybackManager : MonoBehaviour
{
    private int currentIndex = -1;
    private float notationStartTime = 0;
    private float notationEndTime = 0;
    private List<NotationAudioData> notations;

    private bool isABLoop = false;
    private int loopStart = 0;
    private int loopEnd = 0;
    private int remainingLoops = -1; // -1 ��ʾ����ѭ��

    public int pauseDuration = 0; // ÿ��ѭ��ǰ��ͣ��ʱ��

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
    }

    void OnDisable()
    {
        AudioPlayer.AudioTimeUpdated -= UpdateCurrentNotation;
    }

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

    void Start()
    {
        LoadNotations();
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
        // Debug.Log("UpdateCurrentNotation");
        if (!isInNotationTimeRange(currentTime))
        {
            if (++currentIndex == notations.Count)
            {
                currentIndex = -1;
                StartCoroutine(PauseBeforeLoop());
            }

            UpdateNotationTimeRange(totalTime);

            if(currentIndex == -1)
            {
                return;
            }

            if (isInNotationTimeRange(currentTime))
            {
                NotationViewManager.Instance.UpdateHighlight(notations[currentIndex].notationIndex);
            }
            else
            {
                currentIndex = FindNotationIndex(currentTime);
                UpdateNotationTimeRange(totalTime);
                NotationViewManager.Instance.UpdateHighlight(notations[currentIndex].notationIndex);
            }
        }
        CheckABLoop();
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
        else if(currentIndex == notations.Count - 1)
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
    /// ����ABѭ�����ŵ���ʼ��������㡣
    /// </summary>
    public void SetLoopRange(int start, int end)
    {
        loopStart = start;
        loopEnd = end;
    }

    /// <summary>
    /// ��ת���ŵ�ָ�����֡�
    /// </summary>
    public void PlayAtNotation(int id)
    {
        Debug.Log("PlayAtNotation");
        var notationId = DataManager.Instance.GetPositionToAudioNotation(id);
        var notation = DataManager.Instance.GetAudioDataById(notationId[0]);
        if (notation != null)
        {
            // ʹ�� AudioPlayer �� SeekToTime ����������ת����
            AudioPlayer.Instance.SeekToTime(notation.time);

            // ���µ�ǰ����
            currentIndex = notation.notationIndex;

            // NotationViewManager.Instance.UpdateHighlight(notations[currentIndex].id);
        }
    }

    /// <summary>
    /// ʹ�ö��ַ����ҵ�ǰʱ���Ӧ�ļ���������
    /// </summary>
    private int FindNotationIndex(float time)
    {
        Debug.Log($"FindNotationIndex {currentIndex}");

        if (notations == null || notations.Count == 0) return -1;

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

    public void SetPauseDuration(int duration)
    {
        pauseDuration = duration;
    }
    
    IEnumerator PauseBeforeLoop()
    {

        Debug.Log($"pause {pauseDuration}s...");
        // ��ͣ��Ƶ
        AudioPlayer.Instance.SetPause();

        // �ȴ���ͣʱ��
        yield return new WaitForSeconds(pauseDuration);

        Debug.Log("pause Ended.");

        AudioPlayer.Instance.SetPlay();
    }

    private void CheckABLoopCount()
    {
        if (remainingLoops > 1)
        {
            remainingLoops--;
            AudioUIController.Instance.SetLoopCount(remainingLoops);
            StartCoroutine(PauseBeforeLoop());
        }
        else if (remainingLoops == 1)
        {
            remainingLoops = -1;
            AudioUIController.Instance.SetLoopCount(remainingLoops);
            AudioPlayer.Instance.SetPause();
        }
        else
        {
            StartCoroutine(PauseBeforeLoop());
        }
    }

    private void CheckABLoop()
    {
        // ����ѭ����
        if (AudioPlayer.Instance.IsABLoop() && (currentIndex < loopStart || currentIndex > loopEnd) && loopStart != loopEnd)
        {
            Debug.Log($"loopStart{loopStart}, loopEnd{loopEnd}, currentIndex{currentIndex}");
            //if(loopStart != loopEnd)
            //{
                PlayAtNotation(loopStart);
                CheckABLoopCount();
            //}
        }
    }

    public void SetLoopCount(int count)
    {
        remainingLoops = count;
    }
}
