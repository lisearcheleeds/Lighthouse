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

> VContainer 基本用法: [https://vcontainer.hadashikick.jp](https://vcontainer.hadashikick.jp)

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
    [SerializeField] SupportedLanguageSettings supportedLanguageSettings;  // 使用 Language 时
    [SerializeField] LanguageFontSettings languageFontSettings;            // 使用 Font 时
    [SerializeField] LHCanvasSceneObject canvasSceneObjectPrefab;
    [SerializeField] LHInputBlocker inputBlockerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ProductEntryPoint>();
        builder.RegisterInstance(productLifetimeScopeSettings);

        // Lighthouse 核心
        builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneTransitionController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionContextFactory>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneGroupProvider>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<MainSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ModuleSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneCameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionSequenceProvider>(Lifetime.Singleton).AsImplementedInterfaces();

        // LighthouseExtends — Language / Font / TextTable
        builder.RegisterInstance(supportedLanguageSettings);
        builder.Register<SupportedLanguageService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<LanguageService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterInstance(languageFontSettings);
        builder.Register<FontService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<TextTableService>(Lifetime.Singleton).AsImplementedInterfaces();

        // FontService 和 TextTableService 默认延迟解析。
        // 为确保在 SetLanguage 调用前已向 ILanguageService 注册处理器，
        // 通过 RegisterBuildCallback 强制立即解析。
        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<IFontService>();
            container.Resolve<ITextTableService>();
        });

        // LighthouseExtends — UIComponent / InputLayer
        builder.Register<ExclusiveInputService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterComponentInNewPrefab(canvasSceneObjectPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad().AsImplementedInterfaces();
        builder.RegisterComponentInNewPrefab(inputBlockerPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad().AsImplementedInterfaces();

        var inputActions = new InputActions();
        builder.RegisterInstance(inputActions);
        builder.RegisterInstance(inputActions.asset).As<InputActionAsset>();
        builder.Register<InputLayerController>(Lifetime.Singleton).AsImplementedInterfaces();

        // LighthouseExtends — ScreenStack
        builder.Register<ScreenStackModuleProxy>(Lifetime.Singleton).AsImplementedInterfaces();

        // 产品专属服务
        builder.Register<Launcher>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

> **为什么需要 `RegisterBuildCallback`？**  
> VContainer 的单例默认延迟解析。FontService 和 TextTableService 必须在 `ProductEntryPoint` 调用 `SetLanguage` *之前* 向 `ILanguageService` 注册处理器。若省略此步骤，首次语言切换完成时字体和文本尚未加载，导致 UI 处于错误状态。

在 `ProductEntryPoint` 中设置场景管理器的父作用域，并从设备语言解析初始语言。

```csharp
public class ProductEntryPoint : IAsyncStartable
{
    readonly ProductLifetimeScope productLifetimeScope;
    readonly ILauncher launcher;
    readonly IMainSceneManager mainSceneManager;
    readonly IModuleSceneManager moduleSceneManager;
    readonly ILanguageService languageService;
    readonly ISupportedLanguageService supportedLanguageService;

    [Inject]
    public ProductEntryPoint(
        ProductLifetimeScope productLifetimeScope,
        ILauncher launcher,
        IMainSceneManager mainSceneManager,
        IModuleSceneManager moduleSceneManager,
        ILanguageService languageService,
        ISupportedLanguageService supportedLanguageService)
    {
        this.productLifetimeScope = productLifetimeScope;
        this.launcher = launcher;
        this.mainSceneManager = mainSceneManager;
        this.moduleSceneManager = moduleSceneManager;
        this.languageService = languageService;
        this.supportedLanguageService = supportedLanguageService;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        mainSceneManager.SetEnqueueParentLifetimeScope(
            () => LifetimeScope.EnqueueParent(productLifetimeScope));
        moduleSceneManager.SetEnqueueParentLifetimeScope(
            () => LifetimeScope.EnqueueParent(productLifetimeScope));

        await languageService.SetLanguage(
            ResolveInitialLanguage(Application.systemLanguage), cancellation);

        await launcher.Launch();
    }

    string ResolveInitialLanguage(SystemLanguage systemLanguage)
    {
        var code = systemLanguage switch
        {
            SystemLanguage.Japanese           => "ja",
            SystemLanguage.ChineseSimplified  => "zh",
            SystemLanguage.ChineseTraditional => "zh",
            SystemLanguage.Korean             => "ko",
            _                                 => "en",
        };

        // 若不在支持的语言列表中，则回退到默认语言
        var supported = supportedLanguageService.SupportedLanguages;
        for (var i = 0; i < supported.Count; i++)
        {
            if (supported[i] == code) return code;
        }

        return supportedLanguageService.DefaultLanguage;
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
