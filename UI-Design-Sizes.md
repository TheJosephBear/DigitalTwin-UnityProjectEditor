# UI design sizes

On-screen sizes for the runtime uGUI. All pixel values are measured at **1920×1080**. Since every runtime canvas matches screen height, each value scales with the window height (at 1440p everything is 4/3 bigger).

## Canvas setup

| Setting | Value |
|---|---|
| UI Scale Mode | Scale With Screen Size |
| Screen Match Mode | Match Width Or Height, **Match = 1 (height)** |
| Reference resolution | usually 1000×800; older canvases use 800×600, the Sun panel 1920×1080 |

With height matching, the reference resolution only sets the unit size. To turn Unity units into pixels at 1080p:

```
pixels = units × (product of localScale from the element up to the canvas) × canvas factor
```

| Reference resolution | Canvas factor at 1080p |
|---|---|
| 1000×800 | 1.35 |
| 800×600 | 1.8 |
| 1920×1080 | 1.0 |

**Exception:** `MultiViewUI` keeps Match = 0 (width), because its two render views are each exactly half the screen width. The legacy `Login`, `Register` and `ProjectsList` prefabs aren't covered here.

## Screen margins

Everything that floats over the 3D scene keeps **24 px** from the screen edges (M3's margin for large windows). That covers:

- **Top-left:** the UJEP logo.
- **Top-right:** the PerformanceTracker toggle and the FPS panel.
- **Left:** the camera column.
- **Right:** the HUD column.
- **Bottom-left:** the sun button.
- **Bottom-right:** the "Viditelnost pinů" and "Kolize kamery" toggles. They share one center line, so the wider label sets the 24 px.

On the geolocalization screen, the search bar, exit button, top-right buttons and lock toggle also sit at 24 px. The margin is measured to what's drawn (button, text), not to wider empty boxes.

## Icons

Icon color comes from `Assets/Resources/IconTheme.asset` through `ThemedIcon`: purple `#6A58A7` for icons, white for icons on dark backgrounds. Icons are Phosphor Regular (`Assets/Media/Sprites/Phosphor`).

| Element | Button | Icon | Used for |
|---|---|---|---|
| Square icon button, over the scene | **34 px** | **24 px** (70 %) | HUD right column, PerformanceTracker toggle, sun button, geolocalization top-right and exit buttons |
| Square icon button, in a modal | **26 px** | 22 px (84 %) | Action buttons in PinContext and ViewPointContext |
| Round icon button | **34 px** | **24 px** (70 %) | Cameras (ViewPointUI) and pins (PinUI) on the left |
| Close button (X) | **20 px** | 16 px (80 %) | Top-right corner of every modal |

Buttons that sit over the 3D scene are **34 px** (they used to be 26 / 28 px). Each group is scaled as a whole, so the gaps between buttons scale with them: 11.3 px in the HUD column, 17 px between the camera and pin columns. The icon is 70 % of the button (23.8 px). The `Icon` (or `Image` on the "+" buttons) is anchored 0.15–0.85 with zero offsets and scale 1. `MainButton` itself keeps its 84 % icon for the 26 px modal buttons, so the scene buttons override the anchors. That includes the buttons added only in `Editing.unity` (`OpenViewerOnTab`, `ScreenshotButton`, `FPSSettings`). `ViewButton` and `PinButton` are set in their own prefabs.

How to reproduce them:

- **Square button:** the `MainButton` prefab is 100×100 units. For 34 px use scale **0.252** on a 1000×800 canvas (0.193 gives 26 px), or 0.189 on 800×600 (0.144 gives 26 px). Inside a scaled modal container, divide by the container's scale.
- **HUD column:** `MainButtonsContainer/Scroll View` scale 0.840 (items 0.3), container at (−26.74, −51.84). The column has a 24 px right margin, and its first button (3D model) starts 11.3 px below the PerformanceTracker toggle (top 69 px). The Editing scene must not override the container's position.
- **PerformanceTracker toggle:** top-right, above the HUD column and 11.3 px to its left (`Button` scale 0.252 at −63.95, −30.37, top 24 px). When open, the FPS panel (`DataView`, pivot top-right, at −84.91, −17.78) is 11.3 px to the left of the toggle, with the same top.
- **Camera and pin columns:** `ViewsContainer` and `PinContainer` scale 0.840, at (9.39, −57.78) and (47.16, −57.78). That gives a 24 px left margin, a 17 px gap between the columns, and the camera preview (`ViewManager.prefab` → `InterestPointPreviewUI`) 27 px to the right of the pin list. With no pin list, `NoPinListPreviewOffsetX` = −30 puts it 24 px after the camera list.
- **Close button:** a 20×20 unit button whose `Icon` child is a stretched RawImage with `ph-x`, an inset of −4 (20 % of the button size), raycast off, and a `ThemedIcon` with role Icon. Set the button's scale so the result is 20 px.

## Text

| Role | Size at 1080p |
|---|---|
| Body, labels, values, buttons | **≈ 14 px** |
| Modal titles | **≈ 18 px** |
| Small (inputs, counters) | 10–13 px |

TMP font size = target px ÷ (accumulated scale × canvas factor).

## Modals

Each modal's text is sized by scaling its top-level container. The canvas keeps its reference resolution, and the corner panels pivot on their own corner, so they shrink in place.

| Modal | Canvas | Container scale | Body / title |
|---|---|---|---|
| Sun (`SunIRL UI`) | 1920×1080 | 0.7 | 12.6 / 14 px |
| PerformanceTracker (`DataView`) | 1000×800 | 0.8 | 13.7 px |
| ProjectSettingsUI | 800×600 | 0.56 | 14 / 18 px |
| PinInfoUI | 800×600 | 0.71 | 14 / 18 px |
| PinContextUI | 800×600 | 0.7 | 15 px (description 10.8 px) |
| ViewPointContextUI | 800×600 | 0.56 | 14 / 16 px |
| EditorHUD MapUI | 1000×800 | 0.69 | 14 / 18 px |
| MapManagement MapUI | 1000×800 | 0.58 | 14 px |
| MapVarAdjustUI | 800×600 | 0.341 | 13–15 px |
| EditorObjectInfoUI | 800×600 | 0.6 | 13–14 / 18 px |
| FPS_UI player settings | 800×600 | 0.52 | 12–15 px |
| FPS_UI movement hint | 800×600 | 0.6 | 12–15 px |
| PopUpAreYouSure | 800×600 | 0.62 | 14 / 18 px |
| PopUpInput, PopUpMessage | 1000×800 | 0.86 | 14 / 17 px |
| ProjectExportUI | 1000×800 | 0.79 | 13–16 px |

### Geolocalization screen (`GeoLocalizationUI`, 1000×800)

Screen margins are 24 px on every side (17.76 units for the search bar and exit button).

| Element | Size | How |
|---|---|---|
| Top-right buttons (upload, place map) | 34 px | `ButtonContainer` scale 0.3439 at (−30.37, −15.34), column centered on the zoom slider (`Zooming` x −30.37) |
| Exit button (bottom left) | 34 px | `Leave button` scale 0.787 |
| Search bar | text 14 px, bar ≈ 400×26 px, search button 31 px | `Searching` scale 0.315 |
| Info text | 14 px | scale 0.288 |
| Opacity and zoom icons | 20 px | scale 0.74 (opacity), 0.617 (zoom) |
| Opacity and zoom sliders | from 133 px below the top to 155 px above the bottom | so they clear the "Viditelnost pinů" and lock toggles in the bottom-right corner |

## Color and interaction

- **Accent color:** IconTheme purple `#6A58A7`. It's used for icons, the "on" state of every `UISwitcher` toggle, slider handles and fills, and the upload progress bar.
- **Hover:**
  - White icon buttons (`MainButton`, `ViewButton`) tint to `#EDE9FA` on hover and `#D6CCF0` when pressed.
  - Close buttons stay invisible until hovered, then show a 14 % purple rounded square, darker when pressed.
- **Text buttons** (`PopUpAreYouSureUI`, `PopUpInputUI`): bold labels, ≈ 24 px tall, Image white with the color set by the Button's Color Tint.
  - Primary (the action: "Ano", "Uložit a odejít", "Pokračovat"): `#6A58A7` with white text, hover `#5A4A93`, pressed `#4B3D7D`.
  - Secondary ("Zrušit", "Odejít"): `#EDE9FA` with `#6A58A7` text, hover `#D6CCF0`, pressed `#C5B8E8`.
- **Cursor:** a hand over every interactable `Selectable` (buttons, toggles, sliders including the track, dropdowns, scrollbars). Text inputs keep the normal cursor.
- **Tooltips:** every icon-only button has a short Czech `TooltipTrigger`, such as "Uložit", "Zpět" or "Smazat pin". The `TEMPLabelBackground` inside `MainButton` is off by default.
- **Labels over the 3D scene or map** (toggle labels): white text with the `LiberationSans SDF - Soft Shadow` material (`_Features/Utilities/UI/Fonts`), a soft dark halo that keeps them readable over light sky and map.

## Checking sizes

When Unity isn't open, these sizes can be calculated from the prefab YAML: follow `m_LocalScale`, `m_SizeDelta` and the anchors up to the canvas, including nested prefab instances, then multiply by the canvas factor. The HUD's right column measured this way (then 26 px) matched a 1920×1080 Game view screenshot exactly.
