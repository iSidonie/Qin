using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundClickHandler : MonoBehaviour, IPointerClickHandler
{
    public float doubleClickThreshold = 0.2f; // Ë«»÷Ê±¼ä¼ä¸ô
    private float lastClickTime = -1f;

    public void OnPointerClick(PointerEventData eventData)
    {
        // ¼ì²âË«»÷
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
    /// Ë«»÷±³¾°´¥·¢²¥·Å/ÔÝÍ£²Ù×÷¡£
    /// </summary>
    private void OnDoubleClickBackground()
    {
        Debug.Log("Double-clicked on background");

        // µ÷ÓÃ²¥·Å/ÔÝÍ£¹¦ÄÜ
        AudioPlayer.Instance.TogglePlayPause();
    }
}
