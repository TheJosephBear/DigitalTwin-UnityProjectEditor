# Testing model uploads

The model upload tests load real 3D models the way the app does when someone uploads files in the upload dialog. They check that each model came out right, take two screenshots of it, and measure how much memory the load used. The results end up in a report you can open in the browser.

They are Unity **play mode tests** in `Assets/Tests/PlayMode/`.

## What a test does

For each model:

1. **Upload.** It hands the files to `AssetManager` the way `ModelUploadManager` does: `PrepareAssetFromFiles`, one frame, then `BuildPreparedAssetCoroutine`. TriLib parses the model in one frame, then builds its meshes, materials and textures over the following frames.
2. **Check.** A model passes when all of these hold:
   - it loaded, and nothing logged an error
   - it has renderers and vertices
   - its bounds are finite and not zero
   - it is at most 50 km across (anything bigger means the coordinates were misread, usually decimal commas)
   - when the selection has textures the model uses, at least one ends up on a material (TriLib's own 16×16 placeholders don't count)
3. **Record.** It counts vertices, triangles, materials and textures, times the load, measures memory, and takes an angled and a top screenshot.
4. **Clean up.** It destroys the model and deletes the files it copied into `Application.persistentDataPath`.

## Before the first run

### The model files

The test models are too big for git, so they live in folders outside the project. The list of models is in [`TestModelCatalog.cs`](Assets/Tests/PlayMode/TestModelCatalog.cs), and the folders it uses are in [`TestModelFolders.cs`](Assets/Tests/PlayMode/TestModelFolders.cs).

> **Note:** `TestModelCatalog.cs` lists the models on the computer the tests were written on. To test your own models, rewrite it: replace the entries in `Standard()` and `Heavy()` with your models (see [Adding a model](#adding-a-model)), and change the folders in `TestModelFolders.cs` to match.

Get a copy of the model folders, or put your own models in folders of your choice. Then point the tests at them in either of these ways:

- **In the editor:** **Tools > Model Upload Tests > Set Model Folders...** opens a folder picker for each folder. The choice is saved on your computer only.
- **With environment variables:** each folder has one, named in `TestModelFolders.cs`. They win over the editor setting, which makes them handy for command-line runs.

**Tools > Model Upload Tests > Show Model Folders** shows which folders are in use and whether they were found.

A model whose files aren't found is **skipped, not failed**. If every test shows as skipped (yellow), the folders aren't set.

### OBJ files with decimal commas

An OBJ exported on a PC set to a comma locale (Czech, for example) has numbers like `v 181,1149 …`. TriLib reads the comma as a thousands separator, so the model comes out far too big and partly distorted. To test such a model as it should look, make a corrected copy:

```powershell
python Tools/fix_obj_decimal_commas.py "<folder>\model.obj" --out "<folder>\fixed_decimal_dots"
```

The script fixes the OBJ and the MTL next to it (or the one given with `--mtl`). It only changes commas between digits on number lines. The original files stay as they are.

## Running the tests

### In the editor

1. Open **Window > General > Test Runner** and go to the **PlayMode** tab.
2. Expand **ModelUploadTests**. It has two groups:

   | Group | Models | How to run |
   |---|---|---|
   | `UploadModel` | standard models, up to about 70 MB each | **Run All** |
   | `UploadHeavyModel` | big models, 70 MB and more | select them, then **Run Selected** |

   The heavy group is marked to run only on purpose, so **Run All** leaves it out. Heavy models take minutes each and need a lot of RAM, so close other big programs first.
3. When the run finishes, open the report with **Tools > Model Upload Tests > Open Latest Report**.

You can run a single model too: select it under its group and click **Run Selected**.

### From the command line

Close the project in the editor first, because Unity can't open it twice. This hasn't been tried yet, so expect to adjust it:

```powershell
& "<Unity install folder>\Editor\Unity.exe" -batchmode -projectPath "<project folder>" -runTests -testPlatform PlayMode -testFilter "ModelUploadTests.UploadModel" -testResults "TestResults\results.xml"
```

- For the heavy group, use `-testFilter "ModelUploadTests.UploadHeavyModel"`. Naming the group in the filter is what makes the heavy tests run.
- Don't add `-nographics`, or the screenshots can't be taken.
- Set the environment variables first if you haven't set the model folders in the editor.

## The report

Each run writes a new folder: `TestResults/ModelUpload/<date>_<time>_<commit>/`. The commit comes from git and gets `+changes` when tracked files had uncommitted changes. Without git in the PATH it says `unknown`. `TestResults/` is ignored by git.

| File | What it is |
|---|---|
| `report.html` | The table, both screenshots of each model, and its warnings and errors. Open it from **Tools > Model Upload Tests > Open Latest Report**. |
| `report.md` | The same in Markdown. |
| `memory.csv` | The memory figures, one row per model. |
| `*_iso.png`, `*_top.png` | The screenshots. |

**Open Results Folder** in the same menu opens the `TestResults/ModelUpload` folder.

### Table columns

| Column | Meaning |
|---|---|
| Result | `loaded` or `FAILED`. A model can load and still fail a check. The test result says why. |
| Input MB | Total size of the picked files |
| Prepare s | Time to copy the files into the app's storage |
| Build s | Time TriLib took to build the model |
| Mem +MB | How much more memory was in use after the load than before it |
| Renderers, Vertices, Triangles | What the model is made of |
| Materials, Textured mats, Textures | Materials, the materials that have a texture, and the model's own textures |
| Size | The model's bounding box in metres. Check that it's realistic. |
| Warnings, Errors | Messages logged during the load. Errors fail the test. |

### `memory.csv`

All figures are in MB (1 MB = 1,048,576 bytes):

| Column | Meaning |
|---|---|
| `parse_allocated_mb` | Managed memory allocated during the build's busiest frame, usually the one where TriLib parses the file. On WebGL the garbage collector only runs between frames, so all of it is in memory at once. This is the main part of the **peak**. |
| `managed_retained_mb` | Managed memory the loaded model still holds after a garbage collection |
| `native_retained_mb` | Native memory (meshes, textures) still held after the load |
| `input_mb`, `vertices`, `triangles`, `textures`, `build_s` | The same as in the table |

The figures are measured in the 64-bit editor. In the 32-bit browser they are likely a little lower.

## Expected failures

Some models are in the list because they show a real problem with the file or with TriLib's settings, so their tests fail on purpose. Each one's note in `TestModelCatalog.cs` says why, and the note also appears in the report. Any failure the note doesn't explain is new and worth looking at.

## Adding a model

Add a line to `Standard()` (up to about 70 MB) or `Heavy()` in [`TestModelCatalog.cs`](Assets/Tests/PlayMode/TestModelCatalog.cs), using the helper for the folder the model is in:

```csharp
yield return DT("My_model_obj_mtl", "What's special about it.", true,
    @"some folder\model.obj", @"some folder\model.mtl", @"some folder\textures\*.png");
```

- The **name** is shown in the Test Runner and the report. Use letters, digits and underscores.
- The **note** shows up in the report. If the test is expected to fail, say why here.
- The **`true`/`false`** says whether the selection has textures the model uses. When it's `true`, at least one must end up on a material.
- The **files** are what a user would pick in the upload dialog. Paths are relative to the model folder. The main model file goes first, and `*` and `?` work in file names.

Put the model files in the shared model folders, so everyone else can run the new test too.

## Troubleshooting

| Problem | What to do |
|---|---|
| Every test is skipped | Set the model folders (**Show Model Folders** says which ones aren't found). |
| `Missing …/FileLoadingManager.prefab` | The prefab moved. Update `FileLoadingManagerPrefabPath` in `ModelUploadTests.cs`. |
| A test fails on a TriLib error without saying why | TriLib doesn't pass on its errors. Look at the model's warnings and errors in `report.html` and in the Console. |
| Unity runs out of memory or becomes slow during the heavy group | Run the heavy models one at a time. |
