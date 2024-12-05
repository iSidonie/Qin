using UnityEngine;
using UnityEngine.EventSystems;

public class NotationInteractionHandler : MonoBehaviour
{
    public int notationId; // 当前按钮的 ID

    private void Awake()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            Debug.LogError("EventTrigger component is missing.");
            return;
        }

        // 动态绑定事件
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

    // 添加事件触发器监听的方法
    private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((eventData) => action.Invoke(eventData));
        trigger.triggers.Add(entry);
    }
}
