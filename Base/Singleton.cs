using UnityEngine;

namespace FPS.Base
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _instance = this as T;
        }
    }
}