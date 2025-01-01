using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

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

    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>(); // ����
    
    public IEnumerator LoadJsonAsync(string path, System.Action<string> callback)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, path);
        UnityWebRequest uwr = UnityWebRequest.Get(fullPath);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            callback?.Invoke(uwr.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"Failed to load JSON from {path}: {uwr.error}");
        }
    }

    public AudioClip LoadAudioClip(string filePath)
    {
        if (File.Exists(filePath))
        {
            string url = "file://" + filePath;
            var audioClip = new WWW(url).GetAudioClip(false, true, AudioType.MPEG);
            return audioClip;
        }
        Debug.LogError($"Audio file not found: {filePath}");
        return null;
    }

    /// <summary>
    /// ����ָ��·����ͼƬ�ļ������� Texture2D��
    /// </summary>
    public void LoadTexture(string path, System.Action<Texture2D> onLoaded, bool compress = true, int maxSize = 2048)
    {
        // ���ͼƬ�ѻ��棬ֱ�ӷ��ػ���
        if (textureCache.ContainsKey(path))
        {
            onLoaded?.Invoke(textureCache[path]);
            return;
        }

        // �����첽����ͼƬ
        StartCoroutine(LoadTextureCoroutine(path, texture =>
    {
            if (texture != null)
            {
                // ѹ������
                if (compress)
                {
                    CompressTexture(texture);
                }

                //// ��������
                //if (texture.width > maxSize || texture.height > maxSize)
                //{
                //    texture = ResizeTexture(texture, maxSize, maxSize);
                //}

                // ��������
                textureCache[path] = texture;
            }

            onLoaded?.Invoke(texture);
        }));
    }

    private IEnumerator LoadTextureCoroutine(string path, System.Action<Texture2D> onLoaded)
    {
        var fullPath = Path.Combine(Application.streamingAssetsPath, path);
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(fullPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                textureCache[fullPath] = texture; // ����ͼƬ
                Debug.Log($"Load Texture{fullPath}");
                onLoaded?.Invoke(texture);
            }
            else
            {
                Debug.LogError($"Failed to load texture: {uwr.error}");
                onLoaded?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// ����δʹ�õĻ��档
    /// </summary>
    public void ClearUnusedCache()
    {
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// ������л��档
    /// </summary>
    public void ClearAllCache()
    {
        textureCache.Clear();
    }


    /// <summary>
    /// �� Texture2D ����ѹ����
    /// </summary>
    private void CompressTexture(Texture2D texture)
    {
        // ����Ƿ�Ϊ 4 �ı���
        int width = texture.width;
        int height = texture.height;

        if (width % 4 != 0 || height % 4 != 0)
        {
            int newWidth = Mathf.NextPowerOfTwo((width + 3) / 4 * 4);
            int newHeight = Mathf.NextPowerOfTwo((height + 3) / 4 * 4);

            // �����ߴ�
            texture = ResizeTexture(texture, newWidth, newHeight);
        }

        // ѹ������
        texture.Compress(true);
        texture.Apply();
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = rt;

        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
}
