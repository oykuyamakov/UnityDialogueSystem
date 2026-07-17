VJ Rule System - Data Structure Plan

Context

Real-time VJ/performans uygulamasi icin event-driven otomasyon sistemi. Graph editor uzerinde Trigger Box ve Result Box'lar bagimsiz node'lar olarak
var olacak, edge'ler ile birbirine baglanarak Rule'lar olusturacak. Mevcut dialogue sisteminin dual-layer (Data + Editor) yapisi ve GraphView
tooling'i referans alinacak.

Graph Semantics (Vertical Layout - Top to Bottom)

- Trigger → Result edge: Rule olusturur
- Trigger → Trigger (vertical chain): AND
- Multiple Triggers fan-in to same target: OR
- Result → Result edge: Sequential execution (delay metadata on edge)
- Result → Default State Box (horizontal/right): Trigger deaktif oldugunda geri donulecek state

Folder Structure

Assets/realt_vj/
├── Scripts/
│   ├── Data/                          # Serializable, runtime-safe
│   │   ├── Boxes/
│   │   │   ├── BoxData.cs             # Base: GUID, position (like NodeData)
│   │   │   ├── TriggerBoxData.cs      # List<Trigger>, CooldownMode, cooldown params
│   │   │   ├── ResultBoxData.cs       # List<Result>, ExecutionMode, loopCount
│   │   │   └── DefaultStateBoxData.cs # List<Result> (filtered by parent result types)
│   │   ├── Edges/
│   │   │   └── EdgeData.cs            # SourceGuid, TargetGuid, EdgeType enum, metadata (delay etc.)
│   │   ├── Triggers/
│   │   │   ├── Trigger.cs             # Abstract base
│   │   │   ├── HandTrigger.cs         # WhichHand, HandPosition, HandGesture
│   │   │   ├── MusicTrigger.cs        # MusicTriggerType enum + params
│   │   │   └── TimeTrigger.cs         # TimeTriggerType enum + params
│   │   ├── Results/
│   │   │   ├── Result.cs              # Abstract base
│   │   │   ├── ObjectResult.cs  # GameObject ref, active/deactive, transform operantions (scale,rotate etc.)
│   │   │   ├── AnimationResult.cs     # Animator ref, trigger name
│   │   │   ├── VolumeSettingResult.cs  # Volume profile ref, property, value
│   │   │   ├── SoundResult.cs         # AudioClip ref, volume
│   │   │   ├── CameraSwitchResult.cs  # Camera ref, mode( full, left half of the screen etc.)
│   │   │   └── CameraMoveResult.cs    # Target position/rotation, duration
│   │   ├── Groups/
│   │   │   └── RuleGroupData.cs       # Group name, List<string> member box GUIDs
│   │   ├── Enums/
│   │   │   └── VJEnums.cs             # All enums in one file
│   │   └── RuleGraphContainer.cs      # Main ScriptableObject
│   └── Editor/                        # Editor-only, GraphView
│       ├── VJGraph.cs                 # EditorWindow (like DialogueGraph)
│       ├── VJGraphView.cs             # GraphView impl (like DialogueGraphView)
│       ├── Nodes/
│       │   ├── VJNode.cs              # Abstract base (like DialogueNode)
│       │   ├── TriggerBoxNode.cs      # Visual trigger box
│       │   ├── ResultBoxNode.cs       # Visual result box
│       │   └── DefaultStateNode.cs    # Visual default state box
│       ├── GraphSaveUtility.cs        # Save/Load (like existing)
│       └── NodeSearchWindow.cs        # Right-click creation menu

Data Classes Detail

1. BoxData (Base)

// Mirrors existing NodeData pattern
[Serializable]
public class BoxData : ISerializationCallbackReceiver
{
[SerializeField] public string Guid;
[SerializeField] float GraphX, GraphY;
[JsonIgnore] public Vector2 GraphPosition;
// OnBeforeSerialize / OnAfterDeserialize for Vector2 sync
}

2. Trigger System (Polymorphic via SerializeReference)

[Serializable]
public abstract class Trigger
{
public abstract TriggerType Type { get; }
public abstract Trigger Clone();
}

[Serializable]
public class HandTrigger : Trigger
{
[SerializeField] WhichHand m_Which;           // Left, Right, LeftOrRight, LeftAndRight
[SerializeField] HandPosition m_LeftPosition;  // LeftScreen, RightScreen, Middle, Anywhere, NotOnScreen
[SerializeField] HandPosition m_RightPosition; // Only used when LeftAndRight
[SerializeField] HandGesture m_Gesture;        // ThumbsUp, Fist, Peace, etc.
// Position/Gesture fields are optional - default = Anywhere/Any
// Filtering: editor hides invalid combos based on Which selection
}

[Serializable]
public class MusicTrigger : Trigger
{
[SerializeField] MusicTriggerType m_MusicTriggerType; // EveryNBeats, EnergyAbove, FrequencyBand, BPMChange, Silence
[SerializeField] float m_Value;                        // Beat count, threshold, etc.
}

[Serializable]
public class TimeTrigger : Trigger
{
[SerializeField] TimeTriggerType m_TimeTriggerType; // After, Every, RepeatN, Delay
[SerializeField] float m_Duration;
[SerializeField] int m_RepeatCount;                 // -1 = infinite
}

3. TriggerBoxData

[Serializable]
public class TriggerBoxData : BoxData
{
[SerializeReference] List<Trigger> m_Triggers = new(); // AND relation inside box

     [SerializeField] CooldownMode m_CooldownMode;  // Full, ForDuration, EveryInterval
     [SerializeField] float m_CooldownDuration;       // Seconds (used by ForDuration and EveryInterval)
}

public enum CooldownMode
{
Full,           // Active as long as condition is met
ForDuration,    // Active for X seconds after trigger, then stop
EveryInterval   // Re-fires every X seconds while condition met
}

4. Result System (Polymorphic via SerializeReference)

[Serializable]
public abstract class Result
{
public abstract ResultType Type { get; }
public abstract Result Clone();
}

[Serializable]
public class ObjectActiveResult : Result
{
[SerializeField] string m_TargetObjectPath; // Scene path for runtime resolve (build-safe)
[SerializeField] bool m_SetActive;
}

[Serializable]
public class AnimationResult : Result
{
[SerializeField] string m_TargetObjectPath;
[SerializeField] string m_TriggerName;
}

[Serializable]
public class VolumeSettingResult : Result
{
// Volume profile property targeting (hue, color, etc.)
[SerializeField] string m_TargetObjectPath;
[SerializeField] string m_PropertyName;
[SerializeField] float m_Value;
[SerializeField] Color m_ColorValue;
}

[Serializable]
public class SoundResult : Result
{
[SerializeField] AudioClip m_Clip; // AssetReference for build
[SerializeField] float m_Volume;
}

[Serializable]
public class CameraSwitchResult : Result
{
[SerializeField] string m_TargetCameraPath;
}

[Serializable]
public class CameraMoveResult : Result
{
[SerializeField] Vector3 m_TargetPosition;
[SerializeField] Vector3 m_TargetRotation;
[SerializeField] float m_Duration;
}

5. ResultBoxData

[Serializable]
public class ResultBoxData : BoxData
{
[SerializeReference] List<Result> m_Results = new();

     [SerializeField] ResultExecutionMode m_ExecutionMode; // ATST, InOrder
     [SerializeField] int m_LoopCount;                      // -1 = infinite, N = N times
}

public enum ResultExecutionMode
{
ATST,    // At The Same Time (parallel)
InOrder  // Sequential
}

6. DefaultStateBoxData

[Serializable]
public class DefaultStateBoxData : BoxData
{
[SerializeReference] List<Result> m_DefaultResults = new();
// Result types filtered to match parent ResultBox's result types
// Executes when the trigger condition becomes false
}

7. EdgeData

[Serializable]
public class EdgeData
{
[SerializeField] string m_SourceGuid;    // Output box GUID
[SerializeField] string m_TargetGuid;    // Input box GUID
[SerializeField] EdgeType m_EdgeType;     // Derived from connected box types
[SerializeField] float m_Delay;           // Used for Result→Result sequential delay
}

public enum EdgeType
{
TriggerToResult,    // Rule creation
TriggerToTrigger,   // AND chain (vertical)
ResultToResult,     // Sequential execution
ResultToDefault     // Default state link (horizontal)
}

8. RuleGraphContainer (Main ScriptableObject)

[Serializable]
public class RuleGraphContainer : ScriptableObject
{
[SerializeField] public List<TriggerBoxData> TriggerBoxes = new();
[SerializeField] public List<ResultBoxData> ResultBoxes = new();
[SerializeField] public List<DefaultStateBoxData> DefaultStateBoxes = new();
[SerializeField] public List<EdgeData> Edges = new();
[SerializeField] public List<RuleGroupData> RuleGroups = new();

     // Runtime caches (non-serialized) - built on initialize
     // Dictionary<string, BoxData> m_BoxesByGuid
     // Dictionary<string, List<EdgeData>> m_EdgesBySourceGuid
     // Dictionary<string, List<EdgeData>> m_EdgesByTargetGuid
     // List<ResolvedRule> m_ResolvedRules  -- flattened trigger expressions → result chains
}

9. RuleGroupData

[Serializable]
public class RuleGroupData
{
[SerializeField] string m_GroupName;
[SerializeField] List<string> m_MemberGuids = new(); // GUIDs of boxes in this group
[SerializeField] bool m_IsActive;                     // Runtime toggle
}

Edge Semantics & Boolean Logic (via Topology)

Fan-in = OR:
[TB1] ──┐
├──▶ [TB3 or ResultBox]    means: TB1 OR TB2
[TB2] ──┘

Chain = AND:
[TB1] ──▶ [TB2] ──▶ [Result]        means: TB1 AND TB2

Combined:
[TB1] ──┐
├──▶ [TB3] ──▶ [Result]    means: (TB1 OR TB2) AND TB3
[TB2] ──┘

Result chain with delay:
[Result1] ──(0.5s)──▶ [Result2]     means: Result1 then 0.5s delay then Result2

Default state:
[Result1] ──horizontal──▶ [DefaultState]   means: when trigger inactive, apply DefaultState

Port Configuration (Vertical Layout)

TriggerBoxNode:
- Top: Input port (from other TriggerBox = AND chain)
- Bottom: Output port (to TriggerBox for AND, or to ResultBox for rule)

ResultBoxNode:
- Top: Input port (from TriggerBox or another ResultBox)
- Bottom: Output port (to another ResultBox for sequential chain)
- Right: Output port (to DefaultStateBox)

DefaultStateBoxNode:
- Left: Input port (from ResultBox)

Enums

// Hand Trigger
public enum WhichHand { Left, Right, LeftOrRight, LeftAndRight }
public enum HandPosition { LeftScreen, RightScreen, MiddleScreen, Anywhere, NotOnScreen }
public enum HandGesture { Any, ThumbsUp, ThumbsDown, One, Two, Three, Four, Five, Pinching, FingerGun, Fist, Peace, RockAndRoll, HandsTogether,
HandsApart }

// Music Trigger
public enum MusicTriggerType { EveryNBeats, EnergyAbove, FrequencyBandEnergy, BPMChange, SilenceDetection, BeatDrop }

// Time Trigger
public enum TimeTriggerType { AfterDuration, EveryInterval, RepeatN, DelayedStart }

// Result
public enum ResultType { ObjectActive, Animation, VolumeSetting, Sound, CameraSwitch, CameraMove }

// Trigger
public enum TriggerType { Hand, Music, Time }

// System
public enum CooldownMode { Full, ForDuration, EveryInterval }
public enum ResultExecutionMode { ATST, InOrder }
public enum EdgeType { TriggerToResult, TriggerToTrigger, ResultToResult, ResultToDefault }

Hand Trigger Filtering Rules (Editor-time)

When Which changes, editor dynamically filters available options:

┌──────────────┬─────────────────────────────┬────────────────────────────────┐
│    Which     │    Position fields shown    │         Gesture filter         │
├──────────────┼─────────────────────────────┼────────────────────────────────┤
│ Left         │ Single position dropdown    │ Hide HandsTogether, HandsApart │
├──────────────┼─────────────────────────────┼────────────────────────────────┤
│ Right        │ Single position dropdown    │ Hide HandsTogether, HandsApart │
├──────────────┼─────────────────────────────┼────────────────────────────────┤
│ LeftOrRight  │ Single position dropdown    │ Hide HandsTogether, HandsApart │
├──────────────┼─────────────────────────────┼────────────────────────────────┤
│ LeftAndRight │ Left pos + Right pos fields │ All gestures available         │
└──────────────┴─────────────────────────────┴────────────────────────────────┘

If no Position/Gesture explicitly added to box, defaults assumed: Anywhere + Any.

Runtime Resolution Strategy

At initialization, RuleGraphContainer traverses the graph to build ResolvedRule objects:

// Internal runtime structure (not serialized)
class ResolvedRule
{
TriggerExpression Expression;      // Tree of AND/OR trigger references
List<ResultChainEntry> Results;    // Ordered result chain with delays
List<DefaultStateEntry> Defaults;  // Default states per result
RuleGroupData Group;               // Which group this rule belongs to
}

Graph traversal algorithm:
1. Find all ResultBoxes that have incoming TriggerBox edges (these are rule endpoints)
2. For each such ResultBox, walk backwards through trigger edges to build expression tree
3. Fan-in parents = OR node, single parent chain = AND node
4. Walk forward from ResultBox through Result→Result edges to build execution chain
5. Collect Default State boxes connected to each ResultBox

Serialization Strategy

- [SerializeReference] for polymorphic Trigger and Result lists (same pattern as existing Olay system)
- ScriptableObject as main container (same as DialogueContainer)
- GUID-based references between boxes (same as existing node system)
- ISerializationCallbackReceiver for Vector2 position (same as NodeData)
- Edge data stored as flat list, runtime caches built on initialize

Implementation Order

1. Enums - VJEnums.cs
2. Base data classes - BoxData, EdgeData
3. Trigger classes - Abstract Trigger + HandTrigger, MusicTrigger, TimeTrigger
4. Result classes - Abstract Result + all concrete results
5. Box data classes - TriggerBoxData, ResultBoxData, DefaultStateBoxData
6. Container - RuleGraphContainer, RuleGroupData
7. Editor base - VJNode, VJGraphView, VJGraph (EditorWindow)
8. Editor nodes - TriggerBoxNode, ResultBoxNode, DefaultStateNode
9. Save/Load - GraphSaveUtility
10. Search window - NodeSearchWindow

Verification

- Create ScriptableObject asset manually, verify serialization roundtrip
- Open graph editor, create TriggerBox and ResultBox nodes
- Connect with edges, verify edge type detection
- Save and reload, verify all data persists
- Test Hand Trigger filtering in editor
- Test group selection via rubber-band + "Group Selected"
