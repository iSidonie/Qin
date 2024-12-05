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

    private bool isDraggingBar = false; // ��־�û��Ƿ������϶�������
    private bool isProgrammaticUpdate = false; // ��־�Ƿ��Ǵ��봥���ĸ���

    // ����ʵ��
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

    private void Start()
    {
        // ����ʼ/��ͣ��ť����¼�����
        if (playPauseButton != null)
        {
            playPauseButton.onClick.AddListener(OnPlayPauseButtonClicked);

            // ��ʼ��ͼ��
            UpdatePlayPauseIcon(false);
        }

        // ��ABѭ����ť����¼�����
        if (ABLoopButton != null)
        {
            ABLoopButton.onClick.AddListener(OnABLoopButtonClicked);

            // ��ʼ��ͼ��
            UpdateABLoopIcon(false);
            SetABLoopButtonState(false);
        }

        // ��Slider��ֵ�仯�¼�
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
        // ���� AudioPlayer ���¼�
        AudioPlayer.AudioTimeUpdated += UpdateTimeAndProgress;
        AudioPlayer.AudioStateChanged += UpdatePlayPauseIcon;
    }

    private void OnDisable()
    {
        // ȡ�������¼�
        AudioPlayer.AudioTimeUpdated -= UpdateTimeAndProgress;
        AudioPlayer.AudioStateChanged -= UpdatePlayPauseIcon;
    }

    void OnDestroy()
    {
        // ������ʱ����󶨣������¼�й©
        if (playPauseButton != null)
        {
            playPauseButton.onClick.RemoveListener(OnPlayPauseButtonClicked);
        }

        if (ABLoopButton != null)
        {
            ABLoopButton.onClick.RemoveListener(OnABLoopButtonClicked);
        }

        // �Ƴ��¼���������ֹ�ڴ�й©
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
        // �л����ź���ͣ״̬
        if (AudioPlayer.Instance != null)
        {
            AudioPlayer.Instance.TogglePlayPause();
        }
    }

    public void OnABLoopButtonClicked()
    {
        // �л�ABѭ��״̬
        if (AudioPlayer.Instance != null)
        {
            var isABLoop = AudioPlayer.Instance.ToggleABLoop();
            UpdateABLoopIcon(isABLoop);
        }
    }

    /// <summary>
    /// �û���ʼ�϶�������ʱ����
    /// </summary>
    public void OnDragStart()
    {
        isDraggingBar = true;
    }

    /// <summary>
    /// �û������϶�������ʱ����
    /// </summary>
    public void OnDragEnd()
    {
        isDraggingBar = false;

        // ���û��϶������󣬸��ݽ�����ֵ������Ƶʱ��
        if (AudioPlayer.Instance != null && AudioPlayer.Instance.audioSource.clip != null)
        {
            float targetTime = progressBar.value * AudioPlayer.Instance.audioSource.clip.length;
            AudioPlayer.Instance.SeekToTime(targetTime);
        }
    }

    /// <summary>
    /// ����������ֵ�����仯ʱ����
    /// </summary>
    public void OnProgressBarChanged(float value)
    {
        // ����Ǵ��봥���ĸ��£�ֱ�ӷ��أ�����ѭ������
        if (isProgrammaticUpdate)
            return;

        if (AudioPlayer.Instance != null && AudioPlayer.Instance.audioSource.clip != null)
        {
            Debug.Log("OnProgressBarChanged");
            // ���ݽ�������ֵ��ת��ָ��ʱ��
            float targetTime = value * AudioPlayer.Instance.audioSource.clip.length;
            AudioPlayer.Instance.SeekToTime(targetTime);
        }
    }

    /// <summary>
    /// ���µ�ǰʱ���ı��ͽ�������
    /// </summary>
    private void UpdateTimeAndProgress(float currentTime, float totalTime)
    {
        // ���Ϊ���봥������
        isProgrammaticUpdate = true;

        // ����UI
        currentTimeText.text = FormatTime(currentTime);
        totalTimeText.text = FormatTime(totalTime);
        progressBar.value = currentTime / totalTime;

        // ���ñ��
        isProgrammaticUpdate = false;
    }

    /// <summary>
    /// ��ʽ��ʱ��Ϊ "MM:SS" ��ʽ��
    /// </summary>
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// ���¿�ʼ/��ͣ��ť��ͼ�ꡣ
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
    /// ����ABѭ����ť��ͼ�ꡣ
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

        // ����ѡ�����ñ���
        float speed = float.Parse(selectedSpeed.Replace("x", "")); 
        AudioPlayer.Instance.SetPlaybackSpeed(speed);
    }

    private void OnPauseDurationChanged(int index)
    {
        string selectedDuration = pauseDurationDropdown.options[index].text;

        // ����ѡ�����ñ���
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

            // ����ѡ�����ñ���
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
            loopCountDropdown.captionText.text = "��";
        }
    }
}
