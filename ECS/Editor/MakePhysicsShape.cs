using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;

namespace FPS
{
    public class PrefabProcessor : Editor
    {
        [MenuItem("Assets/Process All Prefabs in Folder", false, 50)]
        private static void ProcessPrefabsInFolder()
        {
            // 선택한 폴더의 경로 가져오기
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("선택한 항목이 폴더가 아닙니다.");
                return;
            }

            // 폴더 내 모든 프리팹 찾기
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab != null)
                {
                    Make(prefab);
                    Debug.Log($"Processed: {prefab.name}");
                }
            }

            // 변경 사항 저장
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void Make(GameObject obj)
        {
            // 선택된 오브젝트가 없으면 오류 표시
            if (obj == null)
            {
                return;
            }

            MeshCollider meshCollider = obj.GetComponent<MeshCollider>();

            if (meshCollider == null)
            {
                return;
            }


            var physicsShape = obj.AddComponent<PhysicsShapeAuthoring>();

            physicsShape.SetMesh(meshCollider.sharedMesh);
        }

        [MenuItem("Assets/Create/Remove MeshCollider from Prefabs")]
        private static void RemoveMeshColliders()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("선택한 항목이 폴더가 아닙니다.");
                return;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefabAsset != null)
                {
                    // 프리팹 인스턴스 생성
                    GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);

                    if (prefabInstance != null)
                    {
                        MeshCollider meshCollider = prefabInstance.GetComponent<MeshCollider>();
                        if (meshCollider != null)
                        {
                            DestroyImmediate(meshCollider);
                            Debug.Log($"Removed MeshCollider from: {prefabAsset.name}");

                            // 변경 사항 적용
                            PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.AutomatedAction);
                        }

                        // 인스턴스 삭제
                        DestroyImmediate(prefabInstance);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}