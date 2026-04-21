# Step 3: TextTable Settings

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

> Skip this step if you are not using TextTable.

`TextTableEditorSettings` is auto-created when the TextTable editor window is opened.

Open the window via **Lighthouse > TextTable > ...** from the Unity menu.  
`Assets/Settings/Lighthouse/TextTableEditorSettings.asset` will be created automatically.

Confirm or update the following field in the Inspector:

| Field | Description | Default |
|---|---|---|
| Text Table Folder Path | Path where TSV files are stored | `Assets/StreamingAssets/TextTables` |
