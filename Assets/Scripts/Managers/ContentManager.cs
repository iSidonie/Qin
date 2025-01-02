using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class ContentManager : MonoBehaviour
{
    private static ContentManager _instance;
    public static ContentManager Instance => _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ����ҳ�����ݣ���������ͼƬ�Ͱ�ť��
    /// </summary>
    public void UpdatePageContent(GameObject page, int pageIndex)
    {
        if (page == null) return;

        // ��������ͼƬ
        string sheetFileName = DataManager.Instance.GetSheetFileName();
        string imagePath = sheetFileName.Replace("{n}", pageIndex.ToString());
        var sheetImage = page.transform.Find("SheetImage").GetComponent<RawImage>();
        var rectTransform = sheetImage.GetComponent<RectTransform>();

        ClearPageContent(rectTransform);

        LocalFileManager.Instance.LoadTexture(imagePath, texture =>
        {
            if (texture != null)
            {
                ImageResizer.Instance.ResizeImage(sheetImage, texture);
                AdjustPageComponents(page, rectTransform);

                // ���ذ�ť����
                var pageData = DataManager.Instance.GetPositionByPage(pageIndex);
                CreatePageContent(pageData, rectTransform);
            }
        });
    }

    /// <summary>
    /// ��̬���� PagePrefab �������С��
    /// </summary>
    private void AdjustPageComponents(GameObject page, RectTransform sheetRectTransform)
    {
        var backgroundImage = page.transform.Find("BackgroundImage").GetComponent<Image>();
        var pageRectTransform = page.GetComponent<RectTransform>();

        if (backgroundImage != null)
        {
            backgroundImage.rectTransform.sizeDelta = sheetRectTransform.sizeDelta;
        }

        pageRectTransform.sizeDelta = sheetRectTransform.sizeDelta;
    }

    public void CreatePageContent(List<NotationPositionData> pageData, RectTransform container)
    {
        foreach (var notation in pageData)
        {
            var notationImage = CreateNotationUI(notation, container);

            // ֪ͨ ViewManager ����
            NotationViewManager.Instance.AddNotation(notation.id, notationImage);
        }
    }

    private Image CreateNotationUI(NotationPositionData notation, RectTransform container)
    {
        // ʹ�ö���ػ�ȡ Image
        Image notationImage = NotationViewManager.Instance.GetNotationImageFromPool(notation);

        RectTransform rectTransform = notationImage.GetComponent<RectTransform>();
        rectTransform.SetParent(container, false);
        rectTransform.anchoredPosition = new Vector2(
            notation.x * ImageResizer.Instance.GetScale(),
            notation.y * ImageResizer.Instance.GetScale()
        );

        if (notation.type == "Main")
        {
            // ��̬��� NotationInteractionHandler
            NotationInteractionHandler interactionHandler = notationImage.gameObject.AddComponent<NotationInteractionHandler>();
            interactionHandler.notationId = notation.id; // ���ݵ�ǰ���� ID
        }

        return notationImage;
    }

    public void ClearPageContent(RectTransform container)
    {
        foreach (Transform child in container)
        {
            var image = child.GetComponent<Image>();
            NotationViewManager.Instance.ReturnNotationImageToPool(image);
        }
    }
}
