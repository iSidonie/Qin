using UnityEngine;
using UnityEngine.EventSystems;

public class NotationInteractionHandler : MonoBehaviour
{
    public int notationId; // ��ǰ��ť�� ID

    private void Awake()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            Debug.LogError("EventTrigger component is missing.");
            return;
        }

        // ��̬���¼�
        AddEventTriggerListener(eventTrigger, EventTriggerType.PointerEnter, (data) =>
        {
            NotationSelectHandler.Instance.HandleSelect(notationId);
        });

        AddEventTriggerListener(eventTrigger, EventTriggerType.PointerDown, (data) =>
        {
            NotationSelectHandler.Instance.HandleButtonPressed(notationId);
        });

        AddEventTriggerListener(eventTrigger, EventTriggerType.PointerUp, (data) =>
        {
            NotationSelectHandler.Instance.HandleButtonReleased(notationId);
        });
    }

    // ����¼������������ķ���
    private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((eventData) => action.Invoke(eventData));
        trigger.triggers.Add(entry);
    }
}
