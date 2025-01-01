using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FileManagementController : MonoBehaviour
{
    [Header("References")]
    public GameObject fileManagementPanel; // 文件管理面板
    public Button fileManagementButton; // 文件管理按钮
    public Button backgroundButton; // 半透明背景按钮

    private bool isPanelOpen = false; // 面板当前状态

    private void Start()
    {
        // 初始化状态
        fileManagementPanel.SetActive(false);

        // 绑定按钮事件
        fileManagementButton.onClick.AddListener(ToggleFileManagementPanel);
        backgroundButton.onClick.AddListener(CloseFileManagementPanel);
    }

    /// <summary>
    /// 切换 FileManagementPanel 的展开/关闭状态
    /// </summary>
    public void ToggleFileManagementPanel()
    {

        isPanelOpen = !isPanelOpen;
        
        fileManagementPanel.SetActive(isPanelOpen);
    }

    /// <summary>
    /// 关闭 FileManagementPanel
    /// </summary>
    public void CloseFileManagementPanel()
    {

        isPanelOpen = false;
        
        fileManagementPanel.SetActive(isPanelOpen);
    }
}
