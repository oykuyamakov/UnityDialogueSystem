using UnityEngine;
using Object = UnityEngine.Object;

namespace Cadi.Scripts.CacherSystem
{
    [DefaultExecutionOrder(-10_000)]
    public abstract class CacherSingleton<T> : CacherMonoBehaviour
        where T : CacherSingleton<T>
    {
        private static T s_Instance;
        private static bool s_IsQuitting;

        /// <summary>
        /// True = keep alive across scene loads. False = scene-bound singleton.
        /// </summary>
        protected virtual bool m_PersistAcrossScenes => true;

        public static bool HasInstance => !s_IsQuitting && s_Instance != null;

        public static T Instance
        {
            get
            {
                if (s_IsQuitting)
                {
                    return null;
                }

                if (s_Instance)
                {
                    return s_Instance;
                }

                s_Instance = Object.FindFirstObjectByType<T>();
                if (s_Instance)
                {
                    return s_Instance;
                }

                CreateInstanceGameObject();
                return s_Instance;
            }
        }

        public static bool TryGetInstance(out T instance)
        {
            if (s_IsQuitting)
            {
                instance = null;
                return false;
            }

            if (s_Instance != null)
            {
                instance = s_Instance;
                return true;
            }

            s_Instance = Object.FindFirstObjectByType<T>();
            instance = s_Instance;
            return instance != null;
        }


        private static void CreateInstanceGameObject()
        {
            if (s_IsQuitting)
            {
                return;
            }

            if (s_Instance != null)
            {
                return;
            }

            var go = new GameObject(typeof(T).Name + "_Instance");

            go.AddComponent<T>();
        }

        protected sealed override void OnAwake()
        {
            if (!SingletonAwakeInternal())
                return;            // duplicate — skip resolution, we're being destroyed

            base.OnAwake();        // reference resolution
            OnSingletonAwake();
        }

        private bool SingletonAwakeInternal()
        {
            if (s_IsQuitting)
            {
                return false;
            }

            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return false;
            }

            s_Instance = (T)this;

            if (m_PersistAcrossScenes && Application.isPlaying)
            {
                Object.DontDestroyOnLoad(gameObject);
            }

            return true;
        }

        /// <summary>
        /// Hook for derived classes instead of Awake().
        /// Runs after singleton enforcement + after AutoReferenceMonoBehaviour.Awake() has run.
        /// </summary>
        protected virtual void OnSingletonAwake()
        {
        }

        protected virtual void OnApplicationQuit()
        {
            s_IsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }

            OnSingletonDestroyed();
        }

        /// <summary>
        /// Hook for derived classes instead of OnDestroy().
        /// </summary>
        protected virtual void OnSingletonDestroyed()
        {
        }
        
        private static void ResetStatics()
        {
            s_Instance = null;
            s_IsQuitting = false;
        }
    }
}