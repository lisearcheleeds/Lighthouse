# Step 6: 创建场景文件并注册到 Build Settings

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

### 场景文件命名规则

Lighthouse 的 `SceneIdGenerator` 通过 Build Settings 中注册的场景路径所含关键字来判断场景类型。

| 类型 | 路径中须包含的关键字 | 示例 |
|---|---|---|
| 主场景 | `MainScene` | `Assets/.../Scene/MainScene/Home/Home.unity` |
| 模块场景 | `ModuleScene` | `Assets/.../Scene/ModuleScene/Background/Background.unity` |

> 请确保场景文件名或其父文件夹名称中包含 `MainScene` / `ModuleScene`。

### 注册到 Build Settings

1. 从 Unity 菜单打开 **File > Build Settings**
2. 将场景文件拖拽到 **Scenes In Build** 列表中
3. 将 `Launcher` 场景放置在 **index 0**

```
Scenes In Build:
  0: Assets/.../Scene/Launcher.unity          ← 必须排在第一位
  1: Assets/.../Scene/MainScene/Splash/Splash.unity
  2: Assets/.../Scene/MainScene/Home/Home.unity
  3: Assets/.../Scene/ModuleScene/Background/Background.unity
  ...
```

> 每当 Build Settings 发生变更，`SceneIdGenerator` 将自动重新生成 `MainSceneId` / `ModuleSceneId` 类。

### 手动生成

若自动生成未触发，可从以下菜单手动执行：

- **Lighthouse > Generate > Auto > Generate "MainSceneId" manually**
- **Lighthouse > Generate > Auto > Generate "ModuleSceneId" manually**
