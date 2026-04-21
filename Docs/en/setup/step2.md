# Step 2: ScreenStack Settings

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

> Skip this step if you are not using ScreenStack.

Select **Lighthouse > Settings > ScreenStackGenerateSettings** from the Unity menu.

`Assets/Settings/Lighthouse/ScreenStackGenerateSettings.asset` will be created.

Configure the following fields in the Inspector:

| Field | Description |
|---|---|
| Screen Stack Entity Factory Directory | Output folder for `ScreenStackEntityFactory` |
| Screen Stack Entity Factory Class Name | Generated class name (default: `ScreenStackEntityFactory`) |
| Screen Stack Entity Factory Namespace | Namespace for the generated factory class |
| Screen Stack Entity Factory Template | Template asset for factory generation |
| Screen Stack Dialog Script Output Directory | Output folder for dialog scripts |
| Screen Stack Dialog Script Namespace | Namespace for generated dialog scripts |
| Screen Stack Script Templates | Template assets for dialog script generation |
