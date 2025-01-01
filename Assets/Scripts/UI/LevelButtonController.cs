using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

public class LevelButtonController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform arrowImage; // ��ͷͼ��� RectTransform
    public RectTransform songList; // ��Ŀ�б�����
    public GameObject trackItemPrefab; // ��Ŀ��Ԥ����

    [Header("Animation Settings")]
    public float expandDuration = 0.5f; // չ��/�۵�����ʱ��
    public float itemHeight = 100f; // ÿ����Ŀ��ĸ߶�
    public float spacing = 5f; // ��Ŀ��֮��ļ��
    public float collapsedHeight = 0f; // �۵�ʱ�ĸ߶�

    private bool isExpanded = false; // ��ǰչ��״̬
    private int lastActivatedIndex = -1; // �ϴμ������������

    /// <summary>
    /// �л�չ��/�۵�״̬
    /// </summary>
    public void ToggleExpand()
    {
        isExpanded = !isExpanded;

        // ������ת��ͷ
        arrowImage.DORotate(Vector3.forward * (isExpanded ? -90 : 0), expandDuration);

        // ��̬����չ����ĸ߶�
        float expandedHeight = songList.childCount * itemHeight + (songList.childCount - 1) * spacing;

        if (isExpanded)
        {
            // ��̬չ�� SongList
            songList.DOSizeDelta(new Vector2(songList.sizeDelta.x, expandedHeight), expandDuration)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(UpdateVisibleItems)
                .OnComplete(() =>
                 {
                     // ����������ǿ���ؽ���������
                     LayoutRebuilder.ForceRebuildLayoutImmediate(songList);
                 });

        }
        else
        {
            // �۵� SongList
            songList.DOSizeDelta(new Vector2(songList.sizeDelta.x, 0), expandDuration)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(HideVisibleItems) // ÿ�θ���ʱ����������
                .OnComplete(() =>
                {
                    foreach (Transform item in songList)
                    {
                        item.gameObject.SetActive(false); // ������������
                    }
                    lastActivatedIndex = -1; // ���ü�������
                });
        }

        // ǿ��ˢ�²���
        LayoutRebuilder.ForceRebuildLayoutImmediate(songList);
    }

    /// <summary>
    /// ÿ�������߶ȸ���ʱ�������µ�����
    /// </summary>
    private void UpdateVisibleItems()
    {
        float currentHeight = songList.sizeDelta.y;

        // ���ݵ�ǰ�߶ȼ���Ӧ�������������
        int visibleItemCount = Mathf.FloorToInt((currentHeight + spacing) / (itemHeight + spacing));

        // ������δ��ʾ������
        for (int i = lastActivatedIndex + 1; i < visibleItemCount && i < songList.childCount; i++)
        {
            Transform item = songList.GetChild(i);
            item.gameObject.SetActive(true); // ��ʾ����

            // ˢ������� Content ����
            TrackItemController controller = item.GetComponent<TrackItemController>();
            if (controller != null)
            {
                controller.RefreshContentLayout();
            }

            // ͸���Ƚ��䶯��
            CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(1, 0.3f);

            lastActivatedIndex = i; // ���¼�������
        }
    }

    /// <summary>
    /// ÿ�������߶ȸ���ʱ�����ؿɼ�������
    /// </summary>
    private void HideVisibleItems()
    {
        float currentHeight = songList.sizeDelta.y;

        // ���ݵ�ǰ�߶ȼ���Ӧ���ص���������
        int visibleItemCount = Mathf.CeilToInt((currentHeight + spacing) / (itemHeight + spacing));

        // �����س����߶ȷ�Χ������
        for (int i = lastActivatedIndex; i >= visibleItemCount && i >= 0; i--)
        {
            Transform item = songList.GetChild(i);

            // ͸���Ƚ��䶯��
            CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(0, 0.01f).OnComplete(() =>
            {
                item.gameObject.SetActive(false); // �����������������
            });

            lastActivatedIndex = i - 1; // ���¼�������
        }
    }

    /// <summary>
    /// ��̬������Ŀ��
    /// </summary>
    public void AddTrackItem(TrackData track)
    {
        // ʵ������Ŀ��Ԥ����
        GameObject trackItem = Instantiate(trackItemPrefab, songList);
        trackItem.SetActive(false); // ��ʼ����

        // ���� UI ��Ϣ
        TrackItemController controller = trackItem.GetComponent<TrackItemController>();
        controller.SetTrackData(track);

        //// ��������/ɾ����ť�߼�
        //var button = trackItem.GetComponentInChildren<Button>();
        //if (button != null)
        //{
        //    button.onClick.AddListener(() => OnTrackActionClicked(trackName, artist));
        //}
    }

    public void SimulateTrackClick(string trackId)
    {
        foreach (Transform item in songList)
        {
            var trackController = item.GetComponent<TrackItemController>();
            if (trackController != null && trackController.GetTrackData().id == trackId)
            {
                trackController.OnItemClick(); // ģ����
                trackController.RefreshContentLayout();
                Debug.Log($"Simulated click on track: {trackId}");
                return;
            }
        }

        Debug.LogWarning($"Track with ID {trackId} not found.");
    }

    ///// <summary>
    ///// ��Ŀ�������/ɾ����ť����¼�
    ///// </summary>
    //private void OnTrackActionClicked(string trackName, string artist)
    //{
    //    Debug.Log($"Button clicked for track: {trackName}, artist: {artist}");
    //    // ʵ�����ػ�ɾ���߼��������л���ť״̬�򴥷�����¼�
    //}

    /// <summary>
    /// ��ʼ����Ŀ�б�
    /// </summary>
    public void InitializeTracks(List<TrackData> tracks)
    {
        // ��յ�ǰ�б�
        foreach (RectTransform child in songList)
        {
            Destroy(child.gameObject);
        }

        // �������Ŀ
        foreach (var track in tracks)
        {
            AddTrackItem(track);
        }

        // ��ʼ��ʱ�۵�
        songList.sizeDelta = new Vector2(songList.sizeDelta.x, collapsedHeight);
        isExpanded = false;
        lastActivatedIndex = -1;
    }
}
