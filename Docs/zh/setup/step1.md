# Step 1: 生成设置资产

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

### 1-1. 生成 GenerateSettings

从 Unity 菜单选择 **Lighthouse > Settings > GenerateSettings**。

将生成 `Assets/Settings/Lighthouse/GenerateSettings.asset`。

在 Inspector 中配置以下字段：

| 字段 | 说明 |
|---|---|
| Product Name Space | 生成类的根命名空间（例：`SampleProduct`） |
| Generated File Directory | 生成文件的输出文件夹（指定下文说明的 `LighthouseGenerated` 文件夹） |
| Main Scene Id Prefix | MainSceneId 类名的前缀（例：`Sample`） |
| Module Scene Id Prefix | ModuleSceneId 类名的前缀（例：`Sample`） |
| Scene Script Output Directory | 场景脚本的输出文件夹 |
| Scene Script Templates | 场景脚本生成使用的模板资产 |

### 1-2. 创建 LighthouseGenerated 文件夹

在项目 Assets 目录下创建 `LighthouseGenerated` 文件夹。

```
Assets/
└── {YourProductFolder}/
    └── LighthouseGenerated/   ← 在此创建
```

创建后，将该文件夹拖拽到 `GenerateSettings` 的 **Generated File Directory** 字段中。

### 1-3. 生成 SceneEditSettings

从 Unity 菜单选择 **Lighthouse > Settings > SceneEditSettings**。

将生成 `Assets/Settings/Lighthouse/SceneEditSettings.asset`。

> **说明：** `SceneEditSettings` 也可能在编辑器启动时自动生成。  
> 如果文件已存在，则不会被覆盖。

在 Inspector 中确认并配置以下字段：

| 字段 | 说明 | 默认值 |
|---|---|---|
| Enable Scene Edit Process | 场景编辑时自动更新的启用/禁用 | `true` |
| Canvas Scene Editor Only Object | Canvas 场景编辑用编辑器专用对象的 Prefab（见下文） | 无 |
| Editor Only Object Name | 放置在场景中的对象名称 | `__EditorOnly__` |

### 1-4. 创建并设置 CanvasSceneEditorOnlyObject Prefab

创建一个 Prefab 用于辅助在编辑器中编辑场景时的 Canvas 行为。

**创建 Prefab 的步骤：**

1. 在任意文件夹中创建空 GameObject，并将其制作为 Prefab
2. 为 Prefab 添加以下两个组件：
   - `LHCanvasSceneObject` — 持有 SceneCamera 和 EventSystem 的组件
   - `DefaultCanvasSceneEditorOnlyObject` — `IEditorOnlyObjectCanvasScene` 的标准实现
3. 将同一 GameObject 上的 `LHCanvasSceneObject` 设置到 `DefaultCanvasSceneEditorOnlyObject` 的 **Lh Canvas Scene Object** 字段
4. 为 `LHCanvasSceneObject` 的 **UI Camera** 和 **UI Event System** 字段设置适当的组件

**注册到 SceneEditSettings：**

将创建的 Prefab 设置到 `SceneEditSettings` 的 **Canvas Scene Editor Only Object** 字段。

> 该 Prefab 会在编辑器中打开实现了 `ICanvasSceneBase` 的场景时自动放置到场景中，并在播放或保存时自动移除。
