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

    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>(); // 缓存
    
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
    /// 加载指定路径的图片文件并返回 Texture2D。
    /// </summary>
    public void LoadTexture(string path, System.Action<Texture2D> onLoaded, bool compress = true, int maxSize = 2048)
    {
        // 如果图片已缓存，直接返回缓存
        if (textureCache.ContainsKey(path))
        {
            onLoaded?.Invoke(textureCache[path]);
            return;
        }

        // 否则异步加载图片
        StartCoroutine(LoadTextureCoroutine(path, texture =>
    {
            if (texture != null)
            {
                // 压缩纹理
                if (compress)
                {
                    CompressTexture(texture);
                }

                //// 缩放纹理
                //if (texture.width > maxSize || texture.height > maxSize)
                //{
                //    texture = ResizeTexture(texture, maxSize, maxSize);
                //}

                // 缓存纹理
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
                textureCache[fullPath] = texture; // 缓存图片
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
    /// 清理未使用的缓存。
    /// </summary>
    public void ClearUnusedCache()
    {
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 清除所有缓存。
    /// </summary>
    public void ClearAllCache()
    {
        textureCache.Clear();
    }


    /// <summary>
    /// 对 Texture2D 进行压缩。
    /// </summary>
    private void CompressTexture(Texture2D texture)
    {
        // 检查是否为 4 的倍数
        int width = texture.width;
        int height = texture.height;

        if (width % 4 != 0 || height % 4 != 0)
        {
            int newWidth = Mathf.NextPowerOfTwo((width + 3) / 4 * 4);
            int newHeight = Mathf.NextPowerOfTwo((height + 3) / 4 * 4);

            // 调整尺寸
            texture = ResizeTexture(texture, newWidth, newHeight);
        }

        // 压缩纹理
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
