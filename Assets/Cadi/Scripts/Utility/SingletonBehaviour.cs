using UnityEngine;
using Object = UnityEngine.Object;

namespace Cadi.Scripts.Utility
{
    /// <summary>
    /// Unity singleton base that supports:
    /// - Instance can already exist in scene 
    /// - If missing, it auto-creates on first access
    /// - Duplicate protection
    /// - Safe behavior on application quit (no "ghost singleton" recreation)
    /// - Plays well with "Domain Reload disabled"
    /// </summary>
    [DefaultExecutionOrder(-ExecOrder.SINGLETON)] // Ensure it initializes early vs typical gameplay scripts.
    public abstract class SingletonBehaviour<T> : MonoBehaviour
        where T : SingletonBehaviour<T>
    {
        private static T s_Instance;

        /// <summary>
        /// Override if you want a scene-bound singleton (false) vs global singleton (true).
        /// </summary>
        protected virtual bool m_PersistAcrossScenes => true;

        /// <summary>
        /// Main accessor. Fast path is a single null check.
        /// </summary>
        public static T Instance
        {
            get
            {
                // During shutdown Unity destroys objects; creating new ones causes editor/runtime weirdness.
                if (!Application.isPlaying)
                {
                    return null;
                }

                // Hot path: one check, no search, no allocations.
                if (s_Instance != null)
                {
                    return s_Instance;
                }

                // Slow path: try find an existing instance in loaded scenes.
                s_Instance = FindExistingInstance();

                if (s_Instance != null)
                {
                    return s_Instance;
                }

                CreateInstanceGameObject();
                return s_Instance;
            }
        }

        /// <summary>
        /// Non-creating check (useful when you want to avoid creating singletons implicitly).
        /// </summary>
        public static bool TryGetInstance(out T instance)
        {
            instance = !Application.isPlaying ? null : s_Instance;
            if (instance != null)
            {
                return true;
            }

            instance = FindExistingInstance();
            s_Instance = instance;
            return instance != null;
        }

        /// <summary>
        /// Base enforcement: duplicates are killed and the instance is registered reliably.
        /// </summary>
        protected virtual void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                // - Keep the first registered instance
                // - Destroy later duplicates
                Destroy(gameObject);
                return;
            }

            s_Instance = (T)this;

            if (m_PersistAcrossScenes && Application.isPlaying)
            {
                Object.DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            // If the singleton is destroyed (scene unload, manual destroy), allow recreation later.
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        private static T FindExistingInstance()
        {
            // FindFirstObjectByType is efficient and avoids allocating an array.
            // Note: it typically ignores inactive objects; this is usually desired for singletons.
            // If you need to include inactive, swap to FindAnyObjectByType with FindObjectsInactive.Include (Unity 2023+).
            return Object.FindFirstObjectByType<T>();
        }

        private static void CreateInstanceGameObject()
        {
            if (s_Instance != null)
                return;

            var go = new GameObject(typeof(T).Name + "_Instance");
            s_Instance = go.AddComponent<T>();
        }
    }
}