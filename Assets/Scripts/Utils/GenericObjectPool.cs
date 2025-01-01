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

        // ��ʼ�������
        for (int i = 0; i < initialCapacity; i++)
        {
            T instance = CreateNewInstance(defaultParent);
            poolQueue.Enqueue(instance);
        }
    }

    /// <summary>
    /// ��ȡ����
    /// </summary>
    public T Get(Transform parent = null)
    {
        T instance;

        // �Ӷ���ػ�ȡ�򴴽���ʵ��
        if (poolQueue.Count > 0)
        {
            instance = poolQueue.Dequeue();
        }
        else
        {
            instance = CreateNewInstance(parent ?? defaultParent);
        }

        // ����ʵ���� Parent ����ʾ
        instance.transform.SetParent(parent ?? defaultParent, false);
        
        ShowObject(instance.gameObject);

        return instance;
    }

    /// <summary>
    /// �黹����
    /// </summary>
    public void Return(T instance)
    {
        // ���ز�����ΪĬ�ϸ���
        HideObject(instance.gameObject);
        instance.transform.SetParent(defaultParent, false);
        poolQueue.Enqueue(instance);
    }

    /// <summary>
    /// ������ʵ��
    /// </summary>
    private T CreateNewInstance(Transform parent)
    {
        T instance = Object.Instantiate(prefab, parent);
        EnsureCanvasGroup(instance.gameObject); // ȷ������ CanvasGroup
        HideObject(instance.gameObject);        // ��ʼ��Ϊ����״̬
        return instance;
    }

    /// <summary>
    /// ȷ������ӵ�� CanvasGroup ���
    /// </summary>
    private void EnsureCanvasGroup(GameObject obj)
    {
        if (!obj.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// ��ʾ����
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
    /// ���ض���
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
