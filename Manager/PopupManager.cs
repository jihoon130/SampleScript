using FPS.Base;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class PopupManager : Singleton<PopupManager>
{
    private Dictionary<Type, PopupPresenterBase> popupDict;
    private Dictionary<Type, AsyncOperationHandle<GameObject>> handleDict;
    private Canvas popupCanvas;

    protected override void InitializeSingleton()
    {
        base.InitializeSingleton();

        if (popupCanvas == null)
        {
            GameObject canvasObject = new GameObject("PopupCanvas");
            popupCanvas = canvasObject.AddComponent<Canvas>();
            popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            popupCanvas.gameObject.AddComponent<CanvasScaler>();
            popupCanvas.gameObject.AddComponent<GraphicRaycaster>();

            DontDestroyOnLoad(popupCanvas.gameObject);
        }

        popupDict = new Dictionary<Type, PopupPresenterBase>();
        handleDict = new Dictionary<Type, AsyncOperationHandle<GameObject>>();
    }

    public async UniTask<T> Get<T>(object args = null) where T : PopupPresenterBase, new()
    {
        if (popupDict.ContainsKey(typeof(T)))
        {
            PopupPresenterBase popup = popupDict[typeof(T)];
            return (T)popup;
        }

        string address = typeof(T).Name.Replace("Presenter", "");
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);

        await handle.ToUniTask();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject popupObject = Instantiate(handle.Result);
            popupObject.transform.SetParent(popupCanvas.transform, false);

            var newPopup = new T();

            newPopup.InjectView(popupObject.GetComponent<ViewBase>());
            newPopup.onCreate(args);
            popupDict[typeof(T)] = newPopup;
            handleDict[typeof(T)] = handle;

            popupObject.SetActive(true);
            MessageBroker.Default.Publish(new InputDisableEvent { });

            return newPopup;
        }
        else
        {
            Debug.LogError($"Failed to load popup of type {typeof(T).Name}");
            return null;
        }
    }

    public void Return(Type type)
    {
        if (popupDict.ContainsKey(type))
        {
            PopupPresenterBase popup = popupDict[type];
            popup.onDestroy();

            if (handleDict.ContainsKey(type))
            {
                Addressables.Release(handleDict[type]);
                handleDict.Remove(type);
            }

            popupDict.Remove(type);
        }

        if (popupDict.Count == 0)
            MessageBroker.Default.Publish(new InputEnableEvent { });
    }

}
