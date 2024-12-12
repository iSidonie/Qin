using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class DynamicSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Slider slider; // 滑动条
    public RectTransform sliderBackground; // 滑动条背景
    public RectTransform sliderHandle; // 滑动条手柄

    public float enlargeHeight = 40f; // 放大后的背景高度
    public float enlargeHandleWidth = 40f; // 放大后的手柄宽度
    public float animationDuration = 0.2f; // 动画持续时间

    private float defaultHeight; // 默认背景高度
    private float defaultHandleWidth; // 默认手柄宽度

    void Start()
    {
        // 初始化默认值
        defaultHeight = sliderBackground.sizeDelta.y; // 背景高度
        defaultHandleWidth = sliderHandle.sizeDelta.x; // 手柄宽度
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Enlarge();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetSize();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Enlarge();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetSize();
    }

    private void Enlarge()
    {
        // 仅调整背景高度
        sliderBackground.DOSizeDelta(new Vector2(sliderBackground.sizeDelta.x, enlargeHeight), animationDuration);

        // 仅调整手柄宽度
        sliderHandle.DOSizeDelta(new Vector2(enlargeHandleWidth, sliderHandle.sizeDelta.y), animationDuration);
    }

    private void ResetSize()
    {
        // 恢复背景高度
        sliderBackground.DOSizeDelta(new Vector2(sliderBackground.sizeDelta.x, defaultHeight), animationDuration);

        // 恢复手柄宽度
        sliderHandle.DOSizeDelta(new Vector2(defaultHandleWidth, sliderHandle.sizeDelta.y), animationDuration);
    }
}
