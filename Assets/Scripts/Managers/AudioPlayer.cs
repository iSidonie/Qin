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

    // 单例实例
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
        // 实现单例模式
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
        // 订阅事件
        EventManager.OnTrackSelected += LoadTrackAudio;
    }

    private void OnDisable()
    {
        // 取消订阅事件
        EventManager.OnTrackSelected -= LoadTrackAudio;
    }

    private void LoadTrackAudio(TrackData trackData)
    {
        Debug.Log($"Loading track Aduio: {trackData.name}");

        // 从 LocalFileManager 加载音频文件
        StartCoroutine(LocalFileManager.Instance.LoadAudioClip(trackData.musicFile, OnAudioLoaded));
    }

    void Update()
    {
        if (isPlaying && audioSource.isPlaying)
        {
            // 更新当前播放时间，并触发时间更新事件
            currentTime = audioSource.time;
            AudioTimeUpdated?.Invoke(currentTime, audioSource.clip.length);
        }
    }

    // 音频加载完成的回调方法
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
    /// 播放和暂停控制。
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
    /// 跳转到指定的时间。
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
