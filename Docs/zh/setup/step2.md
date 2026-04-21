# Step 2: ScreenStack 设置

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

> 不使用 ScreenStack 时，可跳过此步骤。

从 Unity 菜单选择 **Lighthouse > Settings > ScreenStackGenerateSettings**。

将生成 `Assets/Settings/Lighthouse/ScreenStackGenerateSettings.asset`。

在 Inspector 中配置以下字段：

| 字段 | 说明 |
|---|---|
| Screen Stack Entity Factory Directory | `ScreenStackEntityFactory` 的输出文件夹 |
| Screen Stack Entity Factory Class Name | 生成类名（默认：`ScreenStackEntityFactory`） |
| Screen Stack Entity Factory Namespace | 生成工厂类的命名空间 |
| Screen Stack Entity Factory Template | 工厂生成用模板资产 |
| Screen Stack Dialog Script Output Directory | 对话框脚本的输出文件夹 |
| Screen Stack Dialog Script Namespace | 生成对话框脚本的命名空间 |
| Screen Stack Script Templates | 对话框脚本生成用模板资产 |
