using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ABLoopManager : MonoBehaviour
{
    private bool isABLoop = false;
    private int loopStart = 0;
    private int loopEnd = -1;
    private int remainingLoops = 0; 
    private float pauseDuration = 0f;

    public event System.Action<int, int> OnLoopEnd; // 通知 UI 剩余循环次数

    private static ABLoopManager _instance;
    public static ABLoopManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ABLoopManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(ABLoopManager));
                    _instance = obj.AddComponent<ABLoopManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private void OnEnable()
    {
        NotationPlaybackManager.Instance.OnNotationChanged += (value1, value2) => HandleNotationChanged(value1);
        NotationSelectionManager.SelectedChanged += SetLoopRange;
    }

    private void OnDisable()
    {
        NotationPlaybackManager.Instance.OnNotationChanged -= (value1, value2) => HandleNotationChanged(value1);
        NotationSelectionManager.SelectedChanged -= SetLoopRange;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 设置循环范围。
    /// </summary>
    public void SetLoopRange(int start, int end)
    {
        Debug.Log($"{start}, {end}");
        loopStart = start;
        loopEnd = end;
        if (loopEnd == -1)
        {
            DisableABLoop();
        }
        else
        {
            EnableABLoop();
        }
    }

    /// <summary>
    /// 设置循环次数。
    /// </summary>
    public void SetLoopCount(int count)
    {
        remainingLoops = count;
    }

    /// <summary>
    /// 设置等待时间。
    /// </summary>
    public void SetPauseDuration(int duration)
    {
        pauseDuration = duration;
    }

    /// <summary>
    /// 监听减字变动。
    /// </summary>
    private void HandleNotationChanged(int currentIndex)
    {
        // Debug.Log($"{currentIndex},{loopStart}, {loopEnd}");
        
        if ((!isABLoop && currentIndex < 0) || (isABLoop && (currentIndex < loopStart || currentIndex > loopEnd)))
        {
            Debug.Log($"{currentIndex} Looping back to {loopStart}");

            if (remainingLoops == 1)
            {
                remainingLoops--;
                AudioPlayer.Instance.SetPause();
            }
            else if (remainingLoops > 1)
            {
                remainingLoops--;
                StartCoroutine(PauseBeforeLoop());
            }
            else
            {
                StartCoroutine(PauseBeforeLoop());
            }

            OnLoopEnd?.Invoke(isABLoop? loopStart: 0, remainingLoops);
        }
    }

    /// <summary>
    /// AB循环前等待。
    /// </summary>
    private IEnumerator PauseBeforeLoop()
    {
        // Debug.Log($"pause {pauseDuration}s...");
        AudioPlayer.Instance.SetPause();

        yield return new WaitForSeconds(pauseDuration);
        // Debug.Log("pause Ended.");
        AudioPlayer.Instance.SetPlay();
    }

    public void ToggleABLoop()
    {
        isABLoop = !isABLoop;
        AudioUIController.Instance.UpdateABLoopIcon(isABLoop);
    }

    private void EnableABLoop()
    {
        AudioUIController.Instance.SetABLoopButtonState(true);
    }

    private void DisableABLoop()
    {
        AudioUIController.Instance.SetABLoopButtonState(false);
        isABLoop = false;
        AudioUIController.Instance.UpdateABLoopIcon(isABLoop);
    }

}
