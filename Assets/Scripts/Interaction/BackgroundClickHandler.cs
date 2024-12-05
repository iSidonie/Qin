using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundClickHandler : MonoBehaviour, IPointerClickHandler
{
    public float doubleClickThreshold = 0.2f; // ˫��ʱ����
    private float lastClickTime = -1f;

    public void OnPointerClick(PointerEventData eventData)
    {
        // ���˫��
        float currentTime = Time.time;
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            OnDoubleClickBackground();
            lastClickTime = -1f;
        }
        else
        {
            lastClickTime = currentTime;
        }
    }

    /// <summary>
    /// ˫��������������/��ͣ������
    /// </summary>
    private void OnDoubleClickBackground()
    {
        Debug.Log("Double-clicked on background");

        // ���ò���/��ͣ����
        AudioPlayer.Instance.TogglePlayPause();
    }
}
