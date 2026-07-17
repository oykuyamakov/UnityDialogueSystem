# Custom Attributes

A collection of Unity inspector attributes that make the Inspector more useful without any extra setup. Includes 
lightweight reimplementations of some handy Odin Inspector attributes, useful as a fallback when Odin isn't available.

---
<br><br><img src="Docs~/customattributes.gif" alt="customattributes inspector" width="600">

## [Button] - Running methods from the Inspector

**Files:** `ButtonAttribute.cs`, `Editor/ButtonEditor.cs`, `Editor/ButtonDrawerUtility.cs`

Adds a clickable button in the Inspector for any method.

**How to use:**
```csharp
[Button]
private void ResetState() { ... }

[Button("Spawn Enemy")]
private void SpawnEnemy() { ... }

// Methods with parameters get input fields drawn automatically
[Button]
private void SetHealth(int amount, bool notify = true) { ... }
```

- Supported parameter types: `int`, `float`, `bool`, `string`, `Vector2/3/4`, `Color`, `Enum`, `UnityEngine.Object`
- Parameter values are persisted between sessions via `EditorPrefs`
- Invokes on all selected objects at once, with Undo support
- Works on both `MonoBehaviour` and `ScriptableObject`

---

## [ShowIf] - Conditionally show fields

**Files:** `ShowIfAttribute.cs`, `Editor/ShowIfDrawer.cs`

Hides a field in the Inspector unless a condition is met. Keeps the Inspector clean.

**How to use:**
```csharp
public bool IsAdvanced;

[ShowIf("IsAdvanced")]
public float AdvancedMultiplier;

// Enum condition
public WeaponType Type;

[ShowIf("Type", (int)WeaponType.Ranged)]
public float Range;

// Not-equals shorthand
[ShowIfNotEquals("Type", (int)WeaponType.None)]
public float Damage;
```

- Condition can be a **field**, **property**, or **parameterless method**
- Comparison modes: `IsTrue`, `Equals`, `NotEquals`
- Composable with Unity's built-in `[Range]` attribute — both work together
- Multiple `[ShowIf]` attributes can stack on the same field (`AllowMultiple = true`)

---

## [DynamicRange] - Slider with runtime-bound limits

**Files:** `DynamicRangeAttribute.cs`, `Editor/DynamicRangeDrawer.cs`

Like Unity's `[Range]`, but the min/max can come from other fields instead of being hardcoded.

**How to use:**
```csharp
public float MinSpeed = 1f;[README.md](../../../../README.md)
public float MaxSpeed = 10f;

[DynamicRange("MinSpeed", "MaxSpeed")]
public float CurrentSpeed;

// Mix field and constant
[DynamicRange(0f, "MaxSpeed")]
public float BoostSpeed;

// Constant range (same as [Range] but explicit)[README.md](../../../../README.md)
[DynamicRange(0f, 100f)]
public float Health;
```

- Works with both `float` and `int` fields
- Bounds can be any combination of field name (string) or constant value (float)
- Works correctly inside arrays and nested structs

---

## [SpritePreview] - Thumbnail below Sprite fields

**Files:** `SpritePreviewAttribute.cs`, `Editor/SpritePreviewDrawer.cs`

Draws a visual preview of the assigned sprite directly in the Inspector.

**How to use:**
```csharp
[SpritePreview]
public Sprite Icon;

// Custom height
[SpritePreview(height: 128f)]
public Sprite Portrait;

// Show placeholder even when null
[SpritePreview(height: 64f, showWhenNull: true)]
public Sprite Background;
```

- Respects atlas packing — crops correctly from sprite sheets
- Maintains aspect ratio
- Default height: 64px

---

## Odin Inspector compatibility

All attributes work with and without Odin Inspector installed.

- When `ODIN_INSPECTOR` is defined, `[Button]` inherits from Odin's own `ButtonAttribute` so Odin renders it natively — no double-drawing.
- `[ShowIf]`, `[DynamicRange]`, and `[SpritePreview]` use standard `PropertyDrawer` and are unaffected by Odin.[README.md](../../../../README.md)