# Step 5: Launcher 场景及 Launcher 实现

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

### Launcher 场景的职责

Launcher 场景是应用程序的**入口场景**。  
Unity 启动时最先加载，是 VContainer 生成 `RootLifetimeScope` 的起点。  
场景本身不放置游戏逻辑，尽量保持空场景。

```
Build Settings:
  0: Launcher      ← 启动时最先加载的场景
  1: （各游戏场景）
```

### ILauncher / Launcher 的职责

`ILauncher` 是负责应用首次启动和重启（Reboot）的接口。

```csharp
public interface ILauncher
{
    UniTask Launch();   // 首次启动
    void Reboot();      // 重启应用（返回 Launcher 场景并重新执行）
}
```

将 `Launcher` 注册到 `ProductLifetimeScope`，即可从 `ProductEntryPoint` 调用。

```csharp
builder.Register<Launcher>(Lifetime.Singleton).AsImplementedInterfaces();
```

### 实现示例

```csharp
public sealed class Launcher : ILauncher
{
    static readonly string LauncherSceneName = "Launcher";

    readonly ISceneManager sceneManager;

    [Inject]
    public Launcher(ISceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
    }

    async UniTask ILauncher.Launch()
    {
        await FirstLaunchProcess();  // 仅首次启动时需要的处理（构建缓存等）
        await LaunchProcess();       // 每次启动时需要的处理（加载主数据等）
        TransitionNextScene();       // 跳转到第一个游戏场景
    }

    void ILauncher.Reboot()
    {
        RebootProcess().Forget();

        async UniTask RebootProcess()
        {
            await sceneManager.PreReboot();
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                LauncherSceneName, LoadSceneMode.Single);
            await LaunchProcess();
            TransitionNextScene();
        }
    }

    UniTask FirstLaunchProcess() => UniTask.CompletedTask;  // 按需实现
    UniTask LaunchProcess() => UniTask.CompletedTask;       // 按需实现

    void TransitionNextScene()
    {
        UniTask.Void(async () =>
        {
            await sceneManager.TransitionScene(new SplashTransitionData());

            // 跳转完成后卸载 Launcher 场景
            if (!string.IsNullOrEmpty(
                    UnityEngine.SceneManagement.SceneManager.GetSceneByName(LauncherSceneName).name))
            {
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(LauncherSceneName);
            }
        });
    }
}
```

### 完整启动流程

```
Unity 启动
  └── 加载 Launcher 场景
        └── VContainerSettings 自动生成 RootLifetimeScope Prefab
              └── RootEntryPoint.Start()
                    └── 将 ProductLifetimeScope Prefab 作为子作用域生成
                          └── ProductEntryPoint.StartAsync()
                                ├── 向场景管理器注册父作用域
                                ├── 语言初始化
                                └── Launcher.Launch()
                                      ├── 首次启动处理
                                      ├── 启动处理
                                      └── 跳转到第一个场景 → 卸载 Launcher 场景
```
