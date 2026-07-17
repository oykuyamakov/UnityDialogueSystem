# GraphixSystem

An alternative to Button, better, more versatile, and faster.

GraphixSystem is a small UI framework for building selectable image grids, galleries, option buttons, card selectors, and layered UI visuals in Unity.

Define how a UI graphic should look in its default and selected states, choose whether it is a single-slot or nested-slot graphic, and let the group handle child creation, selection state, deselection, multi-select limits, visual updates, and optional selection FX.

<br><br><img src="Docs~/slc1.gif" alt="ring" width="1000"><br><br>

```csharp
public class GalleryExample : MonoBehaviour
{
    [SerializeField]
    private SelectixGroup m_Group;

    [SerializeField]
    private List<Sprite> m_Sprites;

    private void Start()
    {
        m_Group.OnImage += OnImageSelectionChanged;

        m_Group.SetContent(
            m_Sprites,
            preserveAspect: true
        );
    }

    private void OnImageSelectionChanged(ISelectix image, bool selected)
    {
        Debug.Log($"Image {image.RuntimeID} selected: {selected}");
    }
}
```



---

## Why

1. **One script to set sprites/textures.**
2. **One script to tint selected/unselected states.**
3. **One script to handle pointer input.**
4. **One script to tell siblings to deselect.**
5. **One script to spawn highlight FX.**
6. **One more script ( :p ) because the design suddenly needs a background image too.**

<img src="Docs~/ring.gif" alt="ring" width="400"><br><br>

How :

| Concept               | Responsibility                                                              |
| --------------------- | --------------------------------------------------------------------------- |
| `Slot`                | Holds graphic type, colors, outline settings, and bound `Graphic` reference |
| `Graphix`             | Single UI visual with content assignment and default/selected states        |
| `NestedGraphix`       | Two-slot UI visual: root slot + child slot                                  |
| `Selectix`            | Selectable single-slot graphic                                              |
| `NestedSelectix`      | Selectable nested graphic                                                   |
| `SelectixGroup`       | Owns children, pushes settings, assigns content, tracks selection           |
| `SelectionController` | Handles pointer selection, locking, deselection, group rules, and FX        |

The result is a reusable UI selection system that can be configured from the inspector and driven from code only where it actually matters.

---

## Core Idea

> A `Graphix` is a visual slot.
> A `Selectix` is a clickable/selectable `Graphix`.
> A `SelectixGroup` owns many of them and decides how selection behaves.

A simple UI image uses one slot:

```text
Selectix
└── Root Slot
```

A nested UI image uses two slots:

```text
NestedSelectix
├── Root Slot
└── Main Child / Child Slot
```

That means one system can cover both:

* icon buttons
* selectable gallery thumbnails
* cards with a colored frame and image inside
* foreground/background image pairs
* option grids
* nested image masks or framed content
* single-select and multi-select UI flows

Basically: if your UI element has “normal vs selected” visuals, this system wants to own it.

---

## Quick Start

### 1. Add a `SelectixGroup`

Create a UI parent object and add:

```csharp
SelectixGroup
```

The group automatically caches:

```csharp
[SerializeField, CachedField(RefSearch.Children, includeInactive: true)]
private List<Graphix> m_SImages;

[CachedField(addComponentIfMissing: true)]
private Canvas m_Canvas;

[CachedField(addComponentIfMissing: true)]
private GraphicRaycaster m_GraphicRaycaster;
```

So the group owns its child graphics and has the UI infrastructure needed for pointer interaction.

<br><br><img src="Docs~/slc2.gif" alt="slc2" width="1000"><br><br>

### 2. Choose the mode

In the group setup, choose:

| Mode     | Child type       |
| -------- | ---------------- |
| `Single` | `Selectix`       |
| `Nested` | `NestedSelectix` |

`Single` is for one visual layer.

`Nested` is for a root visual plus a child visual, useful for frames, backgrounds, image cards, and any “content inside container” layout.

### 3. Set child count and refresh

Set the desired child count, then press `Refresh`.

The group creates children named:

```text
SImage_0
SImage_1
SImage_2
...
```

Depending on the mode, it creates either `Selectix` or `NestedSelectix` children.

### 4. Configure visuals

Each slot supports:

| Setting          | Meaning                  |
| ---------------- | ------------------------ |
| `Type`           | `Image` or `RawImage`    |
| `Default Color`  | Color when unselected    |
| `Selected Color` | Color when selected      |
| `Use Outline`    | Enables outline support  |
| `Outline Color`  | Color used when selected |
| `Outline Width`  | Width used when selected |

The group pushes these settings into its children through `EditorBind`.

### 5. Feed content

```csharp
m_Group.SetContent(mySprites, preserveAspect: true);
```

For nested mode, you can also provide a shared back sprite:

```csharp
m_Group.SetContent(
    content: mySprites,
    preserveAspect: true,
    back: backgroundSprite
);
```

The group decides which slot receives the main content through `m_MainContentSlot`.

---

## Runtime Selection

Every selectable child implements:

```csharp
public interface ISelectix
{
    int RuntimeID { get; }
    bool IsLocked { get; }

    void Lock(bool disableVis);
    void UnLock();
    void TryDeselect();
    void SetGroup(int runtimeId, SelectixGroup group);
    void EditorBind(SelectixGroup group);
    void Init();

    RectTransform CachedRectTransform { get; }

    void UpdateVisuals(bool selected);
}
```

At runtime, `SelectixGroup` assigns each child a `RuntimeID`:

```csharp
private void InitImages()
{
    for (int i = 0; i < m_SImages.Count; i++)
    {
        var selective = m_SImages[i] as ISelectix;
        selective?.SetGroup(i, this);
    }
}
```

When a child is clicked:

```text
PointerDown
→ SelectionController.HandlePointerDown()
→ TrySelect() / TryDeselect()
→ UpdateVisuals()
→ optional FX
→ group event
→ group updates selected set
```

The group exposes one clean callback:

```csharp
public Action<ISelectix, bool> OnImage;
```

Example:

```csharp
m_Group.OnImage += (image, selected) =>
{
    if (selected)
        Debug.Log($"Selected image index: {image.RuntimeID}");
};
```

---

## Selection Rules

Selection behavior is controlled by the group and pushed into each child.

| Setting                    | Behavior                                                                          |
| -------------------------- | --------------------------------------------------------------------------------- |
| `Allow Multiple Selection` | More than one child can stay selected                                             |
| `Multi Select Limit`       | Maximum selected children when multi-select is enabled                            |
| `Allow Deselection`        | Clicking a selected item can deselect it                                          |
| `Allow Overriden Children` | Children can expose their own settings instead of fully inheriting group settings |
| `FX Foreground`            | Optional FX spawned above the selected rect                                       |
| `FX Background`            | Optional FX spawned behind the selected rect                                      |

Single-selection behavior is event-based.

When one `Selectix` is selected, other children in the same group hear the selection event and deselect themselves unless multi-select is enabled.

```csharp
private void OnOtherSelected(SGraphixSelectedEvent evt)
{
    if (evt.Selectix != null && evt.Selectix == m_Owner)
        return;

    if (m_Locked)
        return;

    if (m_MultiSelectable)
        return;

    if (m_Selected)
        TryDeselect();
}
```

So each child stays small, and the group remains the source of truth.

---

## Locking

You can lock every child:

```csharp
m_Group.LockAll(disableVis: true);
```

Unlock every child:

```csharp
m_Group.UnlockAll();
```

Or deselect all:

```csharp
m_Group.DeselectAll();
```

Individual children also expose:

```csharp
selectix.Lock(disableVis: true);
selectix.UnLock();
selectix.TryDeselect();
```

A locked item ignores pointer selection until unlocked.

---

## Single vs Nested

### `Graphix`

A `Graphix` owns one `Slot`.

```csharp
public class Graphix : CacherMonoBehaviour
{
    [SerializeField]
    protected Slot m_Slot;

    public virtual void SetContent(Sprite content, bool preserveAspect, bool fullStretch);
    public virtual void SetContent(Texture content);
    public virtual void UpdateVisuals(bool selected);
}
```

Use this for simple UI elements where the same image is both the content and the selectable visual.

### `NestedGraphix`

A `NestedGraphix` owns two slots:

```csharp
protected Slot m_Slot;
protected Slot m_ChildSlot;
```

The root slot usually acts like the frame, background, border, or container.

The child slot usually acts like the actual content image.

```csharp
nested.SetContent(
    content,
    preserveAspect: true,
    fullStretch: false,
    location: SlotLoc.Child
);
```

Available locations:

| `SlotLoc` | Target     |
| --------- | ---------- |
| `This`    | Root slot  |
| `Child`   | Child slot |

Nested mode is useful when selection needs to affect both layers:

```csharp
public override void UpdateVisuals(bool selected)
{
    ApplySlotVisual(m_Slot, selected);

    if (m_Mode == IsNested.Nested)
        ApplySlotVisual(m_ChildSlot, selected);
}
```

---

## Slot System

`SlotConfig` is the serialized visual configuration.

```csharp
public class SlotConfig
{
    protected GraphicType m_Type;
    protected Color m_DefaultColor;
    protected Color m_SelectedColor;
    protected bool m_UseOutline;
    protected Color m_OutlineColor;
    protected float m_OutlineWidth;
}
```

`Slot` extends it with runtime bindings:

```csharp
public class Slot : SlotConfig
{
    private Graphic m_Graphic;
    private UIOutline m_Outline;

    public void Bind(Graphic graphic, UIOutline outline);
    public void ApplyDefault();
    public void ApplySelected();
    public void SetSprite(Sprite sprite);
    public void SetTexture(Texture texture);
}
```

A slot can target either:

| Type    | Unity component           |
| ------- | ------------------------- |
| `Image` | `UnityEngine.UI.Image`    |
| `Raw`   | `UnityEngine.UI.RawImage` |

Sprite and texture assignment are handled automatically:

```csharp
slot.SetSprite(sprite);
slot.SetTexture(texture);
```

If a `Texture2D` is assigned to an `Image`, the system creates and caches a `Sprite` for it.

---

## Content Layout

For `Image` content, `Graphix` supports preserve-aspect behavior:

```csharp
SetContent(sprite, preserveAspect: true, fullStretch: false);
```

| Option                                       | Behavior                                         |
| -------------------------------------------- | ------------------------------------------------ |
| `preserveAspect: true`, `fullStretch: true`  | Stretches rect while preserving image aspect     |
| `preserveAspect: true`, `fullStretch: false` | Uses native size and centers the rect            |
| `preserveAspect: false`                      | Full-stretches the rect into the available space |

Nested content uses the same logic, but lets you choose which slot receives the content.

---

## Editor Behavior

GraphixSystem is editor-friendly by design.

In editor sync, `Graphix`:

1. Ensures the host has the correct `Graphic` component.
2. Replaces `Image` / `RawImage` if the configured slot type changes.
3. Binds the slot to the actual `Graphic`.
4. Adds or disables `UIOutline` depending on slot settings.
5. Applies default visuals.

For nested graphics, `NestedGraphix` also ensures a child object named `Main` exists:

```text
NestedSelectix
└── Main
    └── Image / RawImage
```

If the child does not exist, it is created automatically.

If a child named `Main` exists but is not a UI `RectTransform`, it is renamed and a valid UI child is created instead. We do not negotiate with cursed hierarchy states.

---

## Optional FX

Selection FX are handled by `SelectionController`.

Each selected child can spawn:

| FX Layer      | Sorting                        |
| ------------- | ------------------------------ |
| Background FX | group canvas sorting order     |
| Foreground FX | group canvas sorting order + 1 |

```csharp
if (m_FxBackground != UIFxType.None)
    m_ActiveFxBg = m_Pooler.ShowFxAtRect(...);

if (m_FxForeground != UIFxType.None)
    m_ActiveFxFg = m_Pooler.ShowFxAtRect(...);
```

FX are cleaned up on deselection, disable, destroy, and lock when requested.

---

## DOTween Support

If `CADI_DOTWEEN` is defined, `Slot.DoColor` uses DOTween:

```csharp
slot.DoColor(Color.red, 0.25f);
```

Without `CADI_DOTWEEN`, the same method falls back to immediate color assignment.

So code using `DoColor` still compiles even when DOTween is not installed. Very civilized.

---

## Odin Inspector Support

If `ODIN_INSPECTOR` is defined, the system uses Odin attributes for a cleaner inspector:

* foldout groups
* inline slot editors
* toggle buttons
* value-change refresh
* compact slot layout

Without Odin, the serialized fields still exist and the runtime system still works.

Odin is a UI upgrade, not a hard dependency.

---

## Design Decisions

### Why composition for selection?

`NestedSelectix` does not inherit from `Selectix`.

Instead:

```text
Graphix
├── Selectix
└── NestedGraphix
    └── NestedSelectix
```

Both `Selectix` and `NestedSelectix` own a `SelectionController`.

That avoids forcing nested visuals into the wrong inheritance chain. Unity/C# does not support multiple class inheritance, so this design keeps visual structure inherited and selection behavior composed.

Basically: inherit the shape, compose the behavior.

### Why group-owned settings?

Because UI consistency should not depend on manually syncing 30 child objects.

The group owns the shared contract:

* slot colors
* graphic type
* outline settings
* nested mode
* content padding
* selection rules
* FX settings

Children can optionally expose overrides, but the default workflow is group-driven.

### Why event-based selection?

A selected child does not need to know its siblings.

It sends:

```csharp
SGraphixSelectedEvent
```

The group tracks selected items, and other children in the same group respond through the event system.

This keeps children decoupled and makes single-select behavior reusable.

### Why support both `Image` and `RawImage`?

Because Unity UI projects always end up mixing both.

* `Image` is convenient for sprites and preserve-aspect UI.
* `RawImage` is useful for textures, render textures, downloaded images, camera feeds, and runtime-generated content.

The slot abstraction lets the same higher-level code handle both.

---

## Requirements

Required:

* Unity UI
* Cadi `CacherSystem`
* Cadi `EventSystem`
* Cadi UI extensions, including `SetFullStretch`
* `UIOutline` if using outlines
* `UIFXPooler` / `UIFxType` if using selection FX

Optional:

* Odin Inspector via `ODIN_INSPECTOR`
* DOTween via `CADI_DOTWEEN`

---

## Files

| File                     | Purpose                                                                                                                                   |
| ------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `Slot.cs`                | Slot config, visual state application, sprite/texture assignment, outline handling, optional DOTween color tweening                       |
| `Graphix.cs`             | Base single-slot UI graphic with content assignment, cached RectTransform, editor sync, and slot binding                                  |
| `NestedGraphix.cs`       | Two-slot graphic with root + child visual layers and automatic `Main` child creation                                                      |
| `ISelectix.cs`           | Shared interface for selectable graphix components                                                                                        |
| `SelectionController.cs` | Selection state machine, locking, deselection, group subscription, and optional selection FX                                              |
| `Selectix.cs`            | Selectable single-slot `Graphix`                                                                                                          |
| `NestedSelectix.cs`      | Selectable nested `NestedGraphix`                                                                                                         |
| `SelectixGroup.cs`       | Parent controller that creates/adjusts children, pushes settings, assigns content, tracks selected items, and exposes selection callbacks |

There you go <3 

<img src="Docs~/dftjpg.jpg" alt="ring" width="400"><br><br>