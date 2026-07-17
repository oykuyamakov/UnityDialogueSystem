using System;
using System.Collections.Generic;
using Cadi.Scripts.CacherSystem;
using Cadi.Scripts.EventSystem;
using Cadi.Scripts.UI.FX;
using UnityEngine;
using UnityEngine.UI;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using Cadi.Scripts.CustomAttributes;
#endif

namespace Cadi.Scripts.UI.GraphicSystems.Selective
{
    public class SelectixGroup : CacherMonoBehaviour
    {       
        [SerializeField, ShowIf(nameof(m_IsNested), IsNested.Nested)]
#if ODIN_INSPECTOR
        [FoldoutGroup("Settings"), OnValueChanged(nameof(OnSettingsChanged))]
#else
       [ShowIf(nameof(m_IsNested), (int)IsNested.Nested)]
#endif
        private float m_Padding = 5;

        [SerializeField]
#if ODIN_INSPECTOR
        [FoldoutGroup("Setup"), OnValueChanged(nameof(OnSettingsChanged)), EnumToggleButtons]
#endif
        private IsNested m_IsNested = IsNested.Single;

        [SerializeField, ShowIf(nameof(m_IsNested), IsNested.Nested)]
#if ODIN_INSPECTOR
        [FoldoutGroup("Setup"), OnValueChanged(nameof(OnSettingsChanged)), EnumToggleButtons]
#else
        [ShowIf(nameof(m_IsNested), (int)IsNested.Nested)]
#endif
        protected SlotLoc m_MainContentSlot = GraphicSystems.SlotLoc.Child;

        [SerializeField, Range(0, 30)]
#if ODIN_INSPECTOR
        [FoldoutGroup("Setup")]
#endif
        private int m_ChildCount;

        [SerializeField]
#if ODIN_INSPECTOR
        [FoldoutGroup("Settings"),BoxGroup("Settings/Root Slot"),InlineProperty(LabelWidth = 68), OnValueChanged(nameof(OnSettingsChanged), true)]
#endif
        private Slot m_RootSettings = new();

        [SerializeField, ShowIf(nameof(m_IsNested), IsNested.Nested)]
#if ODIN_INSPECTOR
        [OnValueChanged(nameof(OnSettingsChanged), true), FoldoutGroup("Settings"), BoxGroup("Settings/Child Slot"), InlineProperty]
#else 
        [ShowIf(nameof(m_IsNested), (int)IsNested.Nested)]
#endif
        private SlotConfig m_ChildSetting = new();

        [SerializeField]
#if ODIN_INSPECTOR
        [FoldoutGroup("Runtime/FX"), OnValueChanged(nameof(OnSettingsChanged))] [InlineProperty(LabelWidth = 68)]
#endif
        private UIFxType m_FxForeground;

        [SerializeField]
#if ODIN_INSPECTOR
        [FoldoutGroup("Runtime/FX"), OnValueChanged(nameof(OnSettingsChanged))] [InlineProperty(LabelWidth = 68)]
#endif
        private UIFxType m_FxBackground;

        [SerializeField]
#if ODIN_INSPECTOR
        [FoldoutGroup("Runtime"), OnValueChanged(nameof(OnSettingsChanged))] [InlineProperty(LabelWidth = 68)]
#endif
        private bool m_AllowOverridenChildren = false;

        [SerializeField]
#if ODIN_INSPECTOR
        [FoldoutGroup("Runtime"), OnValueChanged(nameof(OnSettingsChanged))] [InlineProperty(LabelWidth = 68)]
#endif
        protected bool m_AllowMultipleSelection = false;

        [SerializeField, Min(2), ShowIf(nameof(m_AllowMultipleSelection))]
#if ODIN_INSPECTOR
        [ FoldoutGroup("Runtime")] [InlineProperty(LabelWidth = 68)]
#endif
        private int m_MultiSelectLimit = 2;

        [SerializeField]
#if ODIN_INSPECTOR
        [ FoldoutGroup("Runtime"), OnValueChanged(nameof(OnSettingsChanged))] [InlineProperty(LabelWidth = 68)]
#endif
        private bool m_AllowDeselection = false;


        // -----------------------------------------------------------
        // Public accessors
        // -----------------------------------------------------------

        public bool AllowMultipleSelection => m_AllowMultipleSelection;
        public bool AllowDeselection => m_AllowDeselection;

        public SlotConfig RootSettings => m_RootSettings;
        public SlotConfig ChildSlot => m_ChildSetting;
        public IsNested IsNested => m_IsNested;
        public float ContentPadding => m_Padding;
        public float BgContentPadding => m_Padding;
        public GraphicType GraphicType => m_RootSettings.Type;
        public GraphicType BgGraphicType => m_ChildSetting.Type;
        public bool AllowOverridenChildren => m_AllowOverridenChildren;
        public UIFxType FxForeground => m_FxForeground;
        public UIFxType FxBackground => m_FxBackground;

        // -----------------------------------------------------------
        // Children
        // -----------------------------------------------------------

        [SerializeField, CachedField(RefSearch.Children, includeInactive: true)]
        protected List<Graphix> m_SImages = new();

        [CachedField(addComponentIfMissing: true), SerializeField]
        protected Canvas m_Canvas;

        [CachedField(addComponentIfMissing: true), SerializeField]
        protected GraphicRaycaster m_GraphicRaycaster;

        private readonly HashSet<ISelectix> m_SelectedImages = new();

        public Action<ISelectix, bool> OnImage;
        public int SortingOrder => m_Canvas.sortingOrder;

        // -----------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------

        protected override void OnAwake()
        {
            base.OnAwake();

            EM.AddListener<SGraphixSelectedEvent>(OnSImageSelected, Priority.Critical, this);
            EM.AddListener<SGraphixDeselectedEvent>(OnSImageDeselected, Priority.Critical, this);
            EM.AddListener<SGraphixCreatedEvent>(OnSImageSpawned, Priority.Critical, this);

            InitImages();
        }

        protected virtual void OnDestroy()
        {
            EM.RemoveListener<SGraphixSelectedEvent>(OnSImageSelected, this);
            EM.RemoveListener<SGraphixDeselectedEvent>(OnSImageDeselected, this);
            EM.RemoveListener<SGraphixCreatedEvent>(OnSImageSpawned, this);
        }

        // -----------------------------------------------------------
        // Settings push
        // -----------------------------------------------------------

        private void OnSettingsChanged()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ResolveReferences();
            }
#endif

            AdjustImages();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
#if UNITY_EDITOR
        private void EnsureCorrectChildTypes()
        {
            Type targetType = m_IsNested == IsNested.Nested
                ? typeof(NestedSelectix)
                : typeof(Selectix);

            for (int i = 0; i < m_SImages.Count; i++)
            {
                var image = m_SImages[i];

                if (image == null)
                    continue;

                if (image.GetType() == targetType)
                    continue;

                ReplaceImageObject(i, targetType);
            }
        }

        private void ReplaceImageObject(int index, Type targetType)
        {
            var oldImage = m_SImages[index];

            if (oldImage == null)
                return;

            var oldGo = oldImage.gameObject;
            var oldRect = oldGo.GetComponent<RectTransform>();

            string objectName = oldGo.name;
            int siblingIndex = oldGo.transform.GetSiblingIndex();
            bool activeSelf = oldGo.activeSelf;

            Vector2 anchorMin = oldRect != null ? oldRect.anchorMin : Vector2.zero;
            Vector2 anchorMax = oldRect != null ? oldRect.anchorMax : Vector2.one;
            Vector2 offsetMin = oldRect != null ? oldRect.offsetMin : Vector2.zero;
            Vector2 offsetMax = oldRect != null ? oldRect.offsetMax : Vector2.zero;
            Vector2 pivot = oldRect != null ? oldRect.pivot : new Vector2(0.5f, 0.5f);
            Vector3 localScale = oldGo.transform.localScale;

            UnityEditor.Undo.DestroyObjectImmediate(oldGo);

            var newGo = new GameObject(objectName, typeof(RectTransform), targetType);
            UnityEditor.Undo.RegisterCreatedObjectUndo(newGo, "Replace Selective Graphix");

            newGo.transform.SetParent(transform, false);
            newGo.transform.SetSiblingIndex(siblingIndex);
            newGo.transform.localScale = localScale;
            newGo.SetActive(activeSelf);

            var newRect = newGo.GetComponent<RectTransform>();
            newRect.anchorMin = anchorMin;
            newRect.anchorMax = anchorMax;
            newRect.offsetMin = offsetMin;
            newRect.offsetMax = offsetMax;
            newRect.pivot = pivot;

            m_SImages[index] = newGo.GetComponent<Graphix>();

            UnityEditor.EditorUtility.SetDirty(newGo);
        }
#endif
        
        [Button]
#if ODIN_INSPECTOR
        [ FoldoutGroup("Setup")]
#endif
        public void Refresh()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            m_ChildCount = Mathf.Max(0, m_ChildCount);

            ResolveReferences();

            for (int i = m_SImages.Count - 1; i >= m_ChildCount; i--)
            {
                var image = m_SImages[i];

                if (image != null)
                    UnityEditor.Undo.DestroyObjectImmediate(image.gameObject);
            }

            ResolveReferences();

            EnsureCorrectChildTypes();

            ResolveReferences();
            
            for (int i = m_SImages.Count; i < m_ChildCount; i++)
            {
                Type childType = m_IsNested == IsNested.Nested
                    ? typeof(NestedSelectix)
                    : typeof(Selectix);

                var go = new GameObject($"SImage_{i}", typeof(RectTransform), childType);
                UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Selective Graphix");

                go.transform.SetParent(transform, false);

                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                //m_SImages.Add(go.GetComponent<Graphix>());
            }

            ResolveReferences();
            AdjustImages();
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void AdjustImages()
        {
            for (int i = 0; i < m_SImages.Count; i++)
            {
                var image = m_SImages[i];
                if (image == null)
                    continue;

                var selective = image as ISelectix;
                if (selective == null)
                    continue;

#if UNITY_EDITOR
                image.name = $"SImage_{i}";
                image.transform.SetSiblingIndex(i);
#endif

                selective.EditorBind(this);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(image);
#endif
            }
        }

        // -----------------------------------------------------------
        // Content helpers
        // -----------------------------------------------------------

        public void SetContent(List<Sprite> content, bool preserveAspect, Sprite back = null)
        {
            for (int i = 0; i < m_SImages.Count; i++)
            {
                if (i >= content.Count)
                {
                    m_SImages[i].gameObject.SetActive(false);
                    continue;
                }

                var image = m_SImages[i];
                image.gameObject.SetActive(true);

                if (image is NestedGraphix or NestedSelectix)
                {
                    var nested = image as NestedGraphix;
                    nested.SetContent(
                        content[i],
                        preserveAspect,
                        false,
                        m_MainContentSlot
                    );

                    if (back != null)
                    {
                        nested.SetContent(
                            back,
                            preserveAspect,
                            false,
                            m_MainContentSlot == SlotLoc.Child ? SlotLoc.This : SlotLoc.Child
                        );
                    }
                }
                else
                {
                    image.SetContent(content[i], preserveAspect, false);
                }
            }
        }
   
        public List<Graphix> GetImagesRandomOrder()
        {
            var shuffled = new List<Graphix>(m_SImages);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            return shuffled;
        }

        private void InitImages()
        {
            for (int i = 0; i < m_SImages.Count; i++)
            {
                var selective = m_SImages[i] as ISelectix;
                selective?.SetGroup(i, this);
            }
        }

        public void DeselectAll()
        {
            foreach (var image in m_SImages)
            {
                if (image is ISelectix selective)
                    selective.TryDeselect();
            }
        }

        public void UnlockAll()
        {
            foreach (var image in m_SImages)
            {
                if (image is ISelectix selective)
                    selective.UnLock();
            }
        }

        public void LockAll(bool disableVis)
        {
            foreach (var image in m_SImages)
            {
                if (image is ISelectix selective)
                    selective.Lock(disableVis);
            }
        }

        // -----------------------------------------------------------
        // Event handlers
        // -----------------------------------------------------------

        protected virtual void OnSImageSpawned(SGraphixCreatedEvent evt)
        {
        }

        protected virtual void OnSImageSelected(SGraphixSelectedEvent evt)
        {
            var sImage = evt.Selectix;

            if (sImage == null)
            {
                Debug.LogError("SImage is null on selection event");
                return;
            }
            
            if (m_SelectedImages.Contains(sImage))
            {
                Debug.LogWarning($"Image {sImage.RuntimeID} is already selected in group {this.name}");
                return;
            }

            if (m_AllowMultipleSelection && m_SelectedImages.Count >= m_MultiSelectLimit)
            {
                sImage.TryDeselect();
                return;
            }

            m_SelectedImages.Add(sImage);
            OnImage?.Invoke(sImage, true);
        }

        protected virtual void OnSImageDeselected(SGraphixDeselectedEvent evt)
        {
            var sImage = evt.Selectix;
            
            if (sImage == null)
            {
                Debug.LogError("SImage is null on selection event");
                return;
            }
            
            if (!m_SelectedImages.Remove(sImage))
                return;

            OnImage?.Invoke(sImage, false);
        }
    }
}