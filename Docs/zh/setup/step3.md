# Step 3: TextTable 设置

## 设置指南 — 目录

- [Step 1: 生成设置资产](./step1.md)
- [Step 2: ScreenStack 设置](./step2.md)
- [Step 3: TextTable 设置](./step3.md)
- [Step 4: VContainer LifetimeScope 实现](./step4.md)
- [Step 5: Launcher 场景及 Launcher 实现](./step5.md)
- [Step 6: 创建场景文件并注册到 Build Settings](./step6.md)
- [Step 7: 生成场景脚本](./step7.md)
- [Step 8: 场景 LifetimeScope 及基类实现](./step8.md)

---

> 不使用 TextTable 时，可跳过此步骤。

`TextTableEditorSettings` 在打开 TextTable 编辑器窗口时自动生成。

从 Unity 菜单选择 **Lighthouse > TextTable > ...** 打开窗口后，  
`Assets/Settings/Lighthouse/TextTableEditorSettings.asset` 将自动生成。

在 Inspector 中确认并修改以下字段：

| 字段 | 说明 | 默认值 |
|---|---|---|
| Text Table Folder Path | TSV 文件的存放路径 | `Assets/StreamingAssets/TextTables` |
