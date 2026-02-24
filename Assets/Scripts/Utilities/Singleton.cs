using UnityEngine;

namespace DesertArena.Utilities
{
    /// <summary>
    /// Generic singleton base class for MonoBehaviours.
    /// Ensures only one instance exists and optionally persists across scenes.
    /// </summary>
    /// <typeparam name="T">The MonoBehaviour type that should be a singleton.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Fields

        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting;

        [SerializeField]
        [Tooltip("If true, this singleton will persist across scene loads.")]
        private bool _persistAcrossScenes = true;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the singleton instance. Creates one if it doesn't exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed on application quit.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindAnyObjectByType<T>();

                        if (_instance == null)
                        {
                            var singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                            _instance = singletonObject.AddComponent<T>();
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Returns true if an instance currently exists.
        /// </summary>
        public static bool HasInstance => _instance != null;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;

            if (_persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnSingletonAwake();
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Called after the singleton instance is established in Awake.
        /// Override this instead of Awake in derived classes.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        #endregion
    }
}
