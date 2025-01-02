using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrackItemController : MonoBehaviour
{
    public Image background; // ���� Image�����ڸı�ѡ��״̬��ɫ
    public TextMeshProUGUI trackNameText;
    public TextMeshProUGUI artistNameText;
    public RectTransform content; // ���� Content ������ RectTransform
    private TrackData trackData;
    private bool isDownload;
    private bool isSelected = false; // ��ǰ�Ƿ�ѡ��
    private static TrackItemController currentSelected;

    private static readonly Color selectedColor = new Color((255 * 253)/(255 * 255f), (250 * 250)/ (255 * 255f), (247 * 239)/ (255 * 255f)); // ѡ��״̬��ɫ
    private static readonly Color defaultColor = new Color(255 / 255f, 250 / 255f, 239 / 255f); // Ĭ��״̬��ɫ

    public void SetTrackData(TrackData data)
    {
        trackData = data;
        trackNameText.text = data.name;
        artistNameText.text = data.artist;
    }

    public void OnDownloadClick()
    {
        Debug.Log($"Downloading track: {trackData.name}");
        // ִ�������߼�
    }

    public void OnDeleteClick()
    {
        Debug.Log($"Deleting track: {trackData.name}");
        // ִ��ɾ���߼�
    }

    public void OnItemClick()
    {
        //// ����ѡ��״̬��֪ͨ����

        // ����Ѿ���ѡ��״̬����������
        if (isSelected) return;

        // ȡ��֮ǰѡ�е���
        if (currentSelected != null)
        {
            currentSelected.Deselect();
        }

        // ���õ�ǰ��Ϊѡ��״̬
        Select();

        EventManager.OnTrackSelected?.Invoke(trackData); // ֪ͨȫ���¼�ϵͳ
    }

    /// <summary>
    /// ���õ�ǰ��Ϊѡ��״̬
    /// </summary>
    private void Select()
    {
        //Debug.Log("change selectedColor");
        isSelected = true;
        background.color = selectedColor; // �ı䱳����ɫ
        currentSelected = this; // ����ȫ��ѡ����

        // ��ӡ����ѡ������
        Debug.Log($"Selected Track: {trackNameText.text} by {artistNameText.text}");
    }

    /// <summary>
    /// ȡ��ѡ��״̬
    /// </summary>
    private void Deselect()
    {
        isSelected = false;
        background.color = defaultColor; // �ָ�Ĭ����ɫ
    }

    public void RefreshContentLayout()
    {
        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            //// ǿ��ˢ�� TMP ����
            //trackNameText.ForceMeshUpdate();
            //artistNameText.ForceMeshUpdate();
        }
    }

    public TrackData GetTrackData()
    {
        return trackData; 
    }
}