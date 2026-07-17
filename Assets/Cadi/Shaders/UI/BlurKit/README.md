# BlurKit

UI Blur effect for Unity UI. Displays the top content with a frosted glass blur applied to the background canvases. 
For popups, menus, overlays etc.

---

## How it works

Captures the scene with a dedicated camera, applies a multi-pass separable blur via a shader, and displays the result on a `RawImage` in the UI.
Background canvases are temporarily switched to **Screen Space - Camera** mode while blur is active so they appear in the captured image.

---

## Setup

**1. Place the prefab**
Drop `BlurCamera.prefab` into your UI Canvas hierarchy. It contains the child `Camera` and `RawImage` — these are auto-assigned at runtime if left empty.

**2. Assign Background Canvases**
Add every Canvas that should appear blurred in the background to the **Background Canvases** list. These are the canvases rendered beneath your blur overlay (e.g. your main HUD, world-space UI, etc.).

> When blur is enabled, each canvas is temporarily switched to **Screen Space - Camera** mode using the blur camera so it gets captured. Their original render modes are restored when blur is disabled.

**3. Set the Blur Camera's Culling Mask**
This is the camera that captures the scene for blurring. It should include ONLY the LAYER(s) of objects you want be in the the background 


> Layer setup example
>> - the background canvases you want blurred are on `BGCanvasLayer1` and `BGCanvasLayer2`
>> - your front canvas is on `FrontCanvasLayer`
> - > Then the Blur Camera's culling mask should include `BGCanvasLayer1`, `BGCanvasLayer2`, but **not** `FrontCanvasLayer`.

**4. Position the RawImage**
The child `RawImage` displays the blurred output. Size and position it to cover the area you want frosted (typically fullscreen).


**5. Add your content**
Place your popup/menu/overlay content under/in ContentHere — this will be rendered on top of the blurred background when blur is enabled.


**6. Adjust Blur Amount**
In the Inspector, set `Blur Amount` (0–1) to control the intensity of the blur effect. Higher values = blurrier.

---

## Usage

```csharp
blur.EnableBlur();       // capture and show blur
blur.DisableBlur();      // hide blur, restore canvas render modes
blur.ToggleBlur(bool);   
```

`Blur Amount` (0–1) in the Inspector controls blur intensity. Access the blurred texture directly via `blur.BlurredTexture` if needed elsewhere.