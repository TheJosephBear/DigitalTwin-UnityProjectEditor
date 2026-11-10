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

## Icons

Icon color comes from `Assets/Resources/IconTheme.asset` through `ThemedIcon`: purple `#6A58A7` for icons, white for icons on dark backgrounds. Icons are Phosphor Regular (`Assets/Media/Sprites/Phosphor`).

| Element | Button | Icon | Used for |
|---|---|---|---|
| Square icon button | **26 px** | 22 px (84 %) | HUD right column, PerformanceTracker toggle, sun button, action buttons in PinContext and ViewPointContext |
| Round icon button | **28 px** | 20–21 px (74 %) | Cameras (ViewPointUI) and pins (PinUI) on the left |
| Close button (X) | **20 px** | 16 px (80 %) | Top-right corner of every modal |

How to reproduce them:

- **Square button:** the `MainButton` prefab is 100×100 units. Use scale **0.193** on a 1000×800 canvas, or 0.144 on 800×600. Inside a scaled modal container, divide by the container's scale.
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

Screen margins are 20.5 units (28 px) on the left, top and bottom.

| Element | Size | How |
|---|---|---|
| Top-right buttons (upload, place map) | 26 px | `ButtonContainer` scale 0.2597, column centered on the zoom slider (x −37) |
| Exit button (bottom left) | 26 px | `Leave button` scale 0.605 |
| Search bar | text 14 px, bar ≈ 400×26 px, search button 31 px | `Searching` scale 0.315 |
| Info text | 14 px | scale 0.288 |
| Opacity and zoom icons | 20 px | scale 0.74 (opacity), 0.617 (zoom) |
| Opacity and zoom sliders | from 133 px below the top to 155 px above the bottom | so they clear the "Viditelnost pinů" and lock toggles in the bottom-right corner |

## Color and interaction

- **Accent color:** IconTheme purple `#6A58A7`. It's used for icons, the "on" state of every `UISwitcher` toggle, slider handles and fills, and the upload progress bar.
- **Hover:**
  - White icon buttons (`MainButton`, `ViewButton`) tint to `#EDE9FA` on hover and `#D6CCF0` when pressed.
  - Close buttons stay invisible until hovered, then show a 14 % purple rounded square, darker when pressed.
- **Text buttons** (`PopUpAreYouSureUI`): bold labels, ≈ 24 px tall, Image white with the color set by the Button's Color Tint.
  - Primary (the action: "Ano", "Uložit a odejít"): `#6A58A7` with white text, hover `#5A4A93`, pressed `#4B3D7D`.
  - Secondary ("Zrušit", "Odejít"): `#EDE9FA` with `#6A58A7` text, hover `#D6CCF0`, pressed `#C5B8E8`.
- **Cursor:** a hand over every interactable `Selectable` (buttons, toggles, sliders including the track, dropdowns, scrollbars). Text inputs keep the normal cursor.
- **Tooltips:** every icon-only button has a short Czech `TooltipTrigger`, such as "Uložit", "Zpět" or "Smazat pin". The `TEMPLabelBackground` inside `MainButton` is off by default.
- **Labels over the 3D scene or map** (toggle labels): white text with the `LiberationSans SDF - Soft Shadow` material (`_Features/Utilities/UI/Fonts`), a soft dark halo that keeps them readable over light sky and map.

## Checking sizes

When Unity isn't open, these sizes can be calculated from the prefab YAML: follow `m_LocalScale`, `m_SizeDelta` and the anchors up to the canvas, including nested prefab instances, then multiply by the canvas factor. The HUD's right column measured this way (26 px) matches a 1920×1080 Game view screenshot exactly.
