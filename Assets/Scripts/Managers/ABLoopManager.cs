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

    public event System.Action<int, int> OnLoopEnd; // ֪ͨ UI ʣ��ѭ������

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
    /// ����ѭ����Χ��
    /// </summary>
    public void SetLoopRange(int start, int end)
    {
        Debug.Log($"{start}, {end}");
        if(end == -1)
        {
            loopStart = start;
            loopEnd = end;
            DisableABLoop();
            return;
        }

        // д��ô�õĴ������ǶԲ���
        var startList = DataManager.Instance.GetPositionToAudioNotation(start);
        var endList = DataManager.Instance.GetPositionToAudioNotation(end);

        if (startList.Count == 1)
        {
            loopStart = startList[0];
            loopEnd = endList[endList.Count - 1];
        }
        else
        {
            loopStart = startList[0];
                
            // �ҵ�����С��end��endList[0]��������start
            var maxStart = 0;
            for (int i = 0; i < startList.Count; i++)
            {
                // �ҵ���һ����endList[0]���start��ΪmaxStart
                if (startList[i] > endList[0])
                {
                    maxStart = i;
                    break;
                }
            }
            if (maxStart == 0)
            {
                // ����startList����endList[0]С
                loopEnd = endList[0];
            }
            else
            {
                // �����ҵ���һ����maxStartС��endList
                for (int i = endList.Count - 1; i >= 0; i--)
                {
                    Debug.Log(endList[i]);
                    if (endList[i] < startList[maxStart])
                    {
                        loopEnd = endList[i];
                        break;
                    }
                }
            }
        }

        Debug.Log($"again{loopStart}, {loopEnd}");

        EnableABLoop();
    }

    /// <summary>
    /// ����ѭ��������
    /// </summary>
    public void SetLoopCount(int count)
    {
        remainingLoops = count;
    }

    /// <summary>
    /// ���õȴ�ʱ�䡣
    /// </summary>
    public void SetPauseDuration(int duration)
    {
        pauseDuration = duration;
    }

    /// <summary>
    /// �������ֱ䶯��
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
    /// ABѭ��ǰ�ȴ���
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
