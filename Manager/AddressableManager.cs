using Cysharp.Threading.Tasks;
using FPS.Base;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressableManager : Singleton<AddressableManager>
{
    private Dictionary<string, Dictionary<string, Object>> preloadAssets
        = new Dictionary<string, Dictionary<string, Object>>();

    public async UniTask PreLoad<T>(string label) where T : Object
    {
        var locationHandle = Addressables.LoadResourceLocationsAsync(label);
        await locationHandle.ToUniTask();

        if (!preloadAssets.ContainsKey(label))
            preloadAssets[label] = new Dictionary<string, Object>();

        foreach (var location in locationHandle.Result)
        {
            string key = location.PrimaryKey;

            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.ToUniTask();

            preloadAssets[label][key] = handle.Result;
        }
    }


    public T Get<T>(string label, string name) where T : Object
    {
        if (preloadAssets.TryGetValue(label, out var dict) &&
            dict.TryGetValue(name, out var obj) &&
            obj is T casted)
        {
            return casted;
        }

        return null;
    }

    public async UniTask<T> GetAsync<T>(string label, string name) where T : Object
    {
        var locationHandle = Addressables.LoadResourceLocationsAsync(label);
        await locationHandle.ToUniTask();

        foreach (var location in locationHandle.Result)
        {
            if (location.PrimaryKey.Contains(name))
            {
                var loadHandle = Addressables.LoadAssetAsync<T>(location);
                await loadHandle.ToUniTask();

                return loadHandle.Result;
            }
        }

        return null;
    }


    public GameObject[] InitGameObjects(string label, Transform parent = null, bool isVisible = true)
    {
        if (preloadAssets.TryGetValue(label, out var objs))
        {
            GameObject[] results = new GameObject[objs.Count];

            int index = 0;
            foreach (var obj in objs)
            { 
                results[index] = GameObject.Instantiate(obj.Value as GameObject, parent);
                results[index].SetActive(isVisible);
                results[index].name = results[index].name.Replace("(Clone)", "");
                index++;
            }

            return results;
        }

        return null;
    }

    public GameObject InitGameObject(string label, string name, Transform parent = null, bool isVisible = true)
    {
        if (preloadAssets.TryGetValue(label, out var obj))
        {
            var ob = GameObject.Instantiate(obj[name] as GameObject);
            return ob;
        }

        return null;
    }

    public async UniTask<GameObject> InitGameObject(string name, Transform parent = null)
    {
        var handle = Addressables.InstantiateAsync(name, parent);
        await handle.ToUniTask();

        return handle.Result;
    }
}
