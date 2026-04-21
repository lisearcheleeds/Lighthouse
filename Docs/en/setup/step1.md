# Step 1: Generate Settings Assets

## Setup Guide — Table of Contents

- [Step 1: Generate settings assets](./step1.md)
- [Step 2: ScreenStack settings](./step2.md)
- [Step 3: TextTable settings](./step3.md)
- [Step 4: VContainer LifetimeScope implementation](./step4.md)
- [Step 5: Launcher scene and Launcher implementation](./step5.md)
- [Step 6: Create scene files and register in Build Settings](./step6.md)
- [Step 7: Generate scene scripts](./step7.md)
- [Step 8: Scene LifetimeScope and base class implementation](./step8.md)

---

### 1-1. Generate GenerateSettings

Select **Lighthouse > Settings > GenerateSettings** from the Unity menu.

`Assets/Settings/Lighthouse/GenerateSettings.asset` will be created.

Configure the following fields in the Inspector:

| Field | Description |
|---|---|
| Product Name Space | Root namespace for generated classes (e.g. `SampleProduct`) |
| Generated File Directory | Output folder for generated files (assign the `LighthouseGenerated` folder described below) |
| Main Scene Id Prefix | Prefix for the MainSceneId class name (e.g. `Sample`) |
| Module Scene Id Prefix | Prefix for the ModuleSceneId class name (e.g. `Sample`) |
| Scene Script Output Directory | Output folder for generated scene scripts |
| Scene Script Templates | Template assets used for scene script generation |

### 1-2. Create the LighthouseGenerated Folder

Create a `LighthouseGenerated` folder inside your product's Assets folder.

```
Assets/
└── {YourProductFolder}/
    └── LighthouseGenerated/   ← create here
```

Then drag and drop this folder into the **Generated File Directory** field of `GenerateSettings`.

### 1-3. Generate SceneEditSettings

Select **Lighthouse > Settings > SceneEditSettings** from the Unity menu.

`Assets/Settings/Lighthouse/SceneEditSettings.asset` will be created.

> **Note:** `SceneEditSettings` may also be auto-created on editor startup.  
> If the file already exists, it will not be overwritten.

Confirm or configure the following fields in the Inspector:

| Field | Description | Default |
|---|---|---|
| Enable Scene Edit Process | Enable/disable automatic updates during scene editing | `true` |
| Canvas Scene Editor Only Object | Editor-only object Prefab for Canvas scene editing (see below) | none |
| Editor Only Object Name | Name of the object placed in the scene | `__EditorOnly__` |

### 1-4. Create and Assign the CanvasSceneEditorOnlyObject Prefab

Create a Prefab to assist Canvas behavior when editing scenes in the editor.

**Steps to create the Prefab:**

1. Create an empty GameObject in any folder and make it a Prefab
2. Attach the following two components to the Prefab:
   - `LHCanvasSceneObject` — holds the SceneCamera and EventSystem
   - `DefaultCanvasSceneEditorOnlyObject` — the standard implementation of `IEditorOnlyObjectCanvasScene`
3. Assign the `LHCanvasSceneObject` component on the same GameObject to the **Lh Canvas Scene Object** field of `DefaultCanvasSceneEditorOnlyObject`
4. Assign the appropriate components to the **UI Camera** and **UI Event System** fields of `LHCanvasSceneObject`

**Register in SceneEditSettings:**

Assign the created Prefab to the **Canvas Scene Editor Only Object** field of `SceneEditSettings`.

> This Prefab is automatically placed in the scene when a scene implementing `ICanvasSceneBase` is opened in the editor, and is automatically removed on Play or Save.
