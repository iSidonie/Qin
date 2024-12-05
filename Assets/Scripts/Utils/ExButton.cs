using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Action OnClick { get; set; }
    public Action OnDoubleClick { get; set; }
    public Action OnSelect { get; set; } // 当鼠标进入并选择该按钮时的事件
    public Action OnAnyButtonPressed { get; set; }
    public Action OnAnyButtonReleased { get; set; }

    private float doubleClickInterval = 0.2f;
    private float lastClickTime = -1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnAnyButtonPressed?.Invoke();
            // 处理单击与双击
            float currentTime = Time.time;
            if (currentTime - lastClickTime <= doubleClickInterval)
            {
                OnDoubleClick?.Invoke();
                lastClickTime = -1f;
            }
            else
            {
                OnClick?.Invoke();
                lastClickTime = currentTime;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnAnyButtonReleased?.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //if (isDragging && OnSelect != null)
        //{
            OnSelect.Invoke(); // 当拖拽进入时选择该按钮
        // }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 可添加其他逻辑，例如取消高亮等
    }
}
