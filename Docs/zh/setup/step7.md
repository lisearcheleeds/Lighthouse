# Step 7: 生成场景脚本

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

Lighthouse 附带了用于生成场景样板代码的编辑器窗口。

打开 **Lighthouse > Generate > Open Scene scripts generator**，配置以下字段后按 **Generate**。

| 字段 | 说明 |
|---|---|
| Scene Type | `MainScene` 或 `ModuleScene` |
| Template Preset | 在 `GenerateSettings` 中注册的模板 |
| Base Class | 要继承的基类（例：`CanvasMainSceneBase`） |
| Scene Name | 场景名称（例：`Home`） |

场景脚本将生成到 `GenerateSettings` 的 **Scene Script Output Directory** 所指定的文件夹中。

> 通过自定义模板，可以生成继承产品公共基类的脚本。  
> 详情请参阅 `GenerateSettings` 的 **Scene Script Templates**。
