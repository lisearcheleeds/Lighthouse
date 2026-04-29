# Step 8: シーンの LifetimeScope と基底クラスの実装

## セットアップガイド — 目次

- [Step 1: 設定ファイルの生成と初期設定](./step1.md)
- [Step 2: ScreenStack の設定](./step2.md)
- [Step 3: TextTable の設定](./step3.md)
- [Step 4: VContainer LifetimeScope の実装](./step4.md)
- [Step 5: Launcher シーンと Launcher の実装](./step5.md)
- [Step 6: シーンファイルの作成と Build Settings への登録](./step6.md)
- [Step 7: シーンスクリプトの生成](./step7.md)
- [Step 8: シーンの LifetimeScope と基底クラスの実装](./step8.md)

---

### シーンごとの LifetimeScope

各シーン（メイン・モジュール）には、そのシーン専用の `LifetimeScope` を用意します。  
シーンの `LifetimeScope` は `ProductLifetimeScope` の子スコープとして自動的に構成されます。

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

シーン GameObject の **LifetimeScope** コンポーネントに、このスクリプトをアタッチします。

### シーンスクリプトの実装

`MainSceneBase<TTransitionData>` または `CanvasMainSceneBase<TTransitionData>` を継承して実装します。

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

Canvas を使用しないシーンは `MainSceneBase<TTransitionData>` を、Canvas を使用するシーンは `CanvasMainSceneBase<TTransitionData>` を継承してください。

### モジュールシーンの実装

モジュールシーンもメインシーンと同じ構造です。`ModuleSceneBase` または `CanvasModuleSceneBase` を継承し、専用の `LifetimeScope` を用意します。

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

> モジュールシーンは `TransitionData` を持ちません。メインシーンの遷移に追随してロード・アンロードされます。

### SaveSceneState と遷移のリダイレクト

`MainSceneBase` には `SaveSceneState` をオーバーライドできます。このメソッドはシーンを離れる前（`LoadNextSceneStatePhase`）に呼ばれ、サーバー通信やドメインデータの保存に使います。

```csharp
public override async UniTask SaveSceneState(CancellationToken cancelToken)
{
    await SaveProgress(cancelToken);
}
```

`LoadSceneState`（`TransitionDataBase` 側）で `LHSceneInterceptException` を投げると、遷移を別のシーンにリダイレクトできます。`SaveSceneState` が完了した後に `LoadSceneState` が実行されるため、セーブの整合性は保たれます。

```csharp
// TransitionData 内で期限切れを検知して別画面にリダイレクトする例
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    var isExpired = await CheckEventExpired(ct);
    if (isExpired)
    {
        throw new LHSceneInterceptException(new HomeTransitionData());
    }
}
```

> `LHSceneInterceptException` は `LoadNextSceneStatePhase`（`CanTransitionIntercept = true`）でのみ有効です。  
> それ以外のフェーズで投げた場合は `InvalidOperationException` として実装者に通知されます。

### プロダクト共通の基底クラス（推奨）

アニメーション・InputLayer の Push/Pop・AssetScope の管理をまとめた中間基底クラスを用意します。各シーンはこのクラスを継承することでボイラープレートを削減できます。

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

    // このシーンで使う InputLayer を返す（不要な場合は null を返す）
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

各シーンは最小限の実装になります。

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

### SceneGroupProvider の実装

SceneId の生成後、`ISceneGroupProvider` を実装して SceneId と `SceneGroup` のマッピングを定義します。  
これを `ProductLifetimeScope` に登録することで、シーンの遷移時に正しいシーングループがロードされます。

```csharp
public sealed class SceneGroupProvider : ISceneGroupProvider
{
    // 全シーンに必ず同伴するモジュールシーン
    static readonly ModuleSceneId[] RequireModuleSceneIds =
    {
        YourProductModuleSceneId.Overlay,
    };

    // メインシーンと追加モジュールシーンのマッピング
    static readonly IReadOnlyDictionary<MainSceneId, ModuleSceneId[]> SceneModuleMap =
        new Dictionary<MainSceneId, ModuleSceneId[]>
        {
            { YourProductMainSceneId.Splash, null },
            { YourProductMainSceneId.Home,   new[] { YourProductModuleSceneId.Background } },
        };

    // 同時にキャッシュするメインシーンのグループ定義
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
        // 同一シーンが複数グループに割り当てられていないかエディター時に検証する
        var duplicates = MainSceneGroupList
            .SelectMany(g => g)
            .GroupBy(id => id.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();
        if (duplicates.Length > 0)
            throw new Exception($"SceneGroupList に重複シーンがあります: {string.Join(", ", duplicates)}");
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

> `MainSceneGroupList` の定義によってシーンのキャッシュ戦略が変わります。  
> 同じグループ内のシーンは同時に読み込まれてキャッシュされます。シーンは必ず1つのグループにのみ属してください。エディター時の重複チェックが設定ミスを早期に検出します。
