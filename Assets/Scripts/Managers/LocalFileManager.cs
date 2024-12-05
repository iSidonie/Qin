using System.IO;
using UnityEngine;

public class LocalFileManager : MonoBehaviour
{
    private static LocalFileManager _instance;
    public static LocalFileManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LocalFileManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(LocalFileManager));
                    _instance = obj.AddComponent<LocalFileManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

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
    /// ��鱾���ļ��Ƿ���ڡ�
    /// </summary>
    public bool FileExists(string fileName)
    {
        string path = GetFilePath(fileName);
        return File.Exists(path);
    }

    /// <summary>
    /// ��ȡ�����ļ�·����
    /// </summary>
    public string GetFilePath(string fileName)
    {
        // ʹ�� Application.streamingAssetsPath �����ʱ��ص��ļ�
        return Path.Combine(Application.streamingAssetsPath, fileName);
    }

    /// <summary>
    /// ɾ�������ļ���
    /// </summary>
    public void DeleteFile(string fileName)
    {
        string path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"File deleted: {path}");
        }
        else
        {
            Debug.LogWarning($"File not found: {path}");
        }
    }
}
