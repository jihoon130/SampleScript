using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializerScript : MonoBehaviour
{
    private bool isLoading = false;

    private async UniTask Start()
    {
        DataContainer.InitializeData();

        var preloadTasks = new[]
        {
        AddressableManager.Instance.PreLoad<GameObject>("Weapon"),
        AddressableManager.Instance.PreLoad<ScriptableObject>("WeaponData"),
        AddressableManager.Instance.PreLoad<ScriptableObject>("PlayerData"),
        AddressableManager.Instance.PreLoad<GameObject>("Character"),
    };

        await UniTask.WhenAll(preloadTasks);

        isLoading = true;
    }

    private void Update()
    {
        if (isLoading && Input.GetKeyDown(KeyCode.Q))
        {
            SceneManager.LoadSceneAsync(1);
        }
    }
}
