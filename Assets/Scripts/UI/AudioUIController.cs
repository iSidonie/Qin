using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AudioUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button playPauseButton;
    public Button ABLoopButton;
    public TMP_Dropdown playbackSpeedDropdown;
    public TMP_Dropdown loopCountDropdown;
    public TMP_Dropdown pauseDurationDropdown;
    public TextMeshProUGUI currentTimeText;
    public TextMeshProUGUI totalTimeText;
    public Slider progressBar;
    public Sprite playIcon;
    public Sprite pauseIcon;
    public Sprite enableIcon;
    public Sprite disableIcon;

    private bool isDraggingBar = false; // 标志用户是否正在拖动进度条
    private bool isProgrammaticUpdate = false; // 标志是否是代码触发的更新

    // 单例实例
    private static AudioUIController _instance;
    public static AudioUIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioUIController>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(AudioUIController));
                    _instance = obj.AddComponent<AudioUIController>();
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

    private void Start()
    {
        // 给开始/暂停按钮添加事件监听
        if (playPauseButton != null)
        {
            playPauseButton.onClick.AddListener(OnPlayPauseButtonClicked);

            // 初始化图标
            UpdatePlayPauseIcon(false);
        }

        // 给AB循环按钮添加事件监听
        if (ABLoopButton != null)
        {
            ABLoopButton.onClick.AddListener(OnABLoopButtonClicked);

            // 初始化图标
            UpdateABLoopIcon(false);
            SetABLoopButtonState(false);
        }

        // 绑定Slider的值变化事件
        if (progressBar != null)
        {
            progressBar.onValueChanged.AddListener(OnProgressBarChanged);
        }

        if (playbackSpeedDropdown != null)
        {
            playbackSpeedDropdown.onValueChanged.AddListener(OnPlaybackSpeedChanged);
        }

        if (pauseDurationDropdown != null)
        {
            pauseDurationDropdown.onValueChanged.AddListener(OnPauseDurationChanged);
        }

        if (loopCountDropdown != null)
        {
            loopCountDropdown.onValueChanged.AddListener(OnLoopCountChanged);
        }
    }

    private void OnEnable()
    {
        // 订阅 AudioPlayer 的事件
        AudioPlayer.AudioTimeUpdated += UpdateTimeAndProgress;
        AudioPlayer.AudioStateChanged += UpdatePlayPauseIcon;
    }

    private void OnDisable()
    {
        // 取消订阅事件
        AudioPlayer.AudioTimeUpdated -= UpdateTimeAndProgress;
        AudioPlayer.AudioStateChanged -= UpdatePlayPauseIcon;
    }

    void OnDestroy()
    {
        // 在销毁时解除绑定，避免事件泄漏
        if (playPauseButton != null)
        {
            playPauseButton.onClick.RemoveListener(OnPlayPauseButtonClicked);
        }

        if (ABLoopButton != null)
        {
            ABLoopButton.onClick.RemoveListener(OnABLoopButtonClicked);
        }

        // 移除事件监听，防止内存泄漏
        if (progressBar != null)
        {
            progressBar.onValueChanged.RemoveListener(OnProgressBarChanged);
        }

        if (playbackSpeedDropdown != null)
        {
            playbackSpeedDropdown.onValueChanged.RemoveListener(OnPlaybackSpeedChanged);
        }
    }

    private void OnPlayPauseButtonClicked()
    {
        // 切换播放和暂停状态
        if (AudioPlayer.Instance != null)
        {
            AudioPlayer.Instance.TogglePlayPause();
        }
    }

    public void OnABLoopButtonClicked()
    {
        // 切换AB循环状态
        if (AudioPlayer.Instance != null)
        {
            var isABLoop = AudioPlayer.Instance.ToggleABLoop();
            UpdateABLoopIcon(isABLoop);
        }
    }

    /// <summary>
    /// 用户开始拖动进度条时调用
    /// </summary>
    public void OnDragStart()
    {
        isDraggingBar = true;
    }

    /// <summary>
    /// 用户结束拖动进度条时调用
    /// </summary>
    public void OnDragEnd()
    {
        isDraggingBar = false;

        // 在用户拖动结束后，根据进度条值设置音频时间
        if (AudioPlayer.Instance != null && AudioPlayer.Instance.audioSource.clip != null)
        {
            float targetTime = progressBar.value * AudioPlayer.Instance.audioSource.clip.length;
            AudioPlayer.Instance.SeekToTime(targetTime);
        }
    }

    /// <summary>
    /// 当进度条的值发生变化时调用
    /// </summary>
    public void OnProgressBarChanged(float value)
    {
        // 如果是代码触发的更新，直接返回，避免循环调用
        if (isProgrammaticUpdate)
            return;

        if (AudioPlayer.Instance != null && AudioPlayer.Instance.audioSource.clip != null)
        {
            Debug.Log("OnProgressBarChanged");
            // 根据进度条的值跳转到指定时间
            float targetTime = value * AudioPlayer.Instance.audioSource.clip.length;
            AudioPlayer.Instance.SeekToTime(targetTime);
        }
    }

    /// <summary>
    /// 更新当前时间文本和进度条。
    /// </summary>
    private void UpdateTimeAndProgress(float currentTime, float totalTime)
    {
        // 标记为代码触发更新
        isProgrammaticUpdate = true;

        // 更新UI
        currentTimeText.text = FormatTime(currentTime);
        totalTimeText.text = FormatTime(totalTime);
        progressBar.value = currentTime / totalTime;

        // 重置标记
        isProgrammaticUpdate = false;
    }

    /// <summary>
    /// 格式化时间为 "MM:SS" 格式。
    /// </summary>
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// 更新开始/暂停按钮的图标。
    /// </summary>
    private void UpdatePlayPauseIcon(bool isPlaying)
    {
        Image playPauseIcon = playPauseButton.GetComponent<Image>();
        if (playPauseIcon != null)
        {
            playPauseIcon.sprite = isPlaying ? pauseIcon : playIcon;
        }
    }

    /// <summary>
    /// 更新AB循环按钮的图标。
    /// </summary>
    private void UpdateABLoopIcon(bool isABLoop)
    {
        Image ABLoopIcon = ABLoopButton.GetComponent<Image>();
        if (ABLoopIcon != null)
        {
            ABLoopIcon.sprite = isABLoop ? enableIcon : disableIcon;
        }
    }

    public void SetABLoopButtonState(bool state)
    {
        ABLoopButton.interactable = state;
    }

    private void OnPlaybackSpeedChanged(int index)
    {
        string selectedSpeed = playbackSpeedDropdown.options[index].text;

        // 根据选项设置倍速
        float speed = float.Parse(selectedSpeed.Replace("x", "")); 
        AudioPlayer.Instance.SetPlaybackSpeed(speed);
    }

    private void OnPauseDurationChanged(int index)
    {
        string selectedDuration = pauseDurationDropdown.options[index].text;

        // 根据选项设置倍速
        int duration = int.Parse(selectedDuration.Replace("s", ""));
        NotationPlaybackManager.Instance.SetPauseDuration(duration);
    }

    private void OnLoopCountChanged(int index)
    {
        if (index == 0)
        {
            NotationPlaybackManager.Instance.SetLoopCount(-1);
        }
        else
        {
            string selectedLoopCount = loopCountDropdown.options[index].text;

            // 根据选项设置倍速
            int loopCount = int.Parse(selectedLoopCount.Replace("x", ""));
            NotationPlaybackManager.Instance.SetLoopCount (loopCount);
        }
    }

    public void SetLoopCount(int count)
    {
        if (count > 0)
        {
            loopCountDropdown.captionText.text = $"{count}x";
        }
        else
        {
            loopCountDropdown.captionText.text = "∞";
        }
    }
}
