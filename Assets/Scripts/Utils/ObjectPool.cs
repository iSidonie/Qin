using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> pool = new Queue<T>();

    public ObjectPool(T prefab, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
    }

    public T Get()
    {
        if (pool.Count > 0)
        {
            T instance = pool.Dequeue();
            instance.gameObject.SetActive(true);
            return instance;
        }
        else
        {
            return Object.Instantiate(prefab, parent);
        }
    }

    public void Return(T instance)
    {
        instance.gameObject.SetActive(false);
        pool.Enqueue(instance);
    }
}
