# Lighthouse — Setup Guide

After adding the packages to your Unity project, follow the steps below to complete the initial configuration.

## Starter Kit (UnityPackage)

A UnityPackage that lets you skip most of the setup steps is available from [Releases](https://github.com/lisearcheleeds/Lighthouse/releases).

### Steps you can skip

| Step | Skip | Notes |
|---|---|---|
| Step 1: Generate settings assets | Mostly skippable | You still need to set `ProductNameSpace`, `MainSceneIdPrefix`, `ModuleSceneIdPrefix`, and output directories for your project |
| Step 2: ScreenStack settings | Mostly skippable | Only namespace and output directory changes required |
| Step 3: TextTable settings | Fully skippable | No changes needed if using the default path |
| Step 4: VContainer LifetimeScope implementation | Fully skippable | Scripts, Prefabs, ScriptableObjects, and VContainerSettings all provided |
| Step 5: Launcher scene and Launcher implementation | Fully skippable | Launcher.unity and Launcher.cs provided |
| Step 6: Register scenes in Build Settings | Required | Build Settings must always be configured manually |
| Step 7: Generate scene scripts | Required | Game-specific work |
| Step 8: LifetimeScope and base class implementation | Required | Common base classes provided; per-scene LifetimeScope and SceneGroupProvider customization still needed |

---

## Table of Contents

- [Step 1: Generate settings assets](./setup/step1.md)
- [Step 2: ScreenStack settings](./setup/step2.md)
- [Step 3: TextTable settings](./setup/step3.md)
- [Step 4: VContainer LifetimeScope implementation](./setup/step4.md)
- [Step 5: Launcher scene and Launcher implementation](./setup/step5.md)
- [Step 6: Create scene files and register in Build Settings](./setup/step6.md)
- [Step 7: Generate scene scripts](./setup/step7.md)
- [Step 8: Scene LifetimeScope and base class implementation](./setup/step8.md)
