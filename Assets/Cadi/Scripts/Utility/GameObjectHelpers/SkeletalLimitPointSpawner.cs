using System;
using System.Collections.Generic;
using Cadi.Scripts.CustomAttributes;
using Cadi.Scripts.Utility.DebugHelpers;
using UnityEngine;

namespace Cadi.Scripts.Utility.GameObjectHelpers
{
    public class SkeletalLimitPointSpawner : MonoBehaviour
    {
        [SerializeField]
        private List<JointAnchors> m_JointAnchors;

        [SerializeField]
        private GameObject m_PointPrefab;

        [Serializable]
        public enum AnchorType
        {
            ArmWidth,
            ArmLength,
            TorsoWidth,
            TorsoLength,
            ShoulderHeight,
            ShoulderWidth
        }

        // 1) Update the enum (add Forward/Back)
        [System.Flags]
        [Serializable]
        public enum AnchorDir
        {
            None = 0,
            Up = 1 << 0,
            Down = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3,
            Forward = 1 << 4,
            Back = 1 << 5,
        }

        [Serializable]
        public class JointAnchors
        {
            public List<Transform> Anchors;
            public AnchorType AnchorType;

            [Range(0, 5)]
            public int StepCount = 2;

            [Range(0, 2f)]
            public float StepDist = 0.5f;

            [DynamicRange(0, 3f)]
            public float DistanceFromRootUp = 0f;

            [DynamicRange(0, 3f)]
            public float DistanceFromRootDown = 0f;

            [SerializeField]
            public AnchorDir Direction;

            [HideInInspector]
            public List<Transform> SpawnedPoints = new List<Transform>();
        }

        public Color GetColorDebug(AnchorType type)
        {
            return type switch
            {
                AnchorType.ArmLength => new Color(1f, 0.66f, 0.02f),
                AnchorType.TorsoLength => new Color(1f, 0.1f, 0.23f),
                AnchorType.ArmWidth => Color.blue,
                AnchorType.TorsoWidth => new Color(0.43f, 0.73f, 1f),
                AnchorType.ShoulderHeight => new Color(0.37f, 0.91f, 1f),
                AnchorType.ShoulderWidth => new Color(1f, 0.33f, 0.56f),
                _ => Color.white
            };
        }

        public Transform[] GetSpawnedPoints(AnchorType type)
        {
            var list = new List<Transform>();
            foreach (var anchor in m_JointAnchors)
            {
                if (anchor.AnchorType == type)
                {
                    list.AddRange(anchor.SpawnedPoints);
                }
            }

            return list.ToArray();
        }


        [SerializeField]
        private Animator m_Animator;

        [Button]
        public void SetDefault()
        {
            m_Animator = GetComponent<Animator>();
            if (m_Animator == null)
            {
                Debug.LogError("SkeletalLimitPointSpawner: No Animator found on the GameObject.");
                return;
            }
            
            
            
            var forearms = new List<Transform>
            {
                m_Animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
            };

            var armWidthForwardBack = new JointAnchors
            {
                Anchors = forearms,
                AnchorType = AnchorType.ArmWidth,
                StepCount = 5,
                StepDist = 0.131f,
                DistanceFromRootUp = 1.096f,
                DistanceFromRootDown = 0.893f,
                Direction = AnchorDir.Forward | AnchorDir.Back
            };

            // Element 1 — Arm Width (Up/Down)
            var armWidthUpDown = new JointAnchors
            {
                Anchors = forearms,
                AnchorType = AnchorType.ArmWidth,
                StepCount = 5,
                StepDist = 0.131f,
                DistanceFromRootUp = 0.874f,
                DistanceFromRootDown = 1.182f,
                Direction = AnchorDir.Up | AnchorDir.Down
            };

            // Element 2 — Shoulder Height (Up)
            var arms = new List<Transform>
            {
                m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                m_Animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
            };

            var shoulderHeightUp = new JointAnchors
            {
                Anchors = arms,
                AnchorType = AnchorType.ShoulderHeight,
                StepCount = 4,
                StepDist = 0.142f,
                DistanceFromRootUp = 1.028f,
                DistanceFromRootDown = 1.294f,
                Direction = AnchorDir.Up
            };

            // Element 3 — Torso Length (Left)
            var spine = new List<Transform>
            {
                m_Animator.GetBoneTransform(HumanBodyBones.Spine),
            };

            var torsoLengthLeft = new JointAnchors
            {
                Anchors = spine,
                AnchorType = AnchorType.TorsoLength,
                StepCount = 3,
                StepDist = 0.131f,
                DistanceFromRootUp = 2.81f,
                DistanceFromRootDown = 1.288f,
                Direction = AnchorDir.Left
            };

            var spine1 = new List<Transform>
            {
                m_Animator.GetBoneTransform(HumanBodyBones.Chest),
            };

            var torsoWidthForwardBack = new JointAnchors
            {
                Anchors = spine1,
                AnchorType = AnchorType.TorsoWidth,
                StepCount = 2,
                StepDist = 0.179f,
                DistanceFromRootUp = 2.538f,
                DistanceFromRootDown = 2.51f,
                Direction = AnchorDir.Forward | AnchorDir.Back
            };

           
            
            var spineTorsoWidth = new List<Transform>
            {
                m_Animator.GetBoneTransform(HumanBodyBones.Spine),
            };

            var torsoWidthSpine = new JointAnchors
            {
                Anchors = spineTorsoWidth,
                AnchorType = AnchorType.TorsoWidth,
                StepCount = 2,
                StepDist = 0.176f,
                DistanceFromRootUp = 2.857f,
                DistanceFromRootDown = 2.891f,
                Direction = AnchorDir.Forward | AnchorDir.Back
            };
            
            m_JointAnchors = new List<JointAnchors>
            {
                armWidthForwardBack,
                armWidthUpDown,
                shoulderHeightUp,
                torsoLengthLeft,
                torsoWidthForwardBack,
                torsoWidthSpine,
            };
        }


       

#if UNITY_EDITOR
         [Button]
        private void SpawnAnchors()
        {
            foreach (var anchor in m_JointAnchors)
            {
                foreach (var sp in anchor.SpawnedPoints)
                {
                    if (sp != null)
                        DestroyImmediate(sp.gameObject);
                }

                anchor.SpawnedPoints.Clear();

                foreach (var joint in anchor.Anchors)
                {
                    int stepCount = Mathf.Max(0, anchor.StepCount);
                    if (stepCount == 0)
                    {
                        continue;
                    }

                    bool spawnUp = anchor.Direction.HasFlag(AnchorDir.Up);
                    bool spawnDown = anchor.Direction.HasFlag(AnchorDir.Down);
                    bool spawnLeft = anchor.Direction.HasFlag(AnchorDir.Left);
                    bool spawnRight = anchor.Direction.HasFlag(AnchorDir.Right);
                    bool spawnForward = anchor.Direction.HasFlag(AnchorDir.Forward);
                    bool spawnBack = anchor.Direction.HasFlag(AnchorDir.Back);

                    int lineCount = 0;
                    if (spawnUp) lineCount++;
                    if (spawnDown) lineCount++;
                    if (spawnLeft) lineCount++;
                    if (spawnRight) lineCount++;
                    if (spawnForward) lineCount++;
                    if (spawnBack) lineCount++;

                    if (lineCount == 0)
                    {
                        continue;
                    }

                    int total = stepCount * lineCount;

                    for (int i = 0; i < total; i++)
                    {
                        int lineIndex = i / stepCount;
                        int stepIndexInLine = i % stepCount;
                        int index = stepIndexInLine + 1;
                        bool isFirstInLine = stepIndexInLine == 0;

                        AnchorDir dirFlag = default;
                        int cursor = 0;

                        if (spawnUp)
                        {
                            if (cursor == lineIndex) dirFlag = AnchorDir.Up;
                            cursor++;
                        }

                        if (spawnDown)
                        {
                            if (cursor == lineIndex) dirFlag = AnchorDir.Down;
                            cursor++;
                        }

                        if (spawnLeft)
                        {
                            if (cursor == lineIndex) dirFlag = AnchorDir.Left;
                            cursor++;
                        }

                        if (spawnRight)
                        {
                            if (cursor == lineIndex) dirFlag = AnchorDir.Right;
                            cursor++;
                        }

                        if (spawnForward)
                        {
                            if (cursor == lineIndex) dirFlag = AnchorDir.Forward;
                            cursor++;
                        }

                        if (spawnBack)
                        {
                            if (cursor == lineIndex) dirFlag = AnchorDir.Back;
                            cursor++;
                        }

                        Vector3 dirVec = dirFlag switch
                        {
                            // +/- X axis in your convention: Right uses +up, Left uses -up
                            AnchorDir.Right => joint.up,
                            AnchorDir.Left => -joint.up,

                            // +/- Y axis in your convention: Up uses -forward, Down uses +forward
                            AnchorDir.Up => -joint.forward,
                            AnchorDir.Down => joint.forward,

                            // +/- Z axis in your convention: Forward/Back use joint.right
                            AnchorDir.Forward => joint.right,
                            AnchorDir.Back => -joint.right,

                            _ => joint.forward
                        };

                        float stepDist = anchor.StepDist * 0.1f;

                        // Use DistanceFromRootUp for "positive" directions (Right/Up/Forward)
                        // Use DistanceFromRootDown for "negative" directions (Left/Down/Back)
                        bool isPositive = dirFlag == AnchorDir.Right || dirFlag == AnchorDir.Up ||
                                          dirFlag == AnchorDir.Forward;

                        float offset = (isPositive ? anchor.DistanceFromRootUp : anchor.DistanceFromRootDown) * 0.05f;

                        float offSetAdder = isFirstInLine ? 0f : offset;
                        float totalDis = isFirstInLine ? offset + stepDist : stepDist;

                        GameObject sphere = Instantiate(m_PointPrefab, joint);
                        sphere.transform.position = joint.position + dirVec * ((index * totalDis) + offSetAdder);
                        anchor.SpawnedPoints.Add(sphere.transform);

                        string dirName = dirFlag + "_";
                        sphere.gameObject.name = $"{anchor.AnchorType}_Anchor_{dirName}{index}";

                        if (sphere.TryGetComponent<DebugSphere>(out var dbs))
                        {
                            dbs.Radius = Math.Min(stepDist / 6f, 0.05f);
                            dbs.Color = GetColorDebug(anchor.AnchorType);
                        }
                    }
                }
            }
        }
        private void SpawnPoints()
                {
                    foreach (var anchor in m_JointAnchors)
                    {
                        foreach (var sp in anchor.SpawnedPoints)
                        {
                            if (sp != null)
                                DestroyImmediate(sp.gameObject);
                        }
        
                        anchor.SpawnedPoints.Clear();
        
                        foreach (var joint in anchor.Anchors)
                        {
                            for (int i = 0; i < anchor.StepCount * 2f; i++)
                            {
                                GameObject sphere = Instantiate(m_PointPrefab, joint);
        
                                var index = ((i) % anchor.StepCount) + 1;
        
                                var isUp = i >= anchor.StepCount;
                                var isFirstInLine = i % anchor.StepCount == 0;
        
                                var stepDist = anchor.StepDist * 0.1f;
        
                                var offset = isUp ? anchor.DistanceFromRootUp * 0.05f : anchor.DistanceFromRootDown * 0.05f;
                                var offSetAdder = isFirstInLine ? 0f : offset;
                                var totalDis = (isFirstInLine ? offset + stepDist : stepDist);
        
                                var dirVec = i >= anchor.StepCount ? -joint.forward : joint.forward;
        
                                sphere.transform.position = joint.position + dirVec * (((index) * totalDis) + offSetAdder);
                                anchor.SpawnedPoints.Add(sphere.transform);
        
                                var nameST = ((i >= anchor.StepCount) ? "Up_" : "Down_") + index;
                                sphere.gameObject.name = anchor.AnchorType.ToString() + "_Anchor_" + nameST;
                                if (sphere.TryGetComponent<DebugSphere>(out var dbs))
                                {
                                    dbs.Radius = Math.Min(stepDist / 4f, 0.05f);
                                    dbs.Color = GetColorDebug(anchor.AnchorType);
                                }
        
                                //sphere.transform.localScale = Vector3.one * 0.02f;
                            }
                        }
                    }
                }
        
#endif

    }
}