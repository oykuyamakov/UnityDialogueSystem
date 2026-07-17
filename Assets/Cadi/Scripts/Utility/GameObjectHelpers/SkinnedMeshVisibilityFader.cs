using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cadi.Scripts.Utility.GameObjectHelpers
{
    public static class SkinnedMeshVisibilityFader
    {
        private static readonly int s_UnlitColorId = Shader.PropertyToID("_UnlitColor"); // HDRP/Unlit
        private static readonly int s_BaseColorId = Shader.PropertyToID("_BaseColor"); // HDRP/Lit (fallback)
        private static readonly int s_SurfaceTypeId = Shader.PropertyToID("_SurfaceType"); // 0 Opaque, 1 Transparent

        struct MatState
        {
            public int RenderQueue;
            public int SurfaceType;
            public int Zwrite, SrcBlend, DstBlend;
            public string RenderTypeTag;
        }

        public static Coroutine Fade(MonoBehaviour runner, IReadOnlyList<SkinnedMeshRenderer> renderers, float target01,
            float duration)
        {
            return runner.StartCoroutine(FadeRoutine(renderers, Mathf.Clamp01(target01), Mathf.Max(0.0001f, duration)));
        }

        static IEnumerator FadeRoutine(IReadOnlyList<SkinnedMeshRenderer> renderers, float target, float duration)
        {
            if (renderers == null) yield break;

            // Cache original material states (per material instance) + ensure renderers enabled for fade-in/out
            var cached = new Dictionary<Material, MatState>(64);

            bool anyStartFound = false;
            float startAlpha = 1f;

            foreach (var r in renderers)
            {
                if (r == null) continue;


                var mats = r.materials; // instances
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    if (!r.gameObject.activeSelf) continue;

                    r.enabled =
                        true; // we fade visually; if keep disabled wanted when target==0, we'll disable at end


                    if (!cached.ContainsKey(m))
                    {
                        cached[m] = CaptureState(m);
                    }

                    EnsureTransparent(m);

                    if (!anyStartFound)
                    {
                        if (TryGetAlpha(m, out float a))
                        {
                            startAlpha = a;
                            anyStartFound = true;
                        }
                    }
                }
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float v = Mathf.Lerp(startAlpha, target, Mathf.SmoothStep(0f, 1f, t));
                Debug.Log(v);

                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    if (!r.gameObject.activeSelf) continue;

                    var mats = r.materials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        var m = mats[i];
                        if (m == null) continue;
                        SetAlpha(m, v);
                    }
                }

                yield return null;
            }

            // End state: if invisible -> disable renderer; then restore opaque/original states
            if (target <= 0.0001f)
            {
                foreach (var r in renderers)
                {
                    if (r != null) r.enabled = false;
                }

                // Reset alpha back to 1 so next show starts clean (optional but usually desired)
                foreach (var kv in cached)
                {
                    SetAlpha(kv.Key, 1f);
                    RestoreState(kv.Key, kv.Value);
                }

                yield break;
            }

            // Visible: set alpha=1, restore opaque/original states
            foreach (var kv in cached)
            {
                SetAlpha(kv.Key, 1f);
                RestoreState(kv.Key, kv.Value);
            }
        }

        static MatState CaptureState(Material m)
        {
            return new MatState
            {
                RenderQueue = m.renderQueue,
                SurfaceType = m.HasProperty(s_SurfaceTypeId) ? Mathf.RoundToInt(m.GetFloat(s_SurfaceTypeId)) : 0,
                Zwrite = m.HasProperty("_ZWrite") ? m.GetInt("_ZWrite") : 1,
                SrcBlend = m.HasProperty("_SrcBlend") ? m.GetInt("_SrcBlend") : (int)BlendMode.One,
                DstBlend = m.HasProperty("_DstBlend") ? m.GetInt("_DstBlend") : (int)BlendMode.Zero,
                RenderTypeTag = m.GetTag("RenderType", false, "")
            };
        }

        static void RestoreState(Material m, MatState s)
        {
            if (m == null) return;

            if (m.HasProperty(s_SurfaceTypeId)) m.SetFloat(s_SurfaceTypeId, s.SurfaceType);
            m.renderQueue = s.RenderQueue;

            if (!string.IsNullOrEmpty(s.RenderTypeTag))
            {
                m.SetOverrideTag("RenderType", s.RenderTypeTag);
            }
            else
            {
                m.SetOverrideTag("RenderType", "");
            }

            if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", s.Zwrite);
            if (m.HasProperty("_SrcBlend")) m.SetInt("_SrcBlend", s.SrcBlend);
            if (m.HasProperty("_DstBlend")) m.SetInt("_DstBlend", s.DstBlend);
        }

        static void EnsureTransparent(Material m)
        {
            if (m == null) return;

            if (m.HasProperty(s_SurfaceTypeId)) m.SetFloat(s_SurfaceTypeId, 1f);
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetOverrideTag("RenderType", "Transparent");

            if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", 0);
            if (m.HasProperty("_SrcBlend")) m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        }

        static bool TryGetAlpha(Material m, out float alpha)
        {
            alpha = 1f;
            if (m == null) return false;

            if (m.HasProperty(s_UnlitColorId))
            {
                alpha = m.GetColor(s_UnlitColorId).a;
                return true;
            }

            if (m.HasProperty(s_BaseColorId))
            {
                alpha = m.GetColor(s_BaseColorId).a;
                return true;
            }

            return false;
        }

        static void SetAlpha(Material m, float a)
        {
            if (m == null) return;
            a = Mathf.Clamp01(a);

            if (m.HasProperty(s_UnlitColorId))
            {
                var c = m.GetColor(s_UnlitColorId);
                c.a = a;
                m.SetColor(s_UnlitColorId, c);
                return;
            }

            if (m.HasProperty(s_BaseColorId))
            {
                var c = m.GetColor(s_BaseColorId);
                c.a = a;
                m.SetColor(s_BaseColorId, c);
            }
        }
    }
}