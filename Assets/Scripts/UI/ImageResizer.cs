using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ImageResizer : MonoBehaviour
{
    public RawImage backgroundImage;  // 背景图片组件
    public Image background;  // 背景图片组件
    public ScrollRect scrollRect;  // ScrollRect 组件
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

        // 获取背景图片的实际尺寸（根据图片的纹理宽高比）
        Texture2D texture = (Texture2D)backgroundImage.texture;
        if (texture == null) return;

        float imageAspectRatio = (float)texture.width / texture.height;

        // 计算 Image 高度，使宽度填满 Viewport 的宽度并保持图片比例
        RectTransform viewportRect = scrollRect.viewport;
        float viewportWidth = viewportRect.rect.width;
        float imageHeight = viewportWidth / imageAspectRatio;

        // 设置 Image 的 RectTransform 高度，使其匹配图片实际高度
        RectTransform imageRect = backgroundImage.rectTransform;
        imageRect.sizeDelta = new Vector2(viewportWidth, imageHeight);

        // 设置 background 的 RectTransform 高度，使其匹配图片实际高度
        RectTransform bgRect = background.rectTransform;
        bgRect.sizeDelta = new Vector2(viewportWidth, imageHeight);

        // 设置 Content 的 RectTransform 高度，使其匹配图片实际高度
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
