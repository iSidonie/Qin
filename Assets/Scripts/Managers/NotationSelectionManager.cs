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

    public ScrollRect scrollView; // ���� ScrollView
    private List<int> selectedNotations = new List<int>();
    private int lastSelected = -1;
    private int rangeStart = -1;
    private int rangeEnd = -1;

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

    // ������ѡ��/ȡ��ѡ��
    public void OnNotationClick(int id)
    {
        // Debug.Log("Clicked");
        if (selectedNotations.Contains(id))
        {
            // ��ѡ��ȡ��ѡ��
            DeselectSingleNotation(id);

            if (!isContinuous())
            {
                ClearSelection();
            }
        }
        else
        {
            // δѡ��ѡ��
            SelectSingleNotation(id);

            if (!isContinuous())
            {
                ClearSelection();
                SelectSingleNotation(id);
            }
        }

        if(selectedNotations.Count > 0)
        {
            AudioUIController.Instance.SetABLoopButtonState(true);
            NotationPlaybackManager.Instance.SetLoopRange(selectedNotations[0], selectedNotations[selectedNotations.Count - 1]);
        }
        else
        {
            AudioUIController.Instance.SetABLoopButtonState(false);
            if (AudioPlayer.Instance.IsABLoop())
            {
                AudioUIController.Instance.OnABLoopButtonClicked();
            }
            NotationPlaybackManager.Instance.SetLoopRange(0, 0);
        }
    }

    public void StartDragging(int id)
    {
        //Debug.Log("Pressed");
        rangeStart = id;
        rangeEnd = id;
        lastSelected = id;

        ClearSelection();
        SelectSingleNotation(rangeStart);

        if (scrollView != null)
        {
            scrollView.enabled = false; // ���ù���
        }
    }

    public void EndDragging()
    {
        //Debug.Log("Released");
        AudioUIController.Instance.SetABLoopButtonState(true);
        
        rangeStart = -1;
        rangeEnd = -1;
        lastSelected = -1;

        if (scrollView != null)
        {
            scrollView.enabled = true; // ���ù���
        }
    }

    public void OnNotationSelect(int id)
    {
        // Debug.Log($"now selecting{id}");

        rangeEnd = id;

        UpdateSelectRange();

        lastSelected = rangeEnd;
    }

    public void OnDoubleClickEmptySpace()
    {
        // ˫���հ״�ȡ������ѡ��
        ClearSelection();
    }

    // ��������ѡ��
    private void SelectSingleNotation(int id)
    {
        // ѡ�������ֺ����µ���������
        selectedNotations.Add(id);
        NotationViewManager.Instance.SetNotationState(id, NotationState.Selected);
        
        // ѡ����������µ���������
        foreach (int subId in DataManager.Instance.GetSubNotations(id))
        {
            NotationViewManager.Instance.SetNotationState(subId, NotationState.Selected);
            selectedNotations.Add(subId);
        }
    }

    // ��������ȡ��ѡ��
    private void DeselectSingleNotation(int id)
    {
        // ȡ��ѡ�������ֺ����µ���������
        selectedNotations.Remove(id);
        NotationViewManager.Instance.SetNotationState(id, NotationState.Hidden);
        
        // ȡ��ѡ����������µ���������
        foreach(int subId in DataManager.Instance.GetSubNotations(id))
        {
            NotationViewManager.Instance.SetNotationState(subId, NotationState.Hidden);
            selectedNotations.Remove(subId);
        }
    }
    
    // �϶�ѡ��������
    private void SelectNotations(int start, int end)
    {
        for (int id = start; id <= end; id++)
        {
            if (!selectedNotations.Contains(id))
            {
                selectedNotations.Add(id);
                NotationViewManager.Instance.SetNotationState(id, NotationState.Selected);
            }
        }
    }

    // �϶�ȡ��ѡ��������
    private void DeselectNotations(int start, int end)
    {
        for (int id = start; id <= end; id++)
        {
            if (selectedNotations.Contains(id))
            {
                selectedNotations.Remove(id);
                NotationViewManager.Instance.SetNotationState(id, NotationState.Hidden);
            }
        }
    }

    private void UpdateSelectRange()
    {
        // ѡ��Χ�䶯
        int start = Mathf.Min(rangeStart, rangeEnd);
        int end = Mathf.Max(rangeStart, rangeEnd);
        end = end + DataManager.Instance.GetSubNotations(end).Count;

        int exStart = Mathf.Min(rangeStart, lastSelected);
        int exEnd = Mathf.Max(rangeStart, lastSelected);
        exEnd = exEnd + DataManager.Instance.GetSubNotations(exEnd).Count;

        //Debug.Log($"start:{start}, end:{end}; exStart:{exStart}, exEnd:{exEnd}");
        
        if (start < exStart)
        {
            SelectNotations(start, exStart - 1);
        }

        if (start > exStart)
        {
            DeselectNotations(exStart, start - 1);
        }

        if (end > exEnd)
        {
            SelectNotations(exEnd + 1, end);
        }

        if (end < exEnd)
        {
            DeselectNotations(end + 1, exEnd);
        }

        selectedNotations.Sort();
        NotationPlaybackManager.Instance.SetLoopRange(selectedNotations[0], selectedNotations[selectedNotations.Count - 1]);
    }

    private void ClearSelection()
    {
        foreach(var id in selectedNotations)
        {
            NotationViewManager.Instance.SetNotationState(id, NotationState.Hidden);
        }

        selectedNotations.Clear();
        AudioUIController.Instance.SetABLoopButtonState(false);
    }

    // �����ѡNotations�Ƿ�����
    private bool isContinuous()
    {
        if (selectedNotations.Count > 0)
        {
            selectedNotations.Sort();

            return selectedNotations[selectedNotations.Count - 1] - selectedNotations[0] == selectedNotations.Count - 1;
        }
        else
        {
            return true;
        }
    }

    public bool QueryNotationState(int id)
    {
        return selectedNotations.Contains(id);
    }
}
