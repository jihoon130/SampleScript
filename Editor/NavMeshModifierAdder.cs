using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshModifierAdder
{
    [MenuItem("Assets/AddNavMeshModifier")]
    private static void AddNavMeshModifierToPrefabs()
    {
        string folderPath = GetSelectedFolderPath();
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogWarning("No folder selected.");
            return;
        }

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                bool modified = false;

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                if (prefabRoot.GetComponent<NavMeshModifier>() == null)
                {
                    var nav = prefabRoot.AddComponent<NavMeshModifier>();
                    nav.overrideArea = true;
                    nav.area = 1;
                    modified = true;
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                    Debug.Log($"Added NavMeshModifier to: {path}");
                }

                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }

    private static string GetSelectedFolderPath()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (AssetDatabase.IsValidFolder(path))
        {
            return path;
        }
        return null;
    }

    [MenuItem("Assets/AddNavMeshModifier", true)]
    private static bool ValidateMenu()
    {
        return Selection.activeObject != null &&
               AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
    }
}
