using System.Collections.Generic;
using UnityEngine;

public class GenericObjectPool<T> where T : Component
{
    private Queue<T> poolQueue = new Queue<T>();
    private T prefab;
    private Transform defaultParent;

    public GenericObjectPool(T prefab, Transform defaultParent, int initialCapacity = 10)
    {
        this.prefab = prefab;
        this.defaultParent = defaultParent;

        // 初始化对象池
        for (int i = 0; i < initialCapacity; i++)
        {
            T instance = CreateNewInstance(defaultParent);
            poolQueue.Enqueue(instance);
        }
    }

    /// <summary>
    /// 获取对象
    /// </summary>
    public T Get(Transform parent = null)
    {
        T instance;

        // 从对象池获取或创建新实例
        if (poolQueue.Count > 0)
        {
            instance = poolQueue.Dequeue();
        }
        else
        {
            instance = CreateNewInstance(parent ?? defaultParent);
        }

        // 设置实例的 Parent 并显示
        instance.transform.SetParent(parent ?? defaultParent, false);
        
        ShowObject(instance.gameObject);

        return instance;
    }

    /// <summary>
    /// 归还对象
    /// </summary>
    public void Return(T instance)
    {
        // 隐藏并设置为默认父级
        HideObject(instance.gameObject);
        instance.transform.SetParent(defaultParent, false);
        poolQueue.Enqueue(instance);
    }

    /// <summary>
    /// 创建新实例
    /// </summary>
    private T CreateNewInstance(Transform parent)
    {
        T instance = Object.Instantiate(prefab, parent);
        EnsureCanvasGroup(instance.gameObject); // 确保存在 CanvasGroup
        HideObject(instance.gameObject);        // 初始化为隐藏状态
        return instance;
    }

    /// <summary>
    /// 确保对象拥有 CanvasGroup 组件
    /// </summary>
    private void EnsureCanvasGroup(GameObject obj)
    {
        if (!obj.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// 显示对象
    /// </summary>
    private void ShowObject(GameObject obj)
    {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null) return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        obj.SetActive(true);
    }

    /// <summary>
    /// 隐藏对象
    /// </summary>
    private void HideObject(GameObject obj)
    {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        obj.SetActive(false);
    }
}
