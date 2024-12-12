using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotationSelectHandler : MonoBehaviour
{
    private float doubleClickInterval = 0.2f;
    private float lastClickTime = -1f;
    private bool isDragging = false;
    private bool isPressed = false;

    private int start = -1;

    public event System.Action<int> OnNotationStartDragging;
    public event System.Action<int> OnNotationSelecting;
    public event System.Action OnNotationEndDragging; 

    private static NotationSelectHandler _instance;
    public static NotationSelectHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NotationSelectHandler>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(NotationSelectHandler));
                    _instance = obj.AddComponent<NotationSelectHandler>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
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

    public void HandleSelect(int id)
    {
        // ScrollManager.Instance.SetUserInteraction();

        if (isPressed)
        {
            // Debug.Log($"HandleSelect{id}, start{start}");
            // if (!isDragging && id != start)
            if (!isDragging)
            {
                isDragging = true;
                OnNotationStartDragging?.Invoke(start);
                //NotationSelectionManager.Instance.StartDragging(start);
                //ScrollManager.Instance.StartDragSelection();
            }
            //NotationSelectionManager.Instance.OnNotationSelect(id);
            OnNotationSelecting?.Invoke(id);
        }
    }

    public void HandleButtonPressed(int id)
    {
        ScrollManager.Instance.SetUserInteraction();

        if (!isPressed)
        {
            isPressed = true;
            start = id;
        }
    }

    public void HandleButtonReleased(int id)
    {

        if (!isDragging)
        {
            float currentTime = Time.time;
            if (currentTime - lastClickTime <= doubleClickInterval)
            {
                // Ë«»÷²¥·Å
                // Debug.Log("Double clicked");
                var notationId = DataManager.Instance.GetPositionToAudioNotation(id);
                var notation = DataManager.Instance.GetAudioDataById(notationId[0]);
                NotationPlaybackManager.Instance.PlayAtNotation(notation.id);
                lastClickTime = -1f;
            }
            else
            {
                lastClickTime = currentTime;
                StartCoroutine(WaitForDoubleClick(id));
            }
        }
        else
        {
            //NotationSelectionManager.Instance.EndDragging();
            //ScrollManager.Instance.StopDragSelection();
            OnNotationEndDragging?.Invoke();
        }

        isPressed = false;
        isDragging = false;
        start = -1;
    }

    IEnumerator WaitForDoubleClick(int id)
    {
        yield return new WaitForSeconds(doubleClickInterval + 0.005f);
        if (lastClickTime != -1f)
        {
            // Debug.Log("Clicked");
            NotationSelectionManager.Instance.OnNotationClick(id);
        }
    }
}
