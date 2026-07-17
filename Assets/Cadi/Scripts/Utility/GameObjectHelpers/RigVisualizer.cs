using System;
using System.Collections.Generic;
using Cadi.Scripts.CustomAttributes;
using Cadi.Scripts.Utility.DebugHelpers;
using UnityEditor;
using UnityEngine;

namespace Cadi.Scripts.Utility.GameObjectHelpers
{
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class RigVisualizer : MonoBehaviour
    {
        public enum BoneMode
        {
            None,
            Default,
            Fingers,
            Full,
            Toes
        }

        [Serializable]
        private class BoneBaseline
        {
            public HumanBodyBones Bone;

            [Tooltip("Serialized for reference/debug. Not required for deviation checks.")]
            public HumanBodyBones ParentBone = HumanBodyBones.LastBone; // LastBone => unknown/not mapped

            public Vector3 LocalPosition;
            public Vector3 LocalScale;
        }

        [Header("Target")]
        [SerializeField]
        private Animator m_Animator;

        [Header("Bone Inclusion")]
        [SerializeField]
        private BoneMode m_BoneMode = BoneMode.Default;

        [Header("DebugSphere - Global")]
        [Min(0f)]
        [SerializeField]
        private float m_SphereRadius = 0.05f;

        [SerializeField]
        private Color m_SphereColor = Color.red;

        [Tooltip(
            "If true, DebugSphere draws only when the object is selected (depends on DebugSphere implementation).")]
        [SerializeField]
        private bool m_SphereOnlyWhenSelected = false;

#if UNITY_EDITOR

        [Header("DebugText - Global")]
        [SerializeField]
        private DebugText.TextMode m_TextMode = DebugText.TextMode.None;
#endif

        [SerializeField]
        private Color m_TextColor = Color.white;

        [Min(6)]
        [SerializeField]
        private int m_TextFontSize = 12;

        [SerializeField]
        private Vector3 m_TextWorldOffset = new Vector3(0f, 0.03f, 0f);

        [Min(0)]
        [SerializeField]
        private int m_TextDecimals = 2;

        [SerializeField]
        private bool m_TextOnlyWhenSelected = false;

        [Tooltip(
            "When printing ObjectName/OverrideName, remove this substring from the output (example: 'mixamorig:').")]
        [SerializeField]
        private string m_RemoveFromNames = "mixamorig:";

        [Header("Advanced Bone Relation Debugger")]
        [Tooltip(
            "When enabled, compares current local position/scale to the captured baseline and color-codes spheres.")]
        [SerializeField]
        private bool m_AdvancedBoneRelationDebugger = false;

        [Header("Position Thresholds")]
        [Tooltip("If |localPosition - baseline| >= Warning => sphere turns Orange.")]
        [Min(0f)]
        [SerializeField]
        private float m_PosWarning = 0.02f;

        [Tooltip("If |localPosition - baseline| >= Danger => sphere turns Purple.")]
        [Min(0f)]
        [SerializeField]
        private float m_PosDanger = 0.05f;

        [Header("Scale Threshold")]
        [Tooltip(
            "If scaleRelativeDelta >= threshold => sphere turns Red. (Relative = abs(cur-base)/abs(base), worst axis).")]
        [Min(0f)]
        [SerializeField]
        private float m_ScaleThreshold = 0.15f;

        [Header("Deviation Colors")]
        [SerializeField]
        private Color m_OkColor = Color.green;

        [SerializeField]
        private Color m_PosWarnColor = Color.yellow;

        [SerializeField]
        private Color m_PosDangerColor = Color.red;

        [SerializeField]
        private Color m_ScaleDangerWithPosWarnColor = new Color(1.0f, 0.55f, 0.0f, 1.0f); // Orange

        [SerializeField]
        private Color m_ScaleDangerWithPosDangerColor = new Color(0.6f, 0.0f, 0.8f, 1.0f); // Purple


        [Header("Behavior")]
        [Tooltip("If true, missing DebugSphere/DebugText components are added automatically.")]
        [SerializeField]
        private bool m_AutoInstallMissing = true;

        [Tooltip("If true, searches inactive children for Animator when m_Animator is null.")]
        [SerializeField]
        private bool m_IncludeInactive = true;

        [Header("Baseline (Captured in Editor)")]
        [SerializeField]
        private List<BoneBaseline> m_Baseline = new();

        // Fingers/toes/face spheres are 70% smaller => 0.30x
        private const float SmallBoneRadiusMultiplier = 0.30f;

        private readonly Dictionary<HumanBodyBones, BoneBaseline> m_BaselineMap = new();

        private static readonly HumanBodyBones[] s_DefaultBones =
        {
            HumanBodyBones.Hips,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Neck,
            HumanBodyBones.Head,

            HumanBodyBones.LeftShoulder,
            HumanBodyBones.RightShoulder,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand,

            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot,
        };

        private static readonly HumanBodyBones[] s_ToesBones =
        {
            HumanBodyBones.LeftToes,
            HumanBodyBones.RightToes,
        };

        private static readonly HumanBodyBones[] s_FaceBones =
        {
            HumanBodyBones.LeftEye,
            HumanBodyBones.RightEye,
            HumanBodyBones.Jaw,
        };

        private static readonly HumanBodyBones[] s_FingerBones =
        {
            HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,
            HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,
            HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,
            HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,
            HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal,

            HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal,
            HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,
            HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate,
            HumanBodyBones.RightMiddleDistal,
            HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,
            HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate,
            HumanBodyBones.RightLittleDistal,
        };

        private void OnEnable()
        {
            RebuildBaselineMap();
            Refresh();
        }

        private void OnValidate()
        {
            RebuildBaselineMap();
            Refresh();
        }

        private void Update()
        {
            if (!m_AdvancedBoneRelationDebugger)
            {
                return;
            }

            if (!IsValidHumanoid(m_Animator))
            {
                ResolveAnimator();
                if (!IsValidHumanoid(m_Animator))
                {
                    return;
                }
            }

            if (m_Baseline == null || m_Baseline.Count == 0)
            {
                return;
            }

            ApplyDeviationVisualsOnly(m_Animator);
        }

#if UNITY_EDITOR
        [Button("Capture T-Pose Baseline")]
#endif
        private void CaptureTPoseBaseline()
        {
            ResolveAnimator();
            if (!IsValidHumanoid(m_Animator))
            {
                return;
            }

            m_Baseline.Clear();

            Dictionary<Transform, HumanBodyBones> transformToBone = BuildTransformToBoneMap(m_Animator);

            foreach (HumanBodyBones bone in EnumerateFullSet())
            {
                Transform t = m_Animator.GetBoneTransform(bone);
                if (t == null)
                {
                    continue;
                }

                BoneBaseline entry = new BoneBaseline
                {
                    Bone = bone,
                    LocalPosition = t.localPosition,
                    LocalScale = t.localScale
                };

                HumanBodyBones parentId = HumanBodyBones.LastBone;
                Transform p = t.parent;
                if (p != null && transformToBone.TryGetValue(p, out HumanBodyBones mappedParent))
                {
                    parentId = mappedParent;
                }

                entry.ParentBone = parentId;
                m_Baseline.Add(entry);
            }

            RebuildBaselineMap();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        [Button("Refresh (Install + Apply)")]
#endif
        private void Refresh()
        {
            ResolveAnimator();

            if (!IsValidHumanoid(m_Animator))
            {
                return;
            }

            if (m_AutoInstallMissing)
            {
                InstallMissingComponents(m_Animator);
            }

            ApplyAll(m_Animator);
        }

#if UNITY_EDITOR
        [Button("Install Missing Components")]
#endif
        private void InstallMissingOnly()
        {
            ResolveAnimator();
            if (!IsValidHumanoid(m_Animator))
            {
                return;
            }

            InstallMissingComponents(m_Animator);
            ApplyAll(m_Animator);
        }

#if UNITY_EDITOR
        [Button("Apply Globals + Modes")]
#endif
        private void ApplyOnly()
        {
            ResolveAnimator();
            if (!IsValidHumanoid(m_Animator))
            {
                return;
            }

            ApplyAll(m_Animator);
        }

        private void ResolveAnimator()
        {
            if (m_Animator != null)
            {
                return;
            }

            m_Animator = GetComponent<Animator>();
            if (m_Animator == null)
            {
                m_Animator = GetComponentInChildren<Animator>(m_IncludeInactive);
            }
        }

        private static bool IsValidHumanoid(Animator animator)
        {
            if (animator == null)
            {
                return false;
            }

            if (animator.avatar == null)
            {
                return false;
            }

            if (!animator.isHuman)
            {
                return false;
            }

            return true;
        }

        private void InstallMissingComponents(Animator animator)
        {
            foreach (HumanBodyBones bone in EnumerateFullSet())
            {
                Transform t = animator.GetBoneTransform(bone);
                if (t == null)
                {
                    continue;
                }
#if UNITY_EDITOR

                DebugSphere sphere = t.GetComponent<DebugSphere>();
                if (sphere == null)
                {
                    Undo.AddComponent<DebugSphere>(t.gameObject);
                    EditorUtility.SetDirty(t.gameObject);
                }
#endif


#if UNITY_EDITOR
                DebugText dt = t.GetComponent<DebugText>();
                if (dt == null)
                {
                    Undo.AddComponent<DebugText>(t.gameObject);
                    EditorUtility.SetDirty(t.gameObject);
                }
#endif
            }
        }

        public void ApplyAll(Animator animator)
        {
            HashSet<HumanBodyBones> enabledBones = BuildEnabledSet(m_BoneMode);

            foreach (HumanBodyBones bone in EnumerateFullSet())
            {
                Transform t = animator.GetBoneTransform(bone);
                if (t == null)
                {
                    continue;
                }

                bool boneEnabled = enabledBones.Contains(bone);

#if UNITY_EDITOR
                ApplySphere(t, bone, boneEnabled);
                ApplyText(t, bone, boneEnabled);

                EditorUtility.SetDirty(t.gameObject);
#endif
            }
        }

        private void ApplyDeviationVisualsOnly(Animator animator)
        {
            HashSet<HumanBodyBones> enabledBones = BuildEnabledSet(m_BoneMode);

            foreach (HumanBodyBones bone in EnumerateFullSet())
            {
                if (!enabledBones.Contains(bone))
                {
                    continue;
                }

                Transform t = animator.GetBoneTransform(bone);
                if (t == null)
                {
                    continue;
                }
#if UNITY_EDITOR

                DebugSphere sphere = t.GetComponent<DebugSphere>();
                DebugText dt = t.GetComponent<DebugText>();

                if (sphere == null && dt == null)
                {
                    continue;
                }

                DeviationFlags flags = GetDeviationFlags(bone, t);
                bool hasDeviation = flags.hasPosDeviation || flags.hasScaleDeviation;

                if (sphere != null)
                {
                    sphere.Color = PickDeviationColor(flags);
                    EditorUtility.SetDirty(sphere);
                }
#endif

#if UNITY_EDITOR
                if (dt != null && hasDeviation)
                {
                    // Override text: bone name + what exceeded thresholds
                    dt.Mode = DebugText.TextMode.OverrideName;
                    dt.OverrideName = BuildDeviationLabel(bone, flags);
                    EditorUtility.SetDirty(dt);
                }
                else if (dt != null && !hasDeviation)
                {
                    // Restore global mode when bone is OK
                    dt.Mode = m_TextMode;
                    dt.OverrideName = "";
                    EditorUtility.SetDirty(dt);
                }
#endif
            }
        }

#if UNITY_EDITOR

        private void ApplySphere(Transform t, HumanBodyBones bone, bool boneEnabled)
        {
            DebugSphere sphere = t.GetComponent<DebugSphere>();
            if (sphere == null)
            {
                return;
            }

            sphere.enabled = boneEnabled;
            sphere.OnlyWhenSelected = m_SphereOnlyWhenSelected;

            if (!boneEnabled)
            {
                sphere.Radius = 0f;
                sphere.Color = m_SphereColor;
                EditorUtility.SetDirty(sphere);
                return;
            }

            float r = m_SphereRadius;
            if (IsSmallBone(bone))
            {
                r *= SmallBoneRadiusMultiplier;
            }

            sphere.Radius = r;
            sphere.Color = m_SphereColor;

            EditorUtility.SetDirty(sphere);
        }


        private void ApplyText(Transform t, HumanBodyBones bone, bool boneEnabled)
        {
            DebugText dt = t.GetComponent<DebugText>();
            if (dt == null)
            {
                return;
            }

            // FIX: Text should be enabled based on BoneMode inclusion (finger/toes subsets must be respected)
            // and also require a non-None text mode.

            bool shouldShowText = boneEnabled && (m_TextMode != DebugText.TextMode.None);

            dt.BoneEnabled = boneEnabled;
            dt.Mode = m_TextMode;
            dt.Color = m_TextColor;
            dt.FontSize = m_TextFontSize;
            dt.WorldOffset = m_TextWorldOffset;
            dt.Decimals = m_TextDecimals;
            dt.OnlyWhenSelected = m_TextOnlyWhenSelected;

            dt.RemoveSubstring = m_RemoveFromNames;

            dt.enabled = shouldShowText;

            EditorUtility.SetDirty(dt);

            dt.enabled = shouldShowText;
        }
#endif

        public struct DeviationFlags
        {
            public bool hasPosDeviation;
            public bool hasPosWarning;
            public bool hasPosDanger;

            public bool hasScaleDeviation;
            public float posMagnitude;
            public Vector3 warningAxes;
            public float scaleRelativeDelta;
        }

        public DeviationFlags GetDeviationFlags(HumanBodyBones bone, Transform t)
        {
            DeviationFlags flags = new DeviationFlags();

            if (m_Baseline == null || m_Baseline.Count == 0)
            {
                return flags;
            }

            if (!m_BaselineMap.TryGetValue(bone, out BoneBaseline baseline))
            {
                return flags;
            }

            float posMag = (t.localPosition - baseline.LocalPosition).magnitude;
            float scaleDelta = ComputeScaleRelativeDelta(t.localScale, baseline.LocalScale);

            flags.warningAxes = (t.localPosition - baseline.LocalPosition);
            flags.posMagnitude = posMag;
            flags.scaleRelativeDelta = scaleDelta;

            flags.hasPosDanger = posMag >= m_PosDanger;
            flags.hasPosWarning = (!flags.hasPosDanger) && (posMag >= m_PosWarning);
            flags.hasPosDeviation = flags.hasPosWarning || flags.hasPosDanger;

            flags.hasScaleDeviation = scaleDelta >= m_ScaleThreshold;

            return flags;
        }

        private Color PickDeviationColor(DeviationFlags flags)
        {
            bool posWarn = flags.hasPosWarning;
            bool posDanger = flags.hasPosDanger;
            bool scaleDanger = flags.hasScaleDeviation;

            // pos danger + scale danger => purple
            if (posDanger && scaleDanger)
            {
                return m_ScaleDangerWithPosDangerColor;
            }

            // pos warn + scale danger => orange
            if (posWarn && scaleDanger)
            {
                return m_ScaleDangerWithPosWarnColor;
            }

            // pos ok + scale danger => orange (scale danger should still stand out)
            if (!posWarn && !posDanger && scaleDanger)
            {
                return m_ScaleDangerWithPosWarnColor;
            }

            // pos danger only => red
            if (posDanger)
            {
                return m_PosDangerColor;
            }

            // pos warn only => yellow
            if (posWarn)
            {
                return m_PosWarnColor;
            }

            // both ok => green
            return m_OkColor;
        }


        private string BuildDeviationLabel(HumanBodyBones bone, DeviationFlags flags)
        {
            // Requested format:
            // boneName + (pos deviation => "pw:" or "pd:" + value) and/or (scale deviation => "sw:" + value)
            // - "pw:" for pos warning
            // - "sw:" for scale

            string name = bone.ToString();

            int d = Mathf.Clamp(m_TextDecimals, 0, 6);

            List<string> parts = new List<string>(2);

            if (flags.hasPosWarning)
            {
                parts.Add($"pw:{flags.posMagnitude.ToString($"F{d}")}");
            }

            if (flags.hasPosDanger)
            {
                parts.Add($"pd:{flags.posMagnitude.ToString($"F{d}")}");
            }

            if (flags.hasScaleDeviation)
            {
                parts.Add($"sw:{flags.scaleRelativeDelta.ToString($"F{d}")}");
            }

            if (parts.Count == 0)
            {
                return name;
            }

            return $"{name} ({string.Join(", ", parts)})";
        }

        private static float ComputeScaleRelativeDelta(Vector3 current, Vector3 baseline)
        {
            float dx = Relative(current.x, baseline.x);
            float dy = Relative(current.y, baseline.y);
            float dz = Relative(current.z, baseline.z);

            return Mathf.Max(dx, Mathf.Max(dy, dz));
        }

        private static float Relative(float cur, float baseVal)
        {
            float denom = Mathf.Abs(baseVal);
            if (denom < 1e-6f)
            {
                return Mathf.Abs(cur - baseVal);
            }

            return Mathf.Abs(cur - baseVal) / denom;
        }

        private void RebuildBaselineMap()
        {
            m_BaselineMap.Clear();

            if (m_Baseline == null)
            {
                return;
            }

            for (int i = 0; i < m_Baseline.Count; i++)
            {
                BoneBaseline b = m_Baseline[i];
                m_BaselineMap[b.Bone] = b;
            }
        }

        private static Dictionary<Transform, HumanBodyBones> BuildTransformToBoneMap(Animator animator)
        {
            Dictionary<Transform, HumanBodyBones> map = new();

            foreach (HumanBodyBones bone in EnumerateFullSetStatic())
            {
                Transform t = animator.GetBoneTransform(bone);
                if (t == null)
                {
                    continue;
                }

                map[t] = bone;
            }

            return map;
        }

        private static bool IsSmallBone(HumanBodyBones bone)
        {
            if (IsInArray(bone, s_FingerBones))
            {
                return true;
            }

            if (IsInArray(bone, s_ToesBones))
            {
                return true;
            }

            if (IsInArray(bone, s_FaceBones))
            {
                return true;
            }

            return false;
        }

        private static bool IsInArray(HumanBodyBones bone, HumanBodyBones[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == bone)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<HumanBodyBones> EnumerateFullSet()
        {
            HashSet<HumanBodyBones> set = new();

            AddRange(set, s_DefaultBones);
            AddRange(set, s_FingerBones);
            AddRange(set, s_ToesBones);
            AddRange(set, s_FaceBones);

            foreach (HumanBodyBones b in set)
            {
                yield return b;
            }
        }

        private static IEnumerable<HumanBodyBones> EnumerateFullSetStatic()
        {
            HashSet<HumanBodyBones> set = new();

            foreach (var t in s_DefaultBones)
                set.Add(t);

            foreach (var t in s_FingerBones)
                set.Add(t);

            foreach (var t in s_ToesBones)
                set.Add(t);

            foreach (var t in s_FaceBones)
                set.Add(t);

            foreach (HumanBodyBones b in set)
            {
                yield return b;
            }
        }

        private static HashSet<HumanBodyBones> BuildEnabledSet(BoneMode mode)
        {
            HashSet<HumanBodyBones> set = new();

            if (mode == BoneMode.None)
            {
                return set;
            }

            if (mode == BoneMode.Default)
            {
                AddRange(set, s_DefaultBones);
                return set;
            }

            if (mode == BoneMode.Fingers)
            {
                AddRange(set, s_DefaultBones);
                AddRange(set, s_FingerBones);
                return set;
            }

            if (mode == BoneMode.Toes)
            {
                AddRange(set, s_DefaultBones);
                AddRange(set, s_ToesBones);
                return set;
            }

            AddRange(set, s_DefaultBones);
            AddRange(set, s_FingerBones);
            AddRange(set, s_ToesBones);
            AddRange(set, s_FaceBones);
            return set;
        }

        private static void AddRange(HashSet<HumanBodyBones> set, HumanBodyBones[] bones)
        {
            foreach (var t in bones)
            {
                set.Add(t);
            }
        }
    }
}