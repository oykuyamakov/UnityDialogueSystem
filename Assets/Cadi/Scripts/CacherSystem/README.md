# CacherSystem

**Attribute-driven component reference resolution for Unity — resolve in the editor, verify at runtime, ship zero `GetComponent` calls.**

Declare *where* a reference lives instead of writing boilerplate to fetch it.
The system resolves references at edit time, serializes the results, and validates the whole project at build time :) so a correctly authored scene pays essentially nothing at runtime.

```csharp
public class Weapon : CacherMonoBehaviour
{
    [SerializeField, CachedField]
    private Rigidbody m_Rigidbody;                          // GetComponent on Self

    [SerializeField, CachedField(RefSearch.Children)]
    private List<ParticleSystem> m_MuzzleEffects;           // all in children, incl. inactive

    [SerializeField, CachedField(RefSearch.Parent, required: false)]
    private WeaponRack m_Rack;                              // optional, from parent

    [SerializeField, CachedField(addComponentIfMissing: true)]
    private AudioSource m_Audio;                            // added automatically if absent
}
```

<br><br><img src="Docs~/cacherexample.gif" alt="Cached References inspector" width="600">


No `Awake` boilerplate! No drag-and-drop maintenance! No null refs discovered at runtime! A missing required reference **fails the build** with
a descriptive warning. 

No way you still not convinced!! I should be a saleswoman as a side hustle.

---

## Why

Every Unity project accumulates the same two failure modes:

1. **`GetComponent` boilerplate** in `Awake`/`Start` — repetitive, easy to forget, and paid on every instantiation.
2. **Hand-wired inspector references** — silently broken by hierarchy refactors, discovered as `NullReferenceException` at runtime (or worse, in a shipped build).

CacherSystem replaces both with a declarative contract: the attribute states where the reference comes from, tooling keeps it satisfied, and the build pipeline proves it.

## The Core Guarantee

> **Anything authored in the editor — scene objects and prefabs — ships pre-resolved. The runtime resolution path exists only for objects constructed from scratch at runtime.**

This is enforced in layers:

| Layer | When | What it does |
|---|---|---|
| Deferred editor resolve | `OnValidate` / `Reset` | Coalesced via `EditorApplication.delayCall`; keeps references current as you edit |
| Custom inspector | Always | Shows cached refs read-only with status, config warnings, and a manual **Resolve Now** |
| Menu commands | On demand | `Tools → Auto References → Resolve` for build scenes or open scenes, with progress + summary |
| Build preprocessor | Every build | Re-resolves every build scene **and every prefab**; a missing required reference throws `BuildFailedException` |
| Runtime fast path | `Awake` | Verifies serialized state with a handful of null checks — no component searches on the happy path |

## Runtime Behavior

`CacherMonoBehaviour.Awake` runs a cheap verification:

```
m_IsResolved (serialized) && no required field is empty  →  early return
otherwise                                                →  full resolve, once
```

- **Scene objects / instantiated prefabs**: `m_IsResolved` was serialized at edit time and `Instantiate` remaps internal references, so clones inherit valid refs. Cost per object: one cached-dictionary lookup + a few reflection reads. No `GetComponent` calls.
- **Runtime-constructed objects** (`new GameObject()` + `AddComponent`): full resolution runs once at `Awake` — the same `GetComponent*` calls you'd have written by hand, plus microseconds of reflection overhead.
- **Nothing is per-frame.** All cost is at initialization.

Reflection metadata (`FieldInfo` + attribute data) is computed once per type and cached in a static dictionary. The cache is cleared on `SubsystemRegistration`, so **Enter Play Mode Options with domain reload disabled is fully supported** — no stale `FieldInfo`, no stuck statics.

Writes are change-detected: a resolve that finds identical values (including Unity fake-null equivalence) writes nothing. This keeps re-resolution idempotent and — combined with the build check — keeps already-resolved scenes **byte-identical across builds**, which is friendly to VCS diffs and build caching.

## `[CachedField]` Reference

```csharp
[CachedField(
    search:                RefSearch.Self,     // Self | Parent | Children
    includeInactive:       true,               // include inactive objects in Parent/Children searches
    required:              true,               // empty result = resolve error = build failure
    addComponentIfMissing: false,              // Self only: AddComponent when absent
    addIfMissingBoolField: null)]              // ...or gate the add on a bool field by name
```

| Field type | Resolution |
|---|---|
| `T` (Component) | `GetComponent` / `GetComponentInParent` / `GetComponentInChildren` |
| `T[]` | `GetComponents` / `GetComponentsInParent` / `GetComponentsInChildren` |
| `List<T>` | Same as arrays, materialized into the list |

Rules worth knowing:

- Fields must be serialized (`[SerializeField]` or public) — the inspector warns if they aren't. The resolver walks the inheritance chain, so private attributed fields on base classes resolve correctly.
- `required` for collections means *non-empty*, not just non-null.
- `addComponentIfMissing` is meaningful only for `RefSearch.Self` and never mutates prefab **assets** — component adds happen on instances or in the prefab stage, deferred outside `OnValidate` where `AddComponent` is illegal.
- `RefSearch.Parent` fields on prefabs resolve at runtime by design — the parent doesn't exist until the object is parented. The `Awake` verification detects the empty required field and resolves it then.

## Inspector

Cached fields are pulled out of the normal inspector flow and rendered in a dedicated read-only **Cached References** section (references are system-owned; hand-editing them would just be overwritten). The section shows resolve status, per-field attribute metadata (toggleable), configuration warnings, and a **Resolve Now** button with proper `Undo` support.

Works with or without Odin Inspector — with `ODIN_INSPECTOR` defined, an `OdinAttributeProcessor` hides the raw fields and the section is appended to the Odin-drawn inspector; otherwise a standard custom editor handles both.

## `CacherSingleton<T>`

A singleton base that composes with the resolution lifecycle instead of fighting it:

```csharp
public class AudioManager : CacherSingleton<AudioManager>
{
    [SerializeField, CachedField(RefSearch.Children)]
    private AudioSource[] m_Channels;

    protected override void OnSingletonAwake()
    {
        // runs after singleton enforcement AND after reference resolution
    }
}
```

- `Awake` is sealed at the base; derived classes use `OnSingletonAwake` / `OnSingletonDestroyed`. Singleton enforcement runs **before** resolution, so a duplicate is destroyed without paying the resolve cost.
- `Instance` lazily finds or creates; `TryGetInstance` / `HasInstance` for non-creating access; quit-safe (`s_IsQuitting` guards against ghost instances during teardown).
- `PersistAcrossScenes` (default `true`) controls `DontDestroyOnLoad`.
- `[DefaultExecutionOrder(-10_000)]` ensures singletons initialize before ordinary scripts.
- Statics reset on `SubsystemRegistration` — safe with domain reload disabled.

## Design Decisions

**Why serialize resolution instead of resolving lazily at runtime?**
Lazy resolution hides authoring errors until the code path executes — possibly on a player's machine. Serialized resolution + a build gate moves the failure to the earliest possible moment: your build machine.

**Why reflection instead of source generation?**
The reflection cost sits exclusively on cold paths (once per type for the scan, once per *runtime-constructed* object for field writes).
Source-generated resolvers would shave microseconds off a path that object pooling makes irrelevant anyway. Simplicity wins until a profiler says otherwise. PLS DM ME IF THAT HAPPENS
I'll GIFT YOU A FREE 5-MIN SHADER :D

**Why are cached fields read-only in the inspector?**
Single source of truth! If a reference is declared as "first `AudioSource` in children," a hand-assigned override is a lie waiting to desync. 
Want manual control over a field? Don't put `[CachedField]` on it :)

**Why fail the build instead of warning?**
A warning in a 500-line console is a missed warning. A failed build is a fixed scene. 
<br><br><img src="Docs~/img.png" alt="Cached References inspector" width="100">

## Requirements

- Optional: Odin Inspector (auto-detected via `ODIN_INSPECTOR` define)

## Files

| File | Purpose |
|---|---|
| `CacherMonoBehaviour.cs` | Attribute, resolver core, runtime verification, deferred editor resolve |
| `CacherSingleton.cs` | Singleton base built on the resolution lifecycle |
| `CacherEditor.cs` | Custom inspector (Odin + vanilla), read-only cached section, config warnings |
| `CacherBehavioursResolveMenu.cs` | Batch resolve menu commands with progress and summary dialogs |
| `CacherReferenceBuildCheck.cs` | `IPreprocessBuildWithReport` gate: resolves scenes + prefabs, fails on missing required refs |
