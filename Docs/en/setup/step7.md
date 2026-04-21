# Step 7: Generate Scene Scripts

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

Lighthouse includes an editor window for generating scene boilerplate.

Open **Lighthouse > Generate > Open Scene scripts generator**, configure the following fields, and press **Generate**.

| Field | Description |
|---|---|
| Scene Type | `MainScene` or `ModuleScene` |
| Template Preset | Template registered in `GenerateSettings` |
| Base Class | The base class to inherit (e.g. `CanvasMainSceneBase`) |
| Scene Name | The scene name (e.g. `Home`) |

Scene scripts are generated in the folder specified by **Scene Script Output Directory** in `GenerateSettings`.

> By customizing templates, you can generate scripts that inherit a product-wide common base class.  
> See **Scene Script Templates** in `GenerateSettings` for details.
