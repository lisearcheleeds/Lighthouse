# Step 8: Scene LifetimeScope and Base Class Implementation

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

### Per-Scene LifetimeScope

Each scene (main and module) has its own dedicated `LifetimeScope`.  
The scene's `LifetimeScope` is automatically configured as a child scope of `ProductLifetimeScope`.

```csharp
public class HomeLifetimeScope : LifetimeScope
{
    [SerializeField] HomeScene homeScene;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(homeScene);
        builder.Register<HomePresenter>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

Attach this script to the **LifetimeScope** component on the scene GameObject.

### Scene Script Implementation

Inherit from `MainSceneBase<TTransitionData>` or `CanvasMainSceneBase<TTransitionData>`.

```csharp
public class HomeScene : CanvasMainSceneBase<HomeScene.HomeTransitionData>
{
    IHomePresenter homePresenter;

    public override MainSceneId MainSceneId => YourProductMainSceneId.Home;

    public class HomeTransitionData : TransitionDataBase
    {
        public override MainSceneId MainSceneId => YourProductMainSceneId.Home;
    }

    [Inject]
    public void Construct(IHomePresenter homePresenter)
    {
        this.homePresenter = homePresenter;
    }
}
```

Use `MainSceneBase<TTransitionData>` for scenes without a Canvas, and `CanvasMainSceneBase<TTransitionData>` for scenes with a Canvas.

### Module Scene Implementation

Module scenes follow the same structure as main scenes. Inherit from `ModuleSceneBase` or `CanvasModuleSceneBase` and provide a dedicated `LifetimeScope`.

```csharp
public class BackgroundScene : CanvasModuleSceneBase
{
    IBackgroundPresenter backgroundPresenter;

    [Inject]
    public void Construct(IBackgroundPresenter backgroundPresenter)
    {
        this.backgroundPresenter = backgroundPresenter;
    }
}
```

```csharp
public class BackgroundLifetimeScope : LifetimeScope
{
    [SerializeField] BackgroundScene backgroundScene;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(backgroundScene);
        builder.Register<BackgroundPresenter>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

> Module scenes do not have `TransitionData`. They are loaded and unloaded following the main scene's transitions.

### SaveSceneState and Transition Redirect

`MainSceneBase` provides a `SaveSceneState` override point. It is called before leaving the scene (in `LoadNextSceneStatePhase`) and is the place to persist domain state or communicate with a server.

```csharp
public override async UniTask SaveSceneState(CancellationToken cancelToken)
{
    await SaveProgress(cancelToken);
}
```

By throwing `LHSceneInterceptException` inside `LoadSceneState` (on the `TransitionDataBase` side), you can redirect the transition to a different scene. Because `SaveSceneState` completes before `LoadSceneState` runs, save consistency is guaranteed.

```csharp
// Example: detect expiry inside TransitionData and redirect to a different screen
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    var isExpired = await CheckEventExpired(ct);
    if (isExpired)
    {
        throw new LHSceneInterceptException(new HomeTransitionData());
    }
}
```

> `LHSceneInterceptException` is only valid in `LoadNextSceneStatePhase` (`CanTransitionIntercept = true`).  
> Throwing it in any other phase is reported as an `InvalidOperationException` to alert the developer.

### Product-Wide Common Base Class (Recommended)

It is recommended to create an intermediate base class that consolidates processing common to all scenes, such as animation and input layer handling.

```csharp
// Intermediate base class consolidating product-wide common processing
public abstract class YourProductCanvasMainSceneBase<TTransitionData> : CanvasMainSceneBase<TTransitionData>
    where TTransitionData : TransitionDataBase
{
    // Add common processing such as animation and input layer here
}
```

Each scene can inherit from this class to reduce boilerplate.

### SceneGroupProvider Implementation

After generating the SceneIds, implement `ISceneGroupProvider` to define the mapping between SceneIds and `SceneGroup`.  
Registering this in `ProductLifetimeScope` ensures the correct scene group is loaded during scene transitions.

```csharp
public sealed class SceneGroupProvider : ISceneGroupProvider
{
    // Module scenes that always accompany every scene
    static readonly ModuleSceneId[] RequireModuleSceneIds =
    {
        YourProductModuleSceneId.Overlay,
    };

    // Mapping of main scenes to their additional module scenes
    static readonly IReadOnlyDictionary<MainSceneId, ModuleSceneId[]> SceneModuleMap =
        new Dictionary<MainSceneId, ModuleSceneId[]>
        {
            { YourProductMainSceneId.Splash, null },
            { YourProductMainSceneId.Home,   new[] { YourProductModuleSceneId.Background } },
        };

    // Definition of main scene groups to cache simultaneously
    static readonly MainSceneId[][] MainSceneGroupList =
    {
        new[] { YourProductMainSceneId.Splash },
        new[] { YourProductMainSceneId.Home },
    };

    SceneGroup ISceneGroupProvider.GetSceneGroup(MainSceneId mainSceneId)
    {
        return SceneGroupList.First(g => g.MainSceneIds.Contains(mainSceneId));
    }

    // ... (SceneGroup construction logic)
}
```

> The `MainSceneGroupList` definition determines the scene caching strategy.  
> Scenes in the same group are loaded and cached simultaneously.
