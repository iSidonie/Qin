using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(RectTransform))]
//public class ImageResizer : MonoBehaviour
//{
//    public RawImage backgroundImage;  // 背景图片组件
//    public Image background;  // 背景图片组件
//    public ScrollRect scrollRect;  // ScrollRect 组件
//    public static ImageResizer Instance;

//    private float scale;

//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        AdjustImageSize();
//        CalculateScale();
//    }

//    void AdjustImageSize()
//    {
//        if (backgroundImage == null || scrollRect == null || background == null) return;

//        // 获取背景图片的实际尺寸（根据图片的纹理宽高比）
//        Texture2D texture = (Texture2D)backgroundImage.texture;
//        if (texture == null) return;

//        float imageAspectRatio = (float)texture.width / texture.height;

//        // 计算 Image 高度，使宽度填满 Viewport 的宽度并保持图片比例
//        RectTransform viewportRect = scrollRect.viewport;
//        float viewportWidth = viewportRect.rect.width;
//        float imageHeight = viewportWidth / imageAspectRatio;

//        // 设置 Image 的 RectTransform 高度，使其匹配图片实际高度
//        RectTransform imageRect = backgroundImage.rectTransform;
//        imageRect.sizeDelta = new Vector2(viewportWidth, imageHeight);

//        // 设置 background 的 RectTransform 高度，使其匹配图片实际高度
//        RectTransform bgRect = background.rectTransform;
//        bgRect.sizeDelta = new Vector2(viewportWidth, imageHeight);

//        // 设置 Content 的 RectTransform 高度，使其匹配图片实际高度
//        RectTransform contentRect = scrollRect.content;
//        contentRect.sizeDelta = new Vector2(viewportWidth, imageHeight);
//    }

//    void CalculateScale()
//    {
//        RectTransform imageRect = backgroundImage.rectTransform;
//        Texture2D texture = (Texture2D)backgroundImage.texture;

//        Debug.Log($"{imageRect.rect.width}, {texture.width}");
//        scale = imageRect.rect.width / texture.width;
//    }

//    public float GetScale() => scale;
//}
public class ImageResizer : MonoBehaviour
{
    private static ImageResizer _instance;
    public static ImageResizer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ImageResizer>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(ImageResizer));
                    _instance = obj.AddComponent<ImageResizer>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private float scaleFactor = 0f;

    /// <summary>
    /// 调整图片尺寸并返回缩放因子。
    /// </summary>
    public void ResizeImage(RawImage image, Texture2D texture, System.Action onResizeComplete = null)
    {
        if (image == null || texture == null) return;

        // 设置图片 texture
        image.texture = texture;

        // 计算缩放因子
        RectTransform rectTransform = image.rectTransform;
        Rect containerRect = rectTransform.rect;

        scaleFactor =  containerRect.width / texture.width;

        rectTransform.sizeDelta = new Vector2(containerRect.width, texture.height * scaleFactor);

        // 通知完成
        onResizeComplete?.Invoke();
    }

    /// <summary>
    /// 获取当前缩放因子。
    /// </summary>
    public float GetScale()
    {
        return scaleFactor;
    }
}
