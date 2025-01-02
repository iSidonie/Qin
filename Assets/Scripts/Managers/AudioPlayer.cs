using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.IO;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioMixer audioMixer;

    private bool isPlaying = false;
    private float currentTime;
    private float playbackSpeed = 1.0f;

    public delegate void OnAudioStateChanged(bool isPlaying);
    public static event OnAudioStateChanged AudioStateChanged;

    public delegate void OnAudioTimeUpdated(float currentTime, float totalTime);
    public static event OnAudioTimeUpdated AudioTimeUpdated;

    // ����ʵ��
    private static AudioPlayer _instance;
    public static AudioPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioPlayer>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(AudioPlayer));
                    _instance = obj.AddComponent<AudioPlayer>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        // ʵ�ֵ���ģʽ
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
        EventManager.OnTrackSelected += LoadTrackAudio;
    }

    private void OnDisable()
    {
        // ȡ�������¼�
        EventManager.OnTrackSelected -= LoadTrackAudio;
    }

    private void LoadTrackAudio(TrackData trackData)
    {
        Debug.Log($"Loading track Aduio: {trackData.name}");

        // �� LocalFileManager ������Ƶ�ļ�
        StartCoroutine(LocalFileManager.Instance.LoadAudioClip(trackData.musicFile, OnAudioLoaded));
    }

    void Update()
    {
        if (isPlaying && audioSource.isPlaying)
        {
            // ���µ�ǰ����ʱ�䣬������ʱ������¼�
            currentTime = audioSource.time;
            AudioTimeUpdated?.Invoke(currentTime, audioSource.clip.length);
        }
    }

    // ��Ƶ������ɵĻص�����
    private void OnAudioLoaded(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            AudioTimeUpdated?.Invoke(audioSource.time, audioSource.clip.length);
            Debug.Log("Audio loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load audio clip.");
        }
    }

    /// <summary>
    /// ���ź���ͣ���ơ�
    /// </summary>
    public void TogglePlayPause()
    {
        if (isPlaying)
        {
            SetPause();
        }
        else
        {
            SetPlay();
        }
    }

    /// <summary>
    /// ��ת��ָ����ʱ�䡣
    /// </summary>
    public void SeekToTime(float targetTime)
    {
        Debug.Log("SeekToTime");
        if (audioSource.clip != null)
        {
            audioSource.time = targetTime;
            currentTime = targetTime;
            AudioTimeUpdated?.Invoke(currentTime, audioSource.clip.length);

            // SetPlay();
        }
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        audioSource.pitch = playbackSpeed;
        audioMixer.SetFloat("Pitch", 1.0f / playbackSpeed);
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public void SetPlay()
    {
        if (!isPlaying)
        {
            audioSource.Play();
            isPlaying = true;
            AudioStateChanged?.Invoke(isPlaying);
        }
    }

    public void SetPause()
    {
        if (isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
            AudioStateChanged?.Invoke(isPlaying);
        }
    }
}
