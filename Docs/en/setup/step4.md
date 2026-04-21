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

> VContainer basics: **[TODO: VContainer documentation URL]**  
> Unity project initial setup helper: **[TODO: Preset startup unity package URL]**

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
    // ...other fields (LHCanvasSceneObject, SupportedLanguageSettings, etc.)

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ProductEntryPoint>();
        builder.RegisterInstance(productLifetimeScopeSettings);

        // Lighthouse core services
        builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneTransitionController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionContextFactory>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneGroupProvider>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<MainSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ModuleSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneCameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionSequenceProvider>(Lifetime.Singleton).AsImplementedInterfaces();

        // Add LighthouseExtends services as needed
    }
}
```

In `ProductEntryPoint`, set the parent scope on scene managers and trigger the initial scene launch.

```csharp
public class ProductEntryPoint : IAsyncStartable
{
    // ... fields and [Inject] constructor

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        mainSceneManager.SetEnqueueParentLifetimeScope(() => LifetimeScope.EnqueueParent(productLifetimeScope));
        moduleSceneManager.SetEnqueueParentLifetimeScope(() => LifetimeScope.EnqueueParent(productLifetimeScope));

        // Language initialization (when using Language)
        await languageService.SetLanguage("ja", cancellation);

        // Transition to the initial scene, etc.
        await launcher.Launch();
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
