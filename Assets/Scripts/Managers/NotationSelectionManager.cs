using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotationSelectionManager : MonoBehaviour
{
    private static NotationSelectionManager _instance;
    public static NotationSelectionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NotationSelectionManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(NotationSelectionManager));
                    _instance = obj.AddComponent<NotationSelectionManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }
    
    private int rangeStart = -1;
    private int rangeEnd = -1;
    private int firstSelected = -1;

    public delegate void OnSelectedChanged(int start, int end);
    public static event OnSelectedChanged SelectedChanged;

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

    private void OnEnable()
    {
        NotationSelectHandler.Instance.OnNotationStartDragging += StartDragging;
        NotationSelectHandler.Instance.OnNotationSelecting += OnNotationSelect;
        NotationSelectHandler.Instance.OnNotationEndDragging += EndDragging;
        EventManager.OnTrackSelected += (value) => Reset();
    }

    private void OnDisable()
    {
        NotationSelectHandler.Instance.OnNotationStartDragging -= StartDragging;
        NotationSelectHandler.Instance.OnNotationSelecting -= OnNotationSelect;
        NotationSelectHandler.Instance.OnNotationEndDragging -= EndDragging;
        EventManager.OnTrackSelected -= (value) => Reset();
    }

    private void Reset()
    {
        rangeStart = -1;
        rangeEnd = -1;
        firstSelected = -1;

        SelectedChanged?.Invoke(rangeStart, rangeEnd);
    }

// ������ѡ��/ȡ��ѡ��
public void OnNotationClick(int id)
    {
        //Debug.Log($"Clicked{id}, rangeStart{rangeStart}, rangeEnd{rangeEnd}");
        if (id >= rangeStart && id <= rangeEnd)
        {
            // ��ѡ��ȡ��ѡ��
            if (rangeStart == rangeEnd)
            {
                // ȡ��Ψһһ��
                rangeStart = 0;
                rangeEnd = -1;
            }
            else if (id == rangeStart)
            {
                // ȡ����һ��
                rangeStart += 1 + GetSubNum(rangeStart);
            }
            else if (id + GetSubNum(id) == rangeEnd)
            {
                // ȡ�����һ��
                rangeEnd = id - 1;
            }
            else
            {
                rangeStart = 0;
                rangeEnd = -1;
            }
        }
        else
        {
            // δѡ��ѡ��
            if (id + GetSubNum(id) == rangeStart - 1)
            {
                rangeStart = id;
            }
            else if (id == rangeEnd + 1)
            {
                rangeEnd = id + GetSubNum(id);
            }
            else
            {
                rangeStart = id;
                rangeEnd = id + GetSubNum(id);
            }
        }
        
        SelectedChanged?.Invoke(rangeStart, rangeEnd);
    }

    public void StartDragging(int id)
    {
        //Debug.Log($"Pressed{id}");
        rangeStart = id;
        rangeEnd = id + GetSubNum(id);
        firstSelected = id;

        ScrollManager.Instance.SetEnable(false);
    }

    public void EndDragging()
    {
        //Debug.Log($"Released");
        firstSelected = -1;

        ScrollManager.Instance.SetEnable(true);
    }

    public void OnNotationSelect(int id)
    {
        //Debug.Log($"now selecting{id}");
        rangeStart = Mathf.Min(firstSelected, id);
        rangeEnd = Mathf.Max(firstSelected, id);
        rangeEnd = rangeEnd + GetSubNum(rangeEnd);
        
        SelectedChanged?.Invoke(rangeStart, rangeEnd);
    }

    public int GetSubNum(int id)
    {
        return DataManager.Instance.GetSubNotations(id).Count;
    }

    public void OnDoubleClickEmptySpace()
    {
        // ˫���հ״�ȡ������ѡ��
        // ClearSelection();
    }
}
