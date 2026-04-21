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

アニメーションやインプットレイヤーなど、シーン全体で共通の処理をまとめた中間基底クラスを用意することを推奨します。

```csharp
// プロダクト全体で共通の処理をまとめた中間基底クラス
public abstract class YourProductCanvasMainSceneBase<TTransitionData> : CanvasMainSceneBase<TTransitionData>
    where TTransitionData : TransitionDataBase
{
    // アニメーション・入力レイヤーなど共通処理をここに追加
}
```

各シーンはこのクラスを継承することで、ボイラープレートを削減できます。

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

    SceneGroup ISceneGroupProvider.GetSceneGroup(MainSceneId mainSceneId)
    {
        return SceneGroupList.First(g => g.MainSceneIds.Contains(mainSceneId));
    }

    // ... （SceneGroup の構築ロジック）
}
```

> `MainSceneGroupList` の定義によってシーンのキャッシュ戦略が変わります。  
> 同じグループ内のシーンは同時に読み込まれてキャッシュされます。
