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

Create an intermediate base class that consolidates animation, input layer push/pop, and asset scope management common to all scenes. Individual scenes inherit from this class instead of `CanvasMainSceneBase` directly.

```csharp
[RequireComponent(typeof(LHSceneTransitionAnimatorManager))]
public abstract class YourProductCanvasMainSceneBase<TTransitionData>
    : CanvasMainSceneBase<TTransitionData>
    where TTransitionData : TransitionDataBase
{
    [SerializeField] LHSceneTransitionAnimatorManager sceneTransitionAnimatorManager;

    IInputLayerController inputLayerController;
    IInputLayer currentInputLayer;
    IAssetManager assetManager;
    InputActions inputActions;

    protected IAssetScope AssetScope { get; private set; }

    [Inject]
    public void Construct(
        IInputLayerController inputLayerController,
        InputActions inputActions,
        IAssetManager assetManager)
    {
        this.inputLayerController = inputLayerController;
        this.inputActions = inputActions;
        this.assetManager = assetManager;
    }

    // Override to return the input layer for this scene (return null for no input layer)
    protected virtual IInputLayer CreateInputLayer(InputActions inputActions) => null;

    protected virtual InputActionMap GetInputLayerActionMap(InputActions inputActions)
        => inputActions.Scene;

    protected override UniTask OnSetup()
    {
        AssetScope = assetManager.CreateScope();
        return base.OnSetup();
    }

    public override async UniTask OnUnload()
    {
        await base.OnUnload();
        AssetScope?.Dispose();
        AssetScope = null;
    }

    protected override async UniTask OnEnter(ISceneTransitionContext context, CancellationToken ct)
    {
        var layer = CreateInputLayer(inputActions);
        var map   = GetInputLayerActionMap(inputActions);
        if (layer != null && map != null)
        {
            currentInputLayer = layer;
            inputLayerController.PushLayer(currentInputLayer, map);
        }
        await base.OnEnter(context, ct);
    }

    protected override async UniTask OnLeave(ISceneTransitionContext context, CancellationToken ct)
    {
        await base.OnLeave(context, ct);
        if (currentInputLayer != null)
        {
            inputLayerController.PopLayer(currentInputLayer);
            currentInputLayer = null;
        }
    }

    public override void ResetInAnimation(ISceneTransitionContext context)
        => sceneTransitionAnimatorManager.ResetInAnimation();

    protected override async UniTask InAnimation(ISceneTransitionContext context)
        => await sceneTransitionAnimatorManager.InAnimation();

    protected override async UniTask OutAnimation(ISceneTransitionContext context)
        => await sceneTransitionAnimatorManager.OutAnimation();

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        sceneTransitionAnimatorManager ??= GetComponent<LHSceneTransitionAnimatorManager>();
    }
#endif
}
```

Each scene then becomes minimal:

```csharp
public class HomeScene : YourProductCanvasMainSceneBase<HomeScene.HomeTransitionData>
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

    protected override IInputLayer CreateInputLayer(InputActions inputActions)
        => new DefaultSceneInputLayer(inputActions);

    protected override UniTask OnEnter(HomeTransitionData data,
        ISceneTransitionContext context, CancellationToken ct)
    {
        homePresenter.OnEnter();
        return UniTask.CompletedTask;
    }
}
```

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

    static readonly SceneGroup[] SceneGroupList = CreateSceneGroups();

    SceneGroup ISceneGroupProvider.GetSceneGroup(MainSceneId mainSceneId)
    {
        return SceneGroupList.First(g => g.MainSceneIds.Contains(mainSceneId));
    }

    static SceneGroup[] CreateSceneGroups()
    {
#if UNITY_EDITOR
        // Detect scenes assigned to more than one group at edit time.
        var duplicates = MainSceneGroupList
            .SelectMany(g => g)
            .GroupBy(id => id.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();
        if (duplicates.Length > 0)
            throw new Exception($"Duplicate scenes in SceneGroupList: {string.Join(", ", duplicates)}");
#endif
        return MainSceneGroupList.Select(CreateSceneGroup).ToArray();
    }

    static SceneGroup CreateSceneGroup(MainSceneId[] mainSceneIds)
    {
        var map = mainSceneIds.ToDictionary(
            id => id,
            id => RequireModuleSceneIds
                .Concat(SceneModuleMap[id] ?? Array.Empty<ModuleSceneId>())
                .ToArray());

        return new SceneGroup(map);
    }
}
```

> The `MainSceneGroupList` definition determines the scene caching strategy.  
> Scenes in the same group are loaded and cached simultaneously. A scene must appear in exactly one group — the editor-mode duplicate check catches misconfiguration early.
