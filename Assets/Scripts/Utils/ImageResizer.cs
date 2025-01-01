using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(RectTransform))]
//public class ImageResizer : MonoBehaviour
//{
//    public RawImage backgroundImage;  // ����ͼƬ���
//    public Image background;  // ����ͼƬ���
//    public ScrollRect scrollRect;  // ScrollRect ���
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

//        // ��ȡ����ͼƬ��ʵ�ʳߴ磨����ͼƬ�������߱ȣ�
//        Texture2D texture = (Texture2D)backgroundImage.texture;
//        if (texture == null) return;

//        float imageAspectRatio = (float)texture.width / texture.height;

//        // ���� Image �߶ȣ�ʹ������� Viewport �Ŀ�Ȳ�����ͼƬ����
//        RectTransform viewportRect = scrollRect.viewport;
//        float viewportWidth = viewportRect.rect.width;
//        float imageHeight = viewportWidth / imageAspectRatio;

//        // ���� Image �� RectTransform �߶ȣ�ʹ��ƥ��ͼƬʵ�ʸ߶�
//        RectTransform imageRect = backgroundImage.rectTransform;
//        imageRect.sizeDelta = new Vector2(viewportWidth, imageHeight);

//        // ���� background �� RectTransform �߶ȣ�ʹ��ƥ��ͼƬʵ�ʸ߶�
//        RectTransform bgRect = background.rectTransform;
//        bgRect.sizeDelta = new Vector2(viewportWidth, imageHeight);

//        // ���� Content �� RectTransform �߶ȣ�ʹ��ƥ��ͼƬʵ�ʸ߶�
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
    /// ����ͼƬ�ߴ粢�����������ӡ�
    /// </summary>
    public void ResizeImage(RawImage image, Texture2D texture, System.Action onResizeComplete = null)
    {
        if (image == null || texture == null) return;

        // ����ͼƬ texture
        image.texture = texture;

        // ������������
        RectTransform rectTransform = image.rectTransform;
        Rect containerRect = rectTransform.rect;

        scaleFactor =  containerRect.width / texture.width;

        rectTransform.sizeDelta = new Vector2(containerRect.width, texture.height * scaleFactor);

        // ֪ͨ���
        onResizeComplete?.Invoke();
    }

    /// <summary>
    /// ��ȡ��ǰ�������ӡ�
    /// </summary>
    public float GetScale()
    {
        return scaleFactor;
    }
}
