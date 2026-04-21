# Lighthouse — 设置指南

将包添加到 Unity 项目后，请按照以下步骤完成初始配置。

## 快速入门套件（UnityPackage）

提供可跳过大部分设置步骤的 UnityPackage。  
[TODO: UnityPackage URL]

### 可跳过的步骤

| 步骤 | 跳过 | 备注 |
|---|---|---|
| Step 1: 生成设置资产 | 基本可跳过 | 仍需将 `ProductNameSpace`·`MainSceneIdPrefix`·`ModuleSceneIdPrefix`·输出目录更改为适合项目的值 |
| Step 2: ScreenStack 设置 | 基本可跳过 | 仅需修改命名空间和输出目录 |
| Step 3: TextTable 设置 | 完全可跳过 | 使用默认路径时无需修改 |
| Step 4: VContainer LifetimeScope 实现 | 完全可跳过 | 脚本·Prefab·ScriptableObject·VContainerSettings 均已提供 |
| Step 5: Launcher 场景及 Launcher 实现 | 完全可跳过 | 已提供 Launcher.unity 及 Launcher.cs |
| Step 6: 注册到 Build Settings | 必要 | Build Settings 始终需要手动配置 |
| Step 7: 生成场景脚本 | 必要 | 游戏专属工作 |
| Step 8: LifetimeScope 及基类实现 | 必要 | 已提供通用基类；仍需为每个场景实现 LifetimeScope 并自定义 SceneGroupProvider |

---

## 目录

- [Step 1: 生成设置资产](./setup/step1.md)
- [Step 2: ScreenStack 设置](./setup/step2.md)
- [Step 3: TextTable 设置](./setup/step3.md)
- [Step 4: VContainer LifetimeScope 实现](./setup/step4.md)
- [Step 5: Launcher 场景及 Launcher 实现](./setup/step5.md)
- [Step 6: 创建场景文件并注册到 Build Settings](./setup/step6.md)
- [Step 7: 生成场景脚本](./setup/step7.md)
- [Step 8: 场景 LifetimeScope 及基类实现](./setup/step8.md)
