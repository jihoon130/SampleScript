using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace FPS
{
    public class ModelBakerEditor
    {
        // ���̾��Ű���� ������Ʈ�� ������ Ŭ������ �� ��Ÿ�� �޴� �׸�
        [MenuItem("GameObject/MVP/Create MVP Script", false, 10)]
        private static void CreateMVPScriptForSelectedObject()
        {
            // ���̾��Ű���� ���õ� ������Ʈ�� �����ϴ��� Ȯ��
            GameObject selectedObject = Selection.activeGameObject;

            // ���õ� ������Ʈ�� ������ ���� ǥ��
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog("No Object Selected", "Please select an object in the hierarchy.", "OK");
                return;
            }

            // MVP ��ũ��Ʈ ����
            string prefabName = selectedObject.name;
            CreateMVPScripts(prefabName, selectedObject);
        }

        [MenuItem("Assets/Create/MVP/Create MVP Model Script")]
        public static void CreateModelScript()
        {
            string path = EditorUtility.SaveFilePanel("Create MVP Model Script", "Assets/Scripts/MVP/Model", "", "cs");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            
            if (!fileNameWithoutExtension.EndsWith("Model"))
            {
                fileNameWithoutExtension += "Model";
            }

            string modelTemplate = @"
using UnityEngine;
using UniRx;

namespace FPS.MVP
{
    public class " + fileNameWithoutExtension + @" : ModelBase
    {
    }
}";

            if (Path.GetExtension(path) != ".cs")
            {
                path = Path.Combine(Path.GetDirectoryName(path), fileNameWithoutExtension + ".cs");
            }
            else
            {
                path = Path.Combine(Path.GetDirectoryName(path), fileNameWithoutExtension + Path.GetExtension(path));
            }

            if (File.Exists(path))
            {
                Debug.LogError("File already exists at: " + path);
                return;
            }

            File.WriteAllText(path, modelTemplate);

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/MVP/Create MVP Presenter Script")]
        public static void CreatePresenterScript()
        {
            string path = EditorUtility.SaveFilePanel("Create MVP Model Script", "Assets/Scripts/MVP/Presenter", "", "cs");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            if (!fileNameWithoutExtension.EndsWith("Presenter"))
            {
                fileNameWithoutExtension += "Presenter";
            }

            string presenterTemplate = @"
using UnityEngine;
using UniRx;
using FPS.Base;

namespace FPS.MVP
{
    public class " + fileNameWithoutExtension + @" : PopupBase
    {
        public new class Args : PopupBase.Args
        {
        };

        private new Args args;

        public override void onCreate(object args)
        {
            this.args = (Args)args;
            base.onCreate(args);
        }

        public override void onDestroy()
        {
            base.onDestroy();
        }
    }
}
";

            if (Path.GetExtension(path) != ".cs")
            {
                path = Path.Combine(Path.GetDirectoryName(path), fileNameWithoutExtension + ".cs");
            }
            else
            {
                path = Path.Combine(Path.GetDirectoryName(path), fileNameWithoutExtension + Path.GetExtension(path));
            }

            if (File.Exists(path))
            {
                Debug.LogError("File already exists at: " + path);
                return;
            }

            File.WriteAllText(path, presenterTemplate);

            AssetDatabase.Refresh();
        }

        [MenuItem("GameObject/MVP/Create MVP View Script", false, 10)]
        public static void CreateViewScript()
        {
            string path = EditorUtility.SaveFilePanel("Create MVP View Script", "Assets/Scripts/MVP/View", "", "cs");
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog("No Object Selected", "Please select an object in the hierarchy.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

            if (!fileNameWithoutExtension.EndsWith("View"))
            {
                fileNameWithoutExtension += "View";
            }

            if (Path.GetExtension(path) != ".cs")
            {
                path = Path.Combine(Path.GetDirectoryName(path), fileNameWithoutExtension + ".cs");
            }
            else
            {
                path = Path.Combine(Path.GetDirectoryName(path), fileNameWithoutExtension + Path.GetExtension(path));
            }

            if (File.Exists(path))
            {
                UpdateViewScriptFields(path, selectedObject);
            }
            else
            {
                string name = fileNameWithoutExtension.Replace("View", "");
                string viewTemplate = GenerateViewScript(name, selectedObject);
                File.WriteAllText(path, viewTemplate);
            }

            

            AssetDatabase.Refresh();
        }

        // MVP ��ũ��Ʈ ����
        public static void CreateMVPScripts(string name, GameObject selectedObject)
        {
            // �� ��ũ��Ʈ ��� �� ����
            string modelPath = "Assets/Scripts/MVP/Model/" + name + "Model.cs";

            string modelTemplate = @"
using UnityEngine;
using UniRx;

namespace FPS.MVP
{
    public class " + name + "Model" + @" : ModelBase
    {
    }
}";

            CreateScript(modelPath, modelTemplate);

            // �������� ��ũ��Ʈ ��� �� ����
            string presenterPath = "Assets/Scripts/MVP/Presenter/" + name + "Presenter.cs";
            string presenterTemplate = @"
using UnityEngine;
using UniRx;
using FPS.Base;
using FPS.Attribute;

namespace FPS.MVP
{
    public class " + name + "Presenter" + @" : PopupPresenterBase
    {
        public new class Args : PopupBase.Args
        {
        };

        private new Args args;

        [Inject] private " + name + "Model" + @" model;
        private " + name + "View" + @" view;



        public override void onCreate(object args)
        {
            this.args = (Args)args;
            base.onCreate(args);
        }

        public override void InjectView(ViewBase view)
        {
            base.InjectView(view);
            view = " + "(" + name + "View" + ")" + "view" + @";
        }

        public override void onDestroy()
        {
            base.onDestroy();
        }
    }
}
";

            CreateScript(presenterPath, presenterTemplate);

            // ���� View ��ũ��Ʈ�� �ִٸ� �ʵ常 ����
            string viewPath = "Assets/Scripts/MVP/View/" + name + "View.cs";
            if (File.Exists(viewPath))
            {
                UpdateViewScriptFields(viewPath, selectedObject);
            }
            else
            {
                string viewTemplate = GenerateViewScript(name, selectedObject);
                CreateScript(viewPath, viewTemplate);
            }

            // View ��ũ��Ʈ�� ������ ��, �ش� ��ũ��Ʈ�� ���õ� ������Ʈ�� �ڵ����� �߰�
            AddViewScriptToPrefab(selectedObject, name);
        }

        // ���ο� View ��ũ��Ʈ ����
        private static string GenerateViewScript(string name, GameObject selectedObject)
        {
            List<string> fields = new List<string>();

            // �ڽ� ������Ʈ ��ȸ
            foreach (Transform child in selectedObject.transform)
            {
                AddComponentFieldForChild(child, ref fields);
            }

            // ������ View Ŭ���� ���ø�
            string viewTemplate = @"
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace FPS.MVP
{
    public class " + name + "View" + @" : ViewBase
    {
        " + string.Join("\n        ", fields) + @"
    }
}
";

            return viewTemplate;
        }

        private static void AddComponentFieldForChild(Transform current, ref List<string> fields)
        {
            string originalName = current.name;

            // '!' �� ����
            if (originalName.EndsWith("!"))
                return;

            // '*' �� �ڱ� �ڽ� �߰� + ���� �ڽĸ� �ʵ� �߰�
            if (originalName.EndsWith("*"))
            {
                AddComponentFieldForSelf(current, ref fields);

                foreach (Transform child in current)
                {
                    AddComponentFieldForSelf(child, ref fields); // �ڽ� 1�ܰ踸
                }

                return;
            }

            // '_' �� �ڱ� �ڽ� �߰� + �ڽĵ��� prefix ������ ��� ��ȸ
            if (originalName.EndsWith("_"))
            {
                string thisName = RemoveSpecialCharacter(originalName);
                AddComponentFieldForSelf(current, ref fields);

                foreach (Transform child in current)
                {
                    AddComponentFieldForChildWithPrefix(child, ref fields, thisName);
                }

                return;
            }

            // '#' �� �ڱ� �ڽ��� ����, �ڽ��� �Ϲ� �̸����� �ʵ� �߰�
            if (originalName.EndsWith("#"))
            {
                foreach (Transform child in current)
                {
                    AddComponentFieldForChild(child, ref fields); // ��� ��ȸ
                }

                return;
            }

            // �Ϲ� �̸� �� �ڱ� �ڽŸ� �ʵ�� �߰�
            AddComponentFieldForSelf(current, ref fields);
        }


        private static void AddComponentFieldForChildWithPrefix(Transform child, ref List<string> fields, string parentPrefix)
        {
            // '!' �Ǵ� '*' �Ǵ� '#'���� ������ prefix ó�� �� ��
            if (child.name.EndsWith("!") || child.name.EndsWith("*") || child.name.EndsWith("#"))
                return;

            Component[] components = child.GetComponents<Component>();
            if (components.Length >= 3)
            {
                Component thirdComponent = components[2];
                string fieldType = thirdComponent.GetType().Name;
                string fieldName = RemoveSpecialCharacter(child.name);

                string fullName = $"{parentPrefix}_{fieldName}";
                fields.Add($"[SerializeField] public {fieldType} {fullName};");
            }

            foreach (Transform grandChild in child)
            {
                AddComponentFieldForChildWithPrefix(grandChild, ref fields, parentPrefix);
            }
        }


        private static void AddComponentFieldForSelf(Transform self, ref List<string> fields)
        {
            Component[] components = self.GetComponents<Component>();

            if (components.Length >= 3)
            {
                Component component = components[2];
                string fieldType = component.GetType().Name;
                string fieldName = RemoveSpecialCharacter(self.name);

                fields.Add($"[SerializeField] public {fieldType} {fieldName};");
            }
        }



        // �̸����� '~', '*', '_' �����ϴ� �Լ�
        private static string RemoveSpecialCharacter(string name)
        {
            if (name.EndsWith("~") || name.EndsWith("*") || name.EndsWith("_") || name.EndsWith("!") || name.EndsWith("#"))
            {
                return name.Substring(0, name.Length - 1);
            }
            return name;
        }






        // ���� View ��ũ��Ʈ ����
        private static void UpdateViewScriptFields(string viewPath, GameObject selectedObject)
        {
            // ���� View ��ũ��Ʈ �б�
            string viewScript = File.ReadAllText(viewPath);

            List<string> fields = new List<string>();

            // �ڽ� ������Ʈ ��ȸ�Ͽ� ������Ʈ �߰�
            foreach (Transform child in selectedObject.transform)
            {
                AddComponentFieldForChild(child, ref fields);
            }

            // ������ SerializeField ����� ������Ʈ
            string fieldDefinitions = string.Join("\n        ", fields);
            string updatedViewScript = UpdateFieldsInViewScript(viewScript, fieldDefinitions);

            // ���ŵ� ��ũ��Ʈ�� ���Ͽ� ����
            File.WriteAllText(viewPath, updatedViewScript);

            // ����Ƽ �����Ϳ��� ���� ������ ������ �ε�
            AssetDatabase.Refresh();

            // �α׷� ���
            Debug.Log("Updated View script at: " + viewPath);
        }

        private static string UpdateFieldsInViewScript(string viewScript, string newFields)
        {
            // [SerializeField] public �ʵ� �κ��� ã�Ƽ� ���� ������ �ʵ��� ����
            int startIndex = viewScript.IndexOf("{");
            int endIndex = viewScript.LastIndexOf("}");

            if (startIndex < 0 || endIndex < 0) return viewScript;

            string beforeFields = viewScript.Substring(0, startIndex + 1);
            string afterFields = viewScript.Substring(endIndex);

            return beforeFields + "\n        " + newFields + "\n" + afterFields;
        }

        private static void AddViewScriptToPrefab(GameObject selectedObject, string name)
        {
            // ��ũ��Ʈ ���� ���
            string viewPath = "Assets/Scripts/MVP/View/" + name + "View.cs";

            // ��ũ��Ʈ ������ �����ϴ��� Ȯ��
            if (File.Exists(viewPath))
            {
                // ��ũ��Ʈ�� �ε�
                MonoScript viewScript = AssetDatabase.LoadAssetAtPath<MonoScript>(viewPath);

                if (viewScript != null)
                {
                    // �ش� ��ũ��Ʈ���� MonoBehaviour�� �����Ͽ� Ÿ���� ������
                    Type viewType = viewScript.GetClass();

                    // ������ �ν��Ͻ��� �����ϰ�, �ش� Ÿ���� ������Ʈ�� �߰�
                    GameObject prefabInstance = PrefabUtility.InstantiatePrefab(selectedObject) as GameObject;

                    if (viewType != null)
                    {
                        prefabInstance.AddComponent(viewType); // ������Ʈ�� �߰�

                        // �������� �����Ͽ� ����
                        PrefabUtility.SaveAsPrefabAsset(prefabInstance, AssetDatabase.GetAssetPath(selectedObject));
                        UnityEngine.Object.DestroyImmediate(prefabInstance);

                        Debug.Log("Added " + name + "View script to the prefab.");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to add the view script. The script type could not be found.");
                    }
                }
                else
                {
                    Debug.LogWarning("The script could not be found at: " + viewPath);
                }
            }
            else
            {
                Debug.LogWarning("The script file does not exist at: " + viewPath);
            }
        }


        // ��ũ��Ʈ�� ���Ϸ� �����ϴ� �޼ҵ�
        private static void CreateScript(string path, string scriptTemplate)
        {
            // �̹� ���� �̸��� ��ũ��Ʈ�� ������ ��� ���
            if (File.Exists(path))
            {
                Debug.LogWarning("Script with this name already exists: " + path);
                return;
            }

            // ��ũ��Ʈ ���� ����
            File.WriteAllText(path, scriptTemplate);

            // ����Ƽ �����Ϳ��� ���� ������ ������ �ε�
            AssetDatabase.Refresh();

            // ���� �Ϸ� �α�
            Debug.Log("Script created at: " + path);
        }
    }
}
