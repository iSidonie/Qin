using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ImageResizer : MonoBehaviour
{
    public RawImage backgroundImage;  // ����ͼƬ���
    public Image background;  // ����ͼƬ���
    public ScrollRect scrollRect;  // ScrollRect ���
    public static ImageResizer Instance;

    private float scale;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AdjustImageSize();
        CalculateScale();
    }

    void AdjustImageSize()
    {
        if (backgroundImage == null || scrollRect == null || background == null) return;

        // ��ȡ����ͼƬ��ʵ�ʳߴ磨����ͼƬ�������߱ȣ�
        Texture2D texture = (Texture2D)backgroundImage.texture;
        if (texture == null) return;

        float imageAspectRatio = (float)texture.width / texture.height;

        // ���� Image �߶ȣ�ʹ������� Viewport �Ŀ�Ȳ�����ͼƬ����
        RectTransform viewportRect = scrollRect.viewport;
        float viewportWidth = viewportRect.rect.width;
        float imageHeight = viewportWidth / imageAspectRatio;

        // ���� Image �� RectTransform �߶ȣ�ʹ��ƥ��ͼƬʵ�ʸ߶�
        RectTransform imageRect = backgroundImage.rectTransform;
        imageRect.sizeDelta = new Vector2(viewportWidth, imageHeight);

        // ���� background �� RectTransform �߶ȣ�ʹ��ƥ��ͼƬʵ�ʸ߶�
        RectTransform bgRect = background.rectTransform;
        bgRect.sizeDelta = new Vector2(viewportWidth, imageHeight);

        // ���� Content �� RectTransform �߶ȣ�ʹ��ƥ��ͼƬʵ�ʸ߶�
        RectTransform contentRect = scrollRect.content;
        contentRect.sizeDelta = new Vector2(viewportWidth, imageHeight);
    }

    void CalculateScale()
    {
        RectTransform imageRect = backgroundImage.rectTransform;
        Texture2D texture = (Texture2D)backgroundImage.texture;

        scale = imageRect.rect.width / texture.width;
    }

    public float GetScale() => scale;
}
