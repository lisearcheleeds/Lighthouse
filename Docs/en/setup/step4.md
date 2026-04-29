# Step 4: VContainer LifetimeScope Implementation

## Setup Guide — Table of Contents

- [Step 1: Generate Settings Assets](./step1.md)
- [Step 2: ScreenStack Setup](./step2.md)
- [Step 3: TextTable Setup](./step3.md)
- [Step 4: VContainer LifetimeScope Implementation](./step4.md)
- [Step 5: Launcher Scene and Launcher Implementation](./step5.md)
- [Step 6: Create Scene Files and Register to Build Settings](./step6.md)
- [Step 7: Generate Scene Scripts](./step7.md)
- [Step 8: Scene LifetimeScope and Base Class Implementation](./step8.md)

---

The Lighthouse scene system works in conjunction with VContainer DI scopes.  
By preparing the following scripts, Prefabs, and ScriptableObjects, the application startup flow is configured.

> VContainer basics: [https://vcontainer.hadashikick.jp](https://vcontainer.hadashikick.jp)

### Recommended File Structure

The following layout is recommended.

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
                ├── SupportedLanguageSettings.asset    (when using Language)
                └── LanguageFontSettings.asset         (when using Font)
```

### Script Roles

#### Scripts/Core/

| File | Role |
|---|---|
| `RootLifetimeScope.cs` | Root scope for the entire app. Registers `RootEntryPoint` and `RootLifetimeScopeSettings` |
| `RootLifetimeScopeSettings.cs` | ScriptableObject holding a Prefab reference to `ProductLifetimeScope` |
| `RootEntryPoint.cs` | `IStartable`. Instantiates the `ProductLifetimeScope` Prefab as a child scope on startup |
| `ProductLifetimeScope.cs` | Scope that registers all Lighthouse services and product-specific services |
| `ProductLifetimeScopeSettings.cs` | ScriptableObject holding product-specific settings (add fields as needed) |
| `ProductEntryPoint.cs` | `IAsyncStartable`. Sets the parent scope on scene managers, initializes language, and starts the app |
| `SceneGroupProvider.cs` | Implementation of `ISceneGroupProvider`. Defines the mapping between `MainSceneId` and `SceneGroup` |

#### StaticResources/LifetimeScope/

| File | Role |
|---|---|
| `RootLifetimeScope.prefab` | Prefab with `RootLifetimeScope` component attached |
| `RootLifetimeScopeData.asset` | `RootLifetimeScopeSettings`. References `ProductLifetimeScope.prefab` |
| `ProductLifetimeScope.prefab` | Prefab with `ProductLifetimeScope` component attached |
| `ProductLifetimeScopeSettings.asset` | ScriptableObject for `ProductLifetimeScopeSettings` |
| `VContainerSettings.asset` | VContainer startup settings. Specifies `RootLifetimeScope.prefab` as the Root Prefab |
| `SupportedLanguageSettings.asset` | List of supported language codes (when using Language) |
| `LanguageFontSettings.asset` | Font mapping per language (when using Font) |

### Implementation Steps

#### 1. Create the Scripts

`RootLifetimeScope` / `RootEntryPoint` is a minimal configuration.

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

Register the Lighthouse services you use in `ProductLifetimeScope`.

```csharp
public class ProductLifetimeScope : LifetimeScope
{
    [SerializeField] ProductLifetimeScopeSettings productLifetimeScopeSettings;
    [SerializeField] SupportedLanguageSettings supportedLanguageSettings;  // when using Language
    [SerializeField] LanguageFontSettings languageFontSettings;            // when using Font
    [SerializeField] LHCanvasSceneObject canvasSceneObjectPrefab;
    [SerializeField] LHInputBlocker inputBlockerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ProductEntryPoint>();
        builder.RegisterInstance(productLifetimeScopeSettings);

        // Lighthouse core
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

        // FontService and TextTableService are lazy by default.
        // Force eager resolution so they register their handlers with ILanguageService
        // before SetLanguage is called in ProductEntryPoint.
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

        // Product services
        builder.Register<Launcher>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

> **Why `RegisterBuildCallback` for FontService and TextTableService?**  
> VContainer resolves singletons lazily by default. FontService and TextTableService must register their change handlers with `ILanguageService` *before* `SetLanguage` is called in `ProductEntryPoint`. Without this, the first language switch would complete before fonts or text tables are loaded, leaving the UI in an incorrect state.

In `ProductEntryPoint`, set the parent scope on scene managers and resolve the initial language from the device locale.

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
            SystemLanguage.Japanese          => "ja",
            SystemLanguage.ChineseSimplified  => "zh",
            SystemLanguage.ChineseTraditional => "zh",
            SystemLanguage.Korean            => "ko",
            _                                => "en",
        };

        // Fall back to the configured default if the resolved code is not supported.
        var supported = supportedLanguageService.SupportedLanguages;
        for (var i = 0; i < supported.Count; i++)
        {
            if (supported[i] == code) return code;
        }

        return supportedLanguageService.DefaultLanguage;
    }
}
```

#### 2. Create Prefabs and ScriptableObjects

1. Create `RootLifetimeScope.prefab` and attach the `RootLifetimeScope` component
2. Create `RootLifetimeScopeData.asset` (`RootLifetimeScopeSettings`) via `Assets > Create > Scriptable Objects`
3. Similarly create `ProductLifetimeScope.prefab` and `ProductLifetimeScopeSettings.asset`
4. Set `ProductLifetimeScope.prefab` in the **Product Lifetime Scope Prefab** field of `RootLifetimeScopeData.asset`
5. In the Inspector for `RootLifetimeScope.prefab`, set `RootLifetimeScopeData.asset` in the **Root Lifetime Scope Settings** field

#### 3. Configure VContainerSettings

Create `VContainerSettings.asset` via `Assets > Create > VContainer > VContainerSettings`, and set `RootLifetimeScope.prefab` as the **Root Lifetime Scope**.

> **Note:** `VContainerSettings.asset` can be placed in a `Resources` folder or any folder, and referenced from the VContainer section in Project Settings. See the VContainer documentation for details.
