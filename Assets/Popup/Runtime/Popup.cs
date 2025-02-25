using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DarkNaku.Popup
{
    public sealed class Popup : MonoBehaviour 
    {
        public static Popup Instance
        {
            get
            {
                if (_isDestroyed) return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var instances = FindObjectsByType<Popup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                        if (instances.Length > 0)
                        {
                            _instance = instances[0];

                            for (int i = 1; i < instances.Length; i++)
                            {
                                Debug.LogWarningFormat("[Popup] Instance Duplicated - {0}", instances[i].name);
                                Destroy(instances[i]);
                            }
                        }
                        else
                        {
                            _instance = new GameObject($"[Singleton] Popup").AddComponent<Popup>();
                        }
                    }

                    return _instance;
                }
            }
        }

        public static bool EscapeEnabled { get; set; } = true;
        
        public static Canvas MainCanvas
        {
            get => Instance._mainCanvas;
            set => Instance._mainCanvas = value;
        }

        public static int BaseSortingOrder
        {
            get => Instance._baseSortingOrder;
            set => Instance._baseSortingOrder = value;
        }
        
        public static float EscapeMinInterval
        {
            get => Instance._escapeMinInterval;
            set => Instance._escapeMinInterval = value;
        }
        
        public static IPopupHandler Current => Instance._Current;
        public static bool IsAnyPopupShow => Instance._popups.Count > 0;
        public static UnityEvent<string, IPopupHandler> OnPopupShow => Instance._onPopupShow;
        public static UnityEvent<string, IPopupHandler> OnPopupHide => Instance._onPopupHide;
        
        private static readonly object _lock = new();
        private static Popup _instance;
        private static bool _isDestroyed;
        
        private bool InteractableMainCanvas
        {
            get => MainGraphicRaycaster != null && MainGraphicRaycaster.enabled;
            set
            {
                if (MainGraphicRaycaster != null)
                {
                    MainGraphicRaycaster.enabled = value;
                }
            }
        }
        
        private GraphicRaycaster MainGraphicRaycaster
        {
            get
            {
                if (_mainGraphicRaycaster == null && _mainCanvas != null)
                {
                    _mainGraphicRaycaster ??= _mainCanvas.GetComponent<GraphicRaycaster>();
                }

                return _mainGraphicRaycaster;
            }
        }
        
        private IPopupHandler _Current => (_popups.Count > 0) ? _popups[^1] : null;

        private Canvas _mainCanvas;
        private int _baseSortingOrder = 10; 
        private float _escapeMinInterval = 1f; 
        
        private float _escapePressedTime;
        private GraphicRaycaster _mainGraphicRaycaster;
        private List<IPopupHandler> _popups = new List<IPopupHandler>();
        private Dictionary<string, GameObject> _builtInTable = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, IPopupHandler> _popupTable = new Dictionary<string, IPopupHandler>();
        private readonly UnityEvent<string, IPopupHandler> _onPopupShow = new UnityEvent<string, IPopupHandler>();
        private readonly UnityEvent<string, IPopupHandler> _onPopupHide = new UnityEvent<string, IPopupHandler>();
        
        public static void Register(string key, IPopupHandler handler)
        {
            Instance._Register(key, handler);
        }

        public static void Unregister(string key)
        {
            Instance._Unregister(key);
        }

        public static IPopupHandler Show(string key)
        {
            return Instance._Show<IPopupHandler>(key);
        }
        
        public static IPopupHandler Hide(string key)
        {
            return Instance._Hide<IPopupHandler>(key);
        }

        public static IPopupHandler Hide(IPopupHandler handler)
        {
            return Instance._Hide<IPopupHandler>(handler);
        }

        public static T Show<T>(string key) where T : class, IPopupHandler
        {
            return Instance._Show<T>(key);
        }
        
        public static T Hide<T>(string key) where T : class, IPopupHandler
        {
            return Instance._Hide<T>(key);
        }

        public static T Hide<T>(IPopupHandler handler) where T : class, IPopupHandler
        {
            return Instance._Hide<T>(handler);
        }
        
        public static Coroutine HideAll(params string[] excepts)
        {
            return Instance.StartCoroutine(Instance.CoHideAll(excepts));
        }

        public static bool IsPopupShow(string key)
        {
            return Instance._popupTable.ContainsKey(key) && Instance._popupTable[key].IsShow;   
        }
        
        public static T Get<T>() where T : class, IPopupHandler
        {
            return Instance._Get<T>();
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            _instance = null;
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Popup] Duplicated - {name}");
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (_instance != this) return;
            
            _instance = null;
            _isDestroyed = true;
            
            var keys = new List<string>(_popupTable.Keys);
            
            foreach (var key in keys)
            {
                _Unregister(key);
            }
            
            Debug.Log($"[Popup] Destroyed.");
        }
        
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape)) Escape();
        }

        private void Escape()
        {
            if (EscapeEnabled == false) return;
            if (_Current == null) return;
            if (_Current.IsInTransition) return;
            if (Time.realtimeSinceStartup - _escapePressedTime < _escapeMinInterval) return;

            Current.OnEscape();

            _escapePressedTime = Time.realtimeSinceStartup;
        }
        
        private void _Register(string key, IPopupHandler handler)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[Popup] Register : Key is null or empty.");
                return;
            }

            if (_popupTable.ContainsKey(key))
            {
                Debug.LogErrorFormat("[Popup] Register : Popup is already registed - {0}", key);
                return;
            }
            
            handler.Initialize();

            _popupTable.Add(key, handler);
        }

        private void _Unregister(string key)
        {
            if (_popupTable.ContainsKey(key))
            {
                if (_builtInTable.ContainsKey(key))
                {
                    Destroy(_popupTable[key].GO);
                    
                    _builtInTable.Remove(key);
                }
                
                _popupTable.Remove(key);
            }
            else
            {
                Debug.LogErrorFormat("[Popup] Register : Popup is not registed - {0}", key);
            }
        }

        private T _Show<T>(string key) where T : class, IPopupHandler
        {
            IPopupHandler handler = null;
            
            if (_popupTable.ContainsKey(key) == false)
            {
                handler = LoadFromBuiltIn(key);

                if (handler == null)
                {
                    Debug.LogErrorFormat("[Popup] CoShowPopup : Popup is not registed - {0}", key);
                    return null;
                }
            }

            handler = _popupTable[key];

            StartCoroutine(CoShow(handler));

            return handler as T;
        }

        private IEnumerator CoShow(IPopupHandler handler)
        {
            if (handler.IsInTransition)
            {
                Debug.LogErrorFormat("[Popup] CoShowPopup : {0} is in transition.", handler.Name);
                yield break;
            }

            if (_popups.Contains(handler))
            {
                Debug.LogErrorFormat("[Popup] CoShowPopup : View already shown. - {0}", handler.Name);
                yield break;
            }

            var current = _Current;

            if (current == null)
            {
                if (_mainCanvas == null)
                {
                    handler.PopupCanvas.sortingOrder = _baseSortingOrder;
                }
                else
                {
                    InteractableMainCanvas = false;
                    handler.PopupCanvas.sortingOrder = _mainCanvas.sortingOrder + 1;
                }
            }
            else
            {
                current.Interactable = false;
                handler.PopupCanvas.sortingOrder = current.PopupCanvas.sortingOrder + 1;
            }

            _popups.Add(handler);

            yield return handler.Show();

            _onPopupShow.Invoke(handler.Name, handler);
        }

        private T _Hide<T>(string key) where T : class, IPopupHandler
        {
            if (_popups.Count <= 0)
            {
                Debug.LogError("[Popup] Hide : Popup is not show.");
                return null;
            }

            if (_popupTable.ContainsKey(key))
            {
                Debug.LogErrorFormat("[Popup] Hide : Popup is not registed. - {0}", key);
                return null;
            }

            return _Hide<T>(_popupTable[key]);
        }

        private T _Hide<T>(IPopupHandler handler) where T : class, IPopupHandler
        {
            if (handler.IsInTransition)
            {
                Debug.LogErrorFormat("[Popup] Hide : {0} is in transition.", handler.Name);
                return null;
            }

            StartCoroutine(CoHide(handler));

            return handler as T;
        }

        private IEnumerator CoHide(IPopupHandler handler)
        {
            var isTop = handler == _Current;

            _popups.Remove(handler);

            yield return handler.Hide();

            if (isTop)
            {
                if (_popups.Count > 0)
                {
                    _Current.Interactable = true;
                }
                else
                {
                    InteractableMainCanvas = true;
                }
            }

            _onPopupHide.Invoke(handler.Name, handler);
        }
        
        private IEnumerator CoHideAll(params string[] excepts)
        {
            for (int i = _popups.Count - 1; i < _popups.Count; i--)
            {
                var handler = _popups[i];
                
                if (Array.Exists(excepts, item => item == handler.Name)) continue;
                
                yield return CoHide(handler);
            }
        }
        
        private IPopupHandler LoadFromBuiltIn(string key)
        {
            var go = Resources.Load<GameObject>(key);

            if (go == null) return null;
            
            var handler = go.GetComponent<IPopupHandler>();

            if (handler == null) return null;
            
            _builtInTable[key] = go;
                        
            var popup = Instantiate(go, transform).GetComponent<IPopupHandler>();
                        
            _Register(key, popup);

            return popup;
        }

        private T _Get<T>() where T : class, IPopupHandler
        {
            foreach (var handler in _popupTable.Values)
            {
                if (handler is T) return handler as T;
            }

            return null;
        }
    }
}