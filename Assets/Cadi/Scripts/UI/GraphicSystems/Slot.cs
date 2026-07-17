using System;
using System.Collections.Generic;
using Cadi.Scripts.UI.FX;
#if CADI_DOTWEEN
using DG.Tweening;
#endif
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Cadi.Scripts.UI.GraphicSystems
{
    public enum IsNested
    {
        Single,
        Nested,
    }

    public enum SlotLoc
    {
        This,
        Child
    }

    public enum GraphicType
    {
        Raw,
        Image,
    }

    [Serializable]
#if ODIN_INSPECTOR
    [InlineProperty(LabelWidth = 68)]
#endif
    public class SlotConfig
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [PropertyOrder(0),HorizontalGroup("TypeRow", Width = 165),LabelText("Type"),LabelWidth(40),EnumToggleButtons,PropertySpace(0, 4)]
#endif
        protected GraphicType m_Type = GraphicType.Image;

        [SerializeField]
#if ODIN_INSPECTOR
        [PropertyOrder(1),HorizontalGroup("ColorRow", Width = 115),LabelText("Default"),LabelWidth(58),PropertySpace(0, 6)]
#endif
        protected Color m_DefaultColor = Color.white;

        [SerializeField]
#if ODIN_INSPECTOR
        [PropertyOrder(1),HorizontalGroup("ColorRow", Width = 120),LabelText("Selected"),LabelWidth(62),PropertySpace(0, 6)]
#endif
        protected Color m_SelectedColor = Color.white;

        [SerializeField]
#if ODIN_INSPECTOR
        [PropertyOrder(2),ToggleGroup(nameof(m_UseOutline), "Outline"),PropertySpace(4, 2)]
#endif
        protected bool m_UseOutline = false;

        [SerializeField]
#if ODIN_INSPECTOR
        [PropertyOrder(3),ToggleGroup(nameof(m_UseOutline)),HorizontalGroup(nameof(m_UseOutline) + "/Row", Width = 115),LabelText("Color"),LabelWidth(42),PropertySpace(3, 0)]
#endif
        protected Color m_OutlineColor = Color.black;

        [SerializeField, Range(0f, 30f)]
#if ODIN_INSPECTOR
        [PropertyOrder(3),ToggleGroup(nameof(m_UseOutline)),HorizontalGroup(nameof(m_UseOutline) + "/Row", Width = 190),LabelText("Width"),LabelWidth(45),SuffixLabel("px", Overlay = true),PropertySpace(3, 0)]
#endif
        protected float m_OutlineWidth = 2f;
        
        public virtual Color DefaultColor
        {
            get => m_DefaultColor;
            set => m_DefaultColor = value;
        }
        
        public  GraphicType Type
        {
            get => m_Type;
            set => m_Type = value;
        }

        public Color SelectedColor
        {
            get => m_SelectedColor;
            set => m_SelectedColor = value;
        }

        public bool UseOutline
        {
            get => m_UseOutline;
            set => m_UseOutline = value;
        }
        
        public virtual Color OutlineColor
        {
            get => m_OutlineColor;
            set => m_OutlineColor = value;
        }
        
        public virtual float OutlineWidth
        {
            get => m_OutlineWidth;
            set => m_OutlineWidth = value;
        }

    }

    [Serializable]
#if ODIN_INSPECTOR
    [InlineProperty]
#endif
    public class Slot : SlotConfig
    {
        [SerializeField, HideInInspector]
        private Graphic m_Graphic;

        [SerializeField, HideInInspector]
        private UIOutline m_Outline;
#if CADI_DOTWEEN
        [NonSerialized]
        private Tweener m_Tween;
#endif

        // -----------------------------------------------------------
        // Properties
        // -----------------------------------------------------------
        public override Color DefaultColor
        {
            get => m_DefaultColor;
            set
            {
                m_DefaultColor = value;
                ApplyDefault();
            }
        }

     
        public override Color OutlineColor
        {
            get => m_OutlineColor;
            set
            {
                m_OutlineColor = value;
                ApplyOutlineSettings();
            }
        }

        public override float OutlineWidth
        {
            get => m_OutlineWidth;
            set
            {
                m_OutlineWidth = value;
                ApplyOutlineSettings();
            }
        }

        public Graphic Graphic => m_Graphic;

        // -----------------------------------------------------------
        // Binding (called by owning MonoBehaviour)
        // -----------------------------------------------------------

        public void Bind(Graphic graphic, UIOutline outline)
        {
            m_Graphic = graphic;
            m_Outline = outline;
        }

        // -----------------------------------------------------------
        // Color
        // -----------------------------------------------------------

        public void SetColor(Color color)
        {
            KillTween();

            if (m_Graphic != null)
                m_Graphic.color = color;
        }

        public void ApplyDefault()
        {
            SetColor(m_DefaultColor);
            SetOutlineActive(false);
        }

        public void ApplySelected()
        {
            SetColor(m_SelectedColor);
            SetOutlineActive(true);
        }

        public void DoColor(Color color, float duration)
        {
            if (m_Graphic == null)
                return;

#if CADI_DOTWEEN
    m_Tween?.Kill();

    if (duration <= 0f)
    {
        m_Graphic.color = color;
        return;
    }

    m_Tween = m_Graphic.DOColor(color, duration);
#else
            m_Graphic.color = color;
#endif
        }

        // -----------------------------------------------------------
        // Content
        // -----------------------------------------------------------

        public void SetSprite(Sprite sprite)
        {
            if (m_Graphic is Image img)
                img.sprite = sprite;
            else if (m_Graphic is RawImage raw)
                raw.texture = GraphicUtils.TextureFromSprite(sprite);
        }

        public void SetTexture(Texture texture)
        {
            if (m_Graphic is RawImage raw)
                raw.texture = texture;
            else if (m_Graphic is Image img)
                img.sprite = GraphicUtils.GetSpriteFromTexture(texture);
        }

        public void Toggle(bool enable)
        {
            if (m_Graphic)
                m_Graphic.enabled = enable;
        }

        // -----------------------------------------------------------
        // Outline
        // -----------------------------------------------------------

        public void SetOutlineActive(bool active)
        {
            if (m_Outline == null)
                return;

            m_Outline.enabled = m_UseOutline;

            if (!m_UseOutline)
                return;

            m_Outline.SetOutlineColor(active ? m_OutlineColor : m_DefaultColor, active ? m_OutlineWidth : 0f);
        }

        public void CopyFrom(SlotConfig cfg, bool applyDefault = false)
        {
            m_Type = cfg.Type;
            m_DefaultColor = cfg.DefaultColor;
            m_SelectedColor = cfg.SelectedColor;
            m_UseOutline = cfg.UseOutline;
            m_OutlineWidth = cfg.OutlineWidth;
            m_OutlineColor = cfg.OutlineColor;

            if (applyDefault)
                ApplyDefault();
        }

        public void ApplyOutlineSettings()
        {
            if (m_Outline == null || !m_UseOutline)
                return;

            m_Outline.enabled = true;
            m_Outline.SetOutlineColor(m_OutlineColor, m_OutlineWidth);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(m_Outline);
#endif
        }

        // -----------------------------------------------------------
        // Cleanup
        // -----------------------------------------------------------

        public void Dispose()
        {
            KillTween();
        }
        
        private void KillTween()
        {
#if CADI_DOTWEEN
    m_Tween?.Kill();
    m_Tween = null;
#endif
        }
    }
    
    
    public static class GraphicUtils
    {
        private static readonly Dictionary<Texture, Sprite> s_SpriteCache = new Dictionary<Texture, Sprite>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearCache()
        {
            foreach (Sprite sprite in s_SpriteCache.Values)
            {
                if (sprite == null)
                    continue;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEngine.Object.DestroyImmediate(sprite);
                else
                    UnityEngine.Object.Destroy(sprite);
#else
        UnityEngine.Object.Destroy(sprite);
#endif
            }

            s_SpriteCache.Clear();
        }

        public static Sprite GetSpriteFromTexture(Texture texture)
        {
            if (texture == null)
                return null;

            if (texture is not Texture2D texture2D)
            {
                Debug.LogWarning($"Cannot create Sprite from texture type {texture.GetType().Name}. Expected Texture2D.");
                return null;
            }

            if (s_SpriteCache.TryGetValue(texture, out var sprite))
                return sprite;

            var newSprite = Sprite.Create(
                texture2D,
                new Rect(0, 0, texture2D.width, texture2D.height),
                Vector2.one * 0.5f
            );

            s_SpriteCache[texture] = newSprite;
            return newSprite;
        }

        public static Texture TextureFromSprite(Sprite sprite)
        {
            return sprite?.texture;
        }
    }
}
