using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace FPS
{
    public class ModelBakerEditor
    {
        // 하이어라키에서 오브젝트를 오른쪽 클릭했을 때 나타날 메뉴 항목
        [MenuItem("GameObject/MVP/Create MVP Script", false, 10)]
        private static void CreateMVPScriptForSelectedObject()
        {
            // 하이어라키에서 선택된 오브젝트가 존재하는지 확인
            GameObject selectedObject = Selection.activeGameObject;

            // 선택된 오브젝트가 없으면 오류 표시
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog("No Object Selected", "Please select an object in the hierarchy.", "OK");
                return;
            }

            // MVP 스크립트 생성
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

        // MVP 스크립트 생성
        public static void CreateMVPScripts(string name, GameObject selectedObject)
        {
            // 모델 스크립트 경로 및 내용
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

            // 프레젠터 스크립트 경로 및 내용
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

            // 기존 View 스크립트가 있다면 필드만 갱신
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

            // View 스크립트가 생성된 후, 해당 스크립트를 선택된 오브젝트에 자동으로 추가
            AddViewScriptToPrefab(selectedObject, name);
        }

        // 새로운 View 스크립트 생성
        private static string GenerateViewScript(string name, GameObject selectedObject)
        {
            List<string> fields = new List<string>();

            // 자식 오브젝트 순회
            foreach (Transform child in selectedObject.transform)
            {
                AddComponentFieldForChild(child, ref fields);
            }

            // 생성될 View 클래스 템플릿
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

            // '!' → 무시
            if (originalName.EndsWith("!"))
                return;

            // '*' → 자기 자신 추가 + 직계 자식만 필드 추가
            if (originalName.EndsWith("*"))
            {
                AddComponentFieldForSelf(current, ref fields);

                foreach (Transform child in current)
                {
                    AddComponentFieldForSelf(child, ref fields); // 자식 1단계만
                }

                return;
            }

            // '_' → 자기 자신 추가 + 자식들을 prefix 포함해 재귀 순회
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

            // '#' → 자기 자신은 무시, 자식은 일반 이름으로 필드 추가
            if (originalName.EndsWith("#"))
            {
                foreach (Transform child in current)
                {
                    AddComponentFieldForChild(child, ref fields); // 재귀 순회
                }

                return;
            }

            // 일반 이름 → 자기 자신만 필드로 추가
            AddComponentFieldForSelf(current, ref fields);
        }


        private static void AddComponentFieldForChildWithPrefix(Transform child, ref List<string> fields, string parentPrefix)
        {
            // '!' 또는 '*' 또는 '#'으로 끝나면 prefix 처리 안 함
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



        // 이름에서 '~', '*', '_' 제거하는 함수
        private static string RemoveSpecialCharacter(string name)
        {
            if (name.EndsWith("~") || name.EndsWith("*") || name.EndsWith("_") || name.EndsWith("!") || name.EndsWith("#"))
            {
                return name.Substring(0, name.Length - 1);
            }
            return name;
        }






        // 기존 View 스크립트 갱신
        private static void UpdateViewScriptFields(string viewPath, GameObject selectedObject)
        {
            // 기존 View 스크립트 읽기
            string viewScript = File.ReadAllText(viewPath);

            List<string> fields = new List<string>();

            // 자식 오브젝트 순회하여 컴포넌트 추가
            foreach (Transform child in selectedObject.transform)
            {
                AddComponentFieldForChild(child, ref fields);
            }

            // 기존의 SerializeField 목록을 업데이트
            string fieldDefinitions = string.Join("\n        ", fields);
            string updatedViewScript = UpdateFieldsInViewScript(viewScript, fieldDefinitions);

            // 갱신된 스크립트를 파일에 저장
            File.WriteAllText(viewPath, updatedViewScript);

            // 유니티 에디터에서 새로 생성된 파일을 로드
            AssetDatabase.Refresh();

            // 로그로 출력
            Debug.Log("Updated View script at: " + viewPath);
        }

        private static string UpdateFieldsInViewScript(string viewScript, string newFields)
        {
            // [SerializeField] public 필드 부분을 찾아서 새로 생성된 필드들로 갱신
            int startIndex = viewScript.IndexOf("{");
            int endIndex = viewScript.LastIndexOf("}");

            if (startIndex < 0 || endIndex < 0) return viewScript;

            string beforeFields = viewScript.Substring(0, startIndex + 1);
            string afterFields = viewScript.Substring(endIndex);

            return beforeFields + "\n        " + newFields + "\n" + afterFields;
        }

        private static void AddViewScriptToPrefab(GameObject selectedObject, string name)
        {
            // 스크립트 파일 경로
            string viewPath = "Assets/Scripts/MVP/View/" + name + "View.cs";

            // 스크립트 파일이 존재하는지 확인
            if (File.Exists(viewPath))
            {
                // 스크립트를 로드
                MonoScript viewScript = AssetDatabase.LoadAssetAtPath<MonoScript>(viewPath);

                if (viewScript != null)
                {
                    // 해당 스크립트에서 MonoBehaviour를 추출하여 타입을 가져옴
                    Type viewType = viewScript.GetClass();

                    // 프리팹 인스턴스를 생성하고, 해당 타입을 컴포넌트로 추가
                    GameObject prefabInstance = PrefabUtility.InstantiatePrefab(selectedObject) as GameObject;

                    if (viewType != null)
                    {
                        prefabInstance.AddComponent(viewType); // 컴포넌트를 추가

                        // 프리팹을 갱신하여 저장
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


        // 스크립트를 파일로 생성하는 메소드
        private static void CreateScript(string path, string scriptTemplate)
        {
            // 이미 같은 이름의 스크립트가 있으면 경고 출력
            if (File.Exists(path))
            {
                Debug.LogWarning("Script with this name already exists: " + path);
                return;
            }

            // 스크립트 파일 생성
            File.WriteAllText(path, scriptTemplate);

            // 유니티 에디터에서 새로 생성된 파일을 로드
            AssetDatabase.Refresh();

            // 생성 완료 로그
            Debug.Log("Script created at: " + path);
        }
    }
}
