using System.Collections.Generic;
using UnityEngine;

public partial class ObjectPool
{
    private readonly Dictionary<string, Queue<GameObject>> pool = new();

    public GameObject Get(string name)
    {
        if (!pool.ContainsKey(name))
        {
            var newObj = AddressableManager.Instance.InitGameObject("Character", name);
            return newObj;
        }

        var obj = pool[name].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);

        if (pool.ContainsKey(obj.name))
            pool[obj.name] = new();

        pool[obj.name].Enqueue(obj);
    }
}
