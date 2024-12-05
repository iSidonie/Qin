using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Audio;
using System.IO;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioMixer audioMixer;

    private bool isPlaying = false;
    private bool isABLoop = false;
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

    void Start()
    {
        string trackName = "QiuFengCi";
        string performer = "GongYi";

        StartCoroutine(LoadAudioClip(trackName, performer));
        audioSource.loop = true;
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

    IEnumerator LoadAudioClip(string trackName, string performer)
    {
        string relativePath = $"music/{trackName}_{performer}.mp3";
        string musicPath = Path.Combine(Application.streamingAssetsPath, relativePath);

        // 使用 UnityWebRequest 加载音频文件
        UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(musicPath, AudioType.MPEG);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            // 获取加载的音频文件并赋值给 AudioSource
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
            audioSource.clip = audioClip;

            // 触发时间更新事件，初始化 UI 显示
            AudioTimeUpdated?.Invoke(audioSource.time, audioSource.clip.length);
        }
        else
        {
            Debug.LogError("Failed to load audio clip: " + uwr.error);
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
    /// AB循环控制。
    /// </summary>
    public bool ToggleABLoop()
    {
        if (isABLoop)
        {
            isABLoop = false;
        }
        else
        {
            isABLoop = true;
        }
        return isABLoop;
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

            SetPlay();
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

    public bool IsABLoop()
    {
        return isABLoop;
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
