using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FileManagementController : MonoBehaviour
{
    [Header("References")]
    public GameObject fileManagementPanel; // �ļ��������
    public Button fileManagementButton; // �ļ�����ť
    public Button backgroundButton; // ��͸��������ť

    private bool isPanelOpen = false; // ��嵱ǰ״̬

    private void Start()
    {
        // ��ʼ��״̬
        fileManagementPanel.SetActive(false);

        // �󶨰�ť�¼�
        fileManagementButton.onClick.AddListener(ToggleFileManagementPanel);
        backgroundButton.onClick.AddListener(CloseFileManagementPanel);
    }

    /// <summary>
    /// �л� FileManagementPanel ��չ��/�ر�״̬
    /// </summary>
    public void ToggleFileManagementPanel()
    {

        isPanelOpen = !isPanelOpen;
        
        fileManagementPanel.SetActive(isPanelOpen);
    }

    /// <summary>
    /// �ر� FileManagementPanel
    /// </summary>
    public void CloseFileManagementPanel()
    {

        isPanelOpen = false;
        
        fileManagementPanel.SetActive(isPanelOpen);
    }
}
