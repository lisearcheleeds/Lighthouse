# Step 8: 씬 LifetimeScope 및 기반 클래스 구현

## 세팅 가이드 — 목차

- [Step 1: 설정 에셋 생성](./step1.md)
- [Step 2: ScreenStack 설정](./step2.md)
- [Step 3: TextTable 설정](./step3.md)
- [Step 4: VContainer LifetimeScope 구현](./step4.md)
- [Step 5: Launcher 씬 및 Launcher 구현](./step5.md)
- [Step 6: 씬 파일 생성 및 Build Settings 등록](./step6.md)
- [Step 7: 씬 스크립트 생성](./step7.md)
- [Step 8: 씬 LifetimeScope 및 기반 클래스 구현](./step8.md)

---

### 씬별 LifetimeScope

각 씬(메인·모듈)에는 해당 씬 전용의 `LifetimeScope`를 준비합니다.  
씬의 `LifetimeScope`는 `ProductLifetimeScope`의 자식 스코프로 자동 구성됩니다.

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

씬 GameObject의 **LifetimeScope** 컴포넌트에 이 스크립트를 부착합니다.

### 씬 스크립트 구현

`MainSceneBase<TTransitionData>` 또는 `CanvasMainSceneBase<TTransitionData>`를 상속하여 구현합니다.

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

Canvas를 사용하지 않는 씬은 `MainSceneBase<TTransitionData>`를, Canvas를 사용하는 씬은 `CanvasMainSceneBase<TTransitionData>`를 상속하세요.

### 모듈 씬 구현

모듈 씬도 메인 씬과 동일한 구조입니다. `ModuleSceneBase` 또는 `CanvasModuleSceneBase`를 상속하고 전용 `LifetimeScope`를 준비합니다.

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

> 모듈 씬은 `TransitionData`를 갖지 않습니다. 메인 씬의 전환에 따라 로드·언로드됩니다.

### SaveSceneState와 전환 리다이렉트

`MainSceneBase`에는 `SaveSceneState`를 오버라이드할 수 있습니다. 이 메서드는 씬을 떠나기 전(`LoadNextSceneStatePhase`)에 호출되며, 서버 통신이나 도메인 데이터 저장에 사용합니다.

```csharp
public override async UniTask SaveSceneState(CancellationToken cancelToken)
{
    await SaveProgress(cancelToken);
}
```

`LoadSceneState`(`TransitionDataBase` 측)에서 `LHSceneInterceptException`을 던지면 전환을 다른 씬으로 리다이렉트할 수 있습니다. `SaveSceneState`가 완료된 후 `LoadSceneState`가 실행되므로 저장 정합성이 보장됩니다.

```csharp
// TransitionData 내에서 만료를 감지해 다른 화면으로 리다이렉트하는 예시
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    var isExpired = await CheckEventExpired(ct);
    if (isExpired)
    {
        throw new LHSceneInterceptException(new HomeTransitionData());
    }
}
```

> `LHSceneInterceptException`은 `LoadNextSceneStatePhase`(`CanTransitionIntercept = true`)에서만 유효합니다.  
> 다른 페이즈에서 던진 경우 `InvalidOperationException`으로 구현자에게 알립니다.

### 프로덕트 공통 기반 클래스 (권장)

애니메이션, InputLayer의 Push/Pop, AssetScope 관리를 통합한 중간 기반 클래스를 준비합니다. 각 씬은 이 클래스를 상속함으로써 보일러플레이트를 줄일 수 있습니다.

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

    // 이 씬에서 사용할 InputLayer를 반환합니다 (불필요한 경우 null 반환)
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

각 씬은 최소한의 구현이 됩니다.

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

### SceneGroupProvider 구현

SceneId 생성 후, `ISceneGroupProvider`를 구현하여 SceneId와 `SceneGroup`의 매핑을 정의합니다.  
이것을 `ProductLifetimeScope`에 등록하면 씬 전환 시 올바른 씬 그룹이 로드됩니다.

```csharp
public sealed class SceneGroupProvider : ISceneGroupProvider
{
    // 모든 씬에 반드시 동반하는 모듈 씬
    static readonly ModuleSceneId[] RequireModuleSceneIds =
    {
        YourProductModuleSceneId.Overlay,
    };

    // 메인 씬과 추가 모듈 씬의 매핑
    static readonly IReadOnlyDictionary<MainSceneId, ModuleSceneId[]> SceneModuleMap =
        new Dictionary<MainSceneId, ModuleSceneId[]>
        {
            { YourProductMainSceneId.Splash, null },
            { YourProductMainSceneId.Home,   new[] { YourProductModuleSceneId.Background } },
        };

    // 동시에 캐시할 메인 씬 그룹 정의
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
        // 에디터 실행 시 동일한 씬이 여러 그룹에 할당되지 않았는지 검증합니다
        var duplicates = MainSceneGroupList
            .SelectMany(g => g)
            .GroupBy(id => id.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();
        if (duplicates.Length > 0)
            throw new Exception($"SceneGroupList에 중복된 씬이 있습니다: {string.Join(", ", duplicates)}");
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

> `MainSceneGroupList`의 정의에 따라 씬의 캐시 전략이 달라집니다.  
> 같은 그룹 내의 씬은 동시에 로드되어 캐시됩니다. 씬은 반드시 하나의 그룹에만 속해야 합니다. 에디터 실행 시 중복 검사가 설정 오류를 조기에 감지합니다.
