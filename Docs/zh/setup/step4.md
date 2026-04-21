# Step 4: VContainer LifetimeScope 实现

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

Lighthouse 的场景系统与 VContainer DI 作用域协同工作。  
准备以下脚本、Prefab 和 ScriptableObject 后，即可配置应用程序的启动流程。

> VContainer 基本用法: **[TODO: VContainer 文档 URL]**  
> Unity 项目初始设置辅助: **[TODO: Preset startup unity package URL]**

### 推荐文件结构

推荐以下目录布局。

```
Assets/
└── {YourProduct}/
    └── Runtime/
        ├── Scripts/
        │   └── Core/
        │       ├── RootLifetimeScope.cs
        │       ├── RootLifetimeScopeSettings.cs
        │       ├── RootEntryPoint.cs
        │       ├── ProductLifetimeScope.cs
        │       ├── ProductLifetimeScopeSettings.cs
        │       ├── ProductEntryPoint.cs
        │       └── SceneGroupProvider.cs
        └── StaticResources/
            └── LifetimeScope/
                ├── RootLifetimeScope.prefab
                ├── RootLifetimeScopeData.asset
                ├── ProductLifetimeScope.prefab
                ├── ProductLifetimeScopeSettings.asset
                ├── VContainerSettings.asset
                ├── SupportedLanguageSettings.asset    （使用 Language 时）
                └── LanguageFontSettings.asset         （使用 Font 时）
```

### 脚本职责

#### Scripts/Core/

| 文件 | 职责 |
|---|---|
| `RootLifetimeScope.cs` | 整个应用的根作用域。注册 `RootEntryPoint` 和 `RootLifetimeScopeSettings` |
| `RootLifetimeScopeSettings.cs` | 持有 `ProductLifetimeScope` Prefab 引用的 ScriptableObject |
| `RootEntryPoint.cs` | `IStartable`。启动时将 `ProductLifetimeScope` Prefab 实例化为子作用域 |
| `ProductLifetimeScope.cs` | 注册所有 Lighthouse 服务和产品专属服务的作用域 |
| `ProductLifetimeScopeSettings.cs` | 保存产品专属设置的 ScriptableObject（按需添加字段） |
| `ProductEntryPoint.cs` | `IAsyncStartable`。对场景管理器设置父作用域、初始化语言、启动应用 |
| `SceneGroupProvider.cs` | `ISceneGroupProvider` 的实现。定义 `MainSceneId` 与 `SceneGroup` 的映射关系 |

#### StaticResources/LifetimeScope/

| 文件 | 职责 |
|---|---|
| `RootLifetimeScope.prefab` | 附加了 `RootLifetimeScope` 组件的 Prefab |
| `RootLifetimeScopeData.asset` | `RootLifetimeScopeSettings`。引用 `ProductLifetimeScope.prefab` |
| `ProductLifetimeScope.prefab` | 附加了 `ProductLifetimeScope` 组件的 Prefab |
| `ProductLifetimeScopeSettings.asset` | `ProductLifetimeScopeSettings` 的 ScriptableObject |
| `VContainerSettings.asset` | VContainer 启动设置。将 `RootLifetimeScope.prefab` 指定为 Root Prefab |
| `SupportedLanguageSettings.asset` | 支持的语言代码列表（使用 Language 时） |
| `LanguageFontSettings.asset` | 每种语言的字体映射（使用 Font 时） |

### 实现步骤

#### 1. 创建脚本

`RootLifetimeScope` / `RootEntryPoint` 是最小配置。

```csharp
public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] RootLifetimeScopeSettings rootLifetimeScopeSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<RootEntryPoint>();
        builder.RegisterInstance(rootLifetimeScopeSettings);
    }
}
```

```csharp
public class RootEntryPoint : IStartable
{
    readonly RootLifetimeScope rootLifetimeScope;
    readonly RootLifetimeScopeSettings rootLifetimeScopeSettings;

    [Inject]
    public RootEntryPoint(RootLifetimeScope rootLifetimeScope, RootLifetimeScopeSettings rootLifetimeScopeSettings)
    {
        this.rootLifetimeScope = rootLifetimeScope;
        this.rootLifetimeScopeSettings = rootLifetimeScopeSettings;
    }

    public void Start()
    {
        using (LifetimeScope.EnqueueParent(rootLifetimeScope))
        {
            var instance = Object.Instantiate(rootLifetimeScopeSettings.ProductLifetimeScopePrefab);
            Object.DontDestroyOnLoad(instance.gameObject);
        }
    }
}
```

在 `ProductLifetimeScope` 中注册需要使用的 Lighthouse 服务。

```csharp
public class ProductLifetimeScope : LifetimeScope
{
    [SerializeField] ProductLifetimeScopeSettings productLifetimeScopeSettings;
    // ...其他字段（LHCanvasSceneObject、SupportedLanguageSettings 等）

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ProductEntryPoint>();
        builder.RegisterInstance(productLifetimeScopeSettings);

        // Lighthouse 核心服务
        builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneTransitionController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionContextFactory>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneGroupProvider>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<MainSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ModuleSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneCameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionSequenceProvider>(Lifetime.Singleton).AsImplementedInterfaces();

        // 按需添加 LighthouseExtends 服务
    }
}
```

在 `ProductEntryPoint` 中设置场景管理器的父作用域并触发初始场景启动。

```csharp
public class ProductEntryPoint : IAsyncStartable
{
    // ... 字段及 [Inject] 构造函数

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        mainSceneManager.SetEnqueueParentLifetimeScope(() => LifetimeScope.EnqueueParent(productLifetimeScope));
        moduleSceneManager.SetEnqueueParentLifetimeScope(() => LifetimeScope.EnqueueParent(productLifetimeScope));

        // 语言初始化（使用 Language 时）
        await languageService.SetLanguage("ja", cancellation);

        // 跳转到初始场景等
        await launcher.Launch();
    }
}
```

#### 2. 创建 Prefab 和 ScriptableObject

1. 创建 `RootLifetimeScope.prefab` 并附加 `RootLifetimeScope` 组件
2. 通过 `Assets > Create > Scriptable Objects` 创建 `RootLifetimeScopeData.asset`（`RootLifetimeScopeSettings`）
3. 同样创建 `ProductLifetimeScope.prefab` 和 `ProductLifetimeScopeSettings.asset`
4. 在 `RootLifetimeScopeData.asset` 的 **Product Lifetime Scope Prefab** 字段中设置 `ProductLifetimeScope.prefab`
5. 在 `RootLifetimeScope.prefab` 的 Inspector 中，将 `RootLifetimeScopeData.asset` 设置到 **Root Lifetime Scope Settings** 字段

#### 3. 配置 VContainerSettings

通过 `Assets > Create > VContainer > VContainerSettings` 创建 `VContainerSettings.asset`，  
并将 `RootLifetimeScope.prefab` 设置为 **Root Lifetime Scope**。

> **补充说明：** `VContainerSettings.asset` 可放置在 `Resources` 文件夹或任意文件夹中，  
> 并在 Project Settings 的 VContainer 部分进行引用。详情请参阅 VContainer 文档。
