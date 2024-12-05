using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Action OnClick { get; set; }
    public Action OnDoubleClick { get; set; }
    public Action OnSelect { get; set; } // �������벢ѡ��ð�ťʱ���¼�
    public Action OnAnyButtonPressed { get; set; }
    public Action OnAnyButtonReleased { get; set; }

    private float doubleClickInterval = 0.2f;
    private float lastClickTime = -1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnAnyButtonPressed?.Invoke();
            // ��������˫��
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
            OnSelect.Invoke(); // ����ק����ʱѡ��ð�ť
        // }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ����������߼�������ȡ��������
    }
}
