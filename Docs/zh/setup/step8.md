# Step 8: 场景 LifetimeScope 及基类实现

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

### 每个场景的 LifetimeScope

每个场景（主场景·模块场景）都需要准备该场景专用的 `LifetimeScope`。  
场景的 `LifetimeScope` 会自动配置为 `ProductLifetimeScope` 的子作用域。

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

将此脚本附加到场景 GameObject 的 **LifetimeScope** 组件上。

### 场景脚本实现

继承 `MainSceneBase<TTransitionData>` 或 `CanvasMainSceneBase<TTransitionData>` 进行实现。

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

不使用 Canvas 的场景继承 `MainSceneBase<TTransitionData>`，使用 Canvas 的场景继承 `CanvasMainSceneBase<TTransitionData>`。

### 模块场景实现

模块场景与主场景结构相同。继承 `ModuleSceneBase` 或 `CanvasModuleSceneBase`，并准备专用的 `LifetimeScope`。

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

> 模块场景不持有 `TransitionData`。它随主场景的跳转而加载和卸载。

### SaveSceneState 与跳转重定向

`MainSceneBase` 提供了 `SaveSceneState` 覆盖点。该方法在离开场景前（`LoadNextSceneStatePhase`）调用，用于服务器通信或持久化领域数据。

```csharp
public override async UniTask SaveSceneState(CancellationToken cancelToken)
{
    await SaveProgress(cancelToken);
}
```

在 `LoadSceneState`（`TransitionDataBase` 侧）中抛出 `LHSceneInterceptException`，可将跳转重定向到其他场景。由于 `SaveSceneState` 在 `LoadSceneState` 之前完成，因此保存的一致性得到保证。

```csharp
// 在 TransitionData 中检测到过期后重定向到其他界面的示例
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    var isExpired = await CheckEventExpired(ct);
    if (isExpired)
    {
        throw new LHSceneInterceptException(new HomeTransitionData());
    }
}
```

> `LHSceneInterceptException` 仅在 `LoadNextSceneStatePhase`（`CanTransitionIntercept = true`）中有效。  
> 在其他阶段抛出时，会以 `InvalidOperationException` 的形式通知实现者。

### 产品公共基类（推荐）

创建一个中间基类，统一管理动画、InputLayer 的 Push/Pop 以及 AssetScope。各场景继承此类即可减少样板代码。

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

    // 返回该场景使用的 InputLayer（不需要时返回 null）
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

各场景的实现因此变得极为简洁。

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

### SceneGroupProvider 实现

生成 SceneId 后，实现 `ISceneGroupProvider` 以定义 SceneId 与 `SceneGroup` 的映射关系。  
将其注册到 `ProductLifetimeScope` 后，场景跳转时将加载正确的场景组。

```csharp
public sealed class SceneGroupProvider : ISceneGroupProvider
{
    // 所有场景必须携带的模块场景
    static readonly ModuleSceneId[] RequireModuleSceneIds =
    {
        YourProductModuleSceneId.Overlay,
    };

    // 主场景与附加模块场景的映射
    static readonly IReadOnlyDictionary<MainSceneId, ModuleSceneId[]> SceneModuleMap =
        new Dictionary<MainSceneId, ModuleSceneId[]>
        {
            { YourProductMainSceneId.Splash, null },
            { YourProductMainSceneId.Home,   new[] { YourProductModuleSceneId.Background } },
        };

    // 同时缓存的主场景组定义
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
        // 编辑器模式下检测是否有场景被分配到多个组
        var duplicates = MainSceneGroupList
            .SelectMany(g => g)
            .GroupBy(id => id.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();
        if (duplicates.Length > 0)
            throw new Exception($"SceneGroupList 中存在重复场景: {string.Join(", ", duplicates)}");
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

> `MainSceneGroupList` 的定义决定了场景的缓存策略。  
> 同一组内的场景会同时加载并缓存。场景必须且只能属于一个组 — 编辑器模式下的重复检查可提前发现配置错误。
