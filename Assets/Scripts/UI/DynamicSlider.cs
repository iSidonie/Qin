using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class DynamicSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Slider slider; // ������
    public RectTransform sliderBackground; // ����������
    public RectTransform sliderHandle; // �������ֱ�

    public float enlargeHeight = 40f; // �Ŵ��ı����߶�
    public float enlargeHandleWidth = 40f; // �Ŵ����ֱ����
    public float animationDuration = 0.2f; // ��������ʱ��

    private float defaultHeight; // Ĭ�ϱ����߶�
    private float defaultHandleWidth; // Ĭ���ֱ����

    void Start()
    {
        // ��ʼ��Ĭ��ֵ
        defaultHeight = sliderBackground.sizeDelta.y; // �����߶�
        defaultHandleWidth = sliderHandle.sizeDelta.x; // �ֱ����
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
        // �����������߶�
        sliderBackground.DOSizeDelta(new Vector2(sliderBackground.sizeDelta.x, enlargeHeight), animationDuration);

        // �������ֱ����
        sliderHandle.DOSizeDelta(new Vector2(enlargeHandleWidth, sliderHandle.sizeDelta.y), animationDuration);
    }

    private void ResetSize()
    {
        // �ָ������߶�
        sliderBackground.DOSizeDelta(new Vector2(sliderBackground.sizeDelta.x, defaultHeight), animationDuration);

        // �ָ��ֱ����
        sliderHandle.DOSizeDelta(new Vector2(defaultHandleWidth, sliderHandle.sizeDelta.y), animationDuration);
    }
}
