# Step 6: Create Scene Files and Register to Build Settings

## Setup Guide — Table of Contents

- [Step 1: Generate Settings Assets](./step1.md)
- [Step 2: ScreenStack Setup](./step2.md)
- [Step 3: TextTable Setup](./step3.md)
- [Step 4: VContainer LifetimeScope Implementation](./step4.md)
- [Step 5: Launcher Scene and Launcher Implementation](./step5.md)
- [Step 6: Create Scene Files and Register to Build Settings](./step6.md)
- [Step 7: Generate Scene Scripts](./step7.md)
- [Step 8: Scene LifetimeScope and Base Class Implementation](./step8.md)

---

### Scene File Naming Convention

Lighthouse's `SceneIdGenerator` determines the scene type by keywords contained in the scene paths registered in Build Settings.

| Type | Required keyword in path | Example |
|---|---|---|
| Main scene | `MainScene` | `Assets/.../Scene/MainScene/Home/Home.unity` |
| Module scene | `ModuleScene` | `Assets/.../Scene/ModuleScene/Background/Background.unity` |

> Ensure that the scene file name or one of its parent folder names contains `MainScene` / `ModuleScene`.

### Registering to Build Settings

1. Open **File > Build Settings** from the Unity menu
2. Drag and drop scene files into the **Scenes In Build** list
3. Place the `Launcher` scene at **index 0**

```
Scenes In Build:
  0: Assets/.../Scene/Launcher.unity          ← must be first
  1: Assets/.../Scene/MainScene/Splash/Splash.unity
  2: Assets/.../Scene/MainScene/Home/Home.unity
  3: Assets/.../Scene/ModuleScene/Background/Background.unity
  ...
```

> When Build Settings change, `SceneIdGenerator` automatically regenerates the `MainSceneId` / `ModuleSceneId` classes.

### Manual Generation

If automatic generation does not trigger, you can run it manually from the following menus:

- **Lighthouse > Generate > Auto > Generate "MainSceneId" manually**
- **Lighthouse > Generate > Auto > Generate "ModuleSceneId" manually**
