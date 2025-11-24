using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPool<T> where T : Component, IPoolable
{
    private readonly Stack<T> _pool = new Stack<T>();
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly int _initialSize;

    public int ActiveCount => _pool.Count(x => x.IsActive);
    public int TotalCount => _pool.Count;

    public ObjectPool(T prefab, Transform parent, int initialSize = 10)
    {
        _prefab = prefab;
        _parent = parent;
        _initialSize = initialSize;
        
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialSize; i++)
        {
            CreateNewObject();
        }
    }

    private T CreateNewObject()
    {
        var obj = UnityEngine.Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        _pool.Push(obj);
        return obj;
    }

    public T Get()
    {
        T obj = _pool.Count > 0 ? _pool.Pop() : CreateNewObject();
        obj.gameObject.SetActive(true);
        obj.OnSpawn();
        return obj;
    }

    public void Return(T obj)
    {
        obj.OnDespawn();
        obj.gameObject.SetActive(false);
        _pool.Push(obj);
    }

    public void Clear()
    {
        foreach (var obj in _pool)
        {
            if (obj != null)
                UnityEngine.Object.Destroy(obj.gameObject);
        }
        _pool.Clear();
    }
}