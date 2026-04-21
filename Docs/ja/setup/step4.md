# Step 4: VContainer LifetimeScope の実装

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

Lighthouse のシーンシステムは VContainer の DI スコープと連携して動作します。  
以下のスクリプトと Prefab・ScriptableObject を用意することで、アプリケーションの起動フローが構成されます。

> VContainer の基本的な使い方: **[TODO: VContainer ドキュメント URL]**  
> Unity プロジェクトの初期セットアップ補助: **[TODO: Preset startup unity package URL]**

### 推奨ファイル構成

以下の配置を推奨します。

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
                ├── SupportedLanguageSettings.asset    （Language 使用時）
                └── LanguageFontSettings.asset         （Font 使用時）
```

### スクリプトの役割

#### Scripts/Core/

| ファイル | 役割 |
|---|---|
| `RootLifetimeScope.cs` | アプリ全体のルートスコープ。`RootEntryPoint` と `RootLifetimeScopeSettings` を登録する |
| `RootLifetimeScopeSettings.cs` | `ProductLifetimeScope` の Prefab 参照を持つ ScriptableObject |
| `RootEntryPoint.cs` | `IStartable`。起動時に `ProductLifetimeScope` Prefab を子スコープとしてインスタンス化する |
| `ProductLifetimeScope.cs` | Lighthouse サービス群とプロダクト固有のサービスをすべて登録するスコープ |
| `ProductLifetimeScopeSettings.cs` | プロダクト固有の設定を保持する ScriptableObject（必要に応じてフィールドを追加） |
| `ProductEntryPoint.cs` | `IAsyncStartable`。シーンマネージャーへの親スコープ設定・言語初期化・アプリ起動を行う |
| `SceneGroupProvider.cs` | `ISceneGroupProvider` の実装。MainSceneId と SceneGroup のマッピングを定義する |

#### StaticResources/LifetimeScope/

| ファイル | 役割 |
|---|---|
| `RootLifetimeScope.prefab` | `RootLifetimeScope` コンポーネントをアタッチした Prefab |
| `RootLifetimeScopeData.asset` | `RootLifetimeScopeSettings`。`ProductLifetimeScope.prefab` を参照する |
| `ProductLifetimeScope.prefab` | `ProductLifetimeScope` コンポーネントをアタッチした Prefab |
| `ProductLifetimeScopeSettings.asset` | `ProductLifetimeScopeSettings` の ScriptableObject |
| `VContainerSettings.asset` | VContainer の起動設定。Root Prefab として `RootLifetimeScope.prefab` を指定する |
| `SupportedLanguageSettings.asset` | 対応言語コードのリスト（Language 使用時） |
| `LanguageFontSettings.asset` | 言語ごとのフォントマッピング（Font 使用時） |

### 実装手順

#### 1. スクリプトを作成する

`RootLifetimeScope` / `RootEntryPoint` は最小限の構成です。

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

`ProductLifetimeScope` では使用する Lighthouse サービスを登録します。

```csharp
public class ProductLifetimeScope : LifetimeScope
{
    [SerializeField] ProductLifetimeScopeSettings productLifetimeScopeSettings;
    // ...その他フィールド（LHCanvasSceneObject, SupportedLanguageSettings など）

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ProductEntryPoint>();
        builder.RegisterInstance(productLifetimeScopeSettings);

        // Lighthouse コアサービス
        builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneTransitionController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionContextFactory>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneGroupProvider>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<MainSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ModuleSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneCameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionSequenceProvider>(Lifetime.Singleton).AsImplementedInterfaces();

        // 使用する LighthouseExtends サービスを必要に応じて追加
    }
}
```

`ProductEntryPoint` では、シーンマネージャーへの親スコープ設定と初回シーン起動を行います。

```csharp
public class ProductEntryPoint : IAsyncStartable
{
    // ... フィールドと [Inject] コンストラクタ

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        mainSceneManager.SetEnqueueParentLifetimeScope(() => LifetimeScope.EnqueueParent(productLifetimeScope));
        moduleSceneManager.SetEnqueueParentLifetimeScope(() => LifetimeScope.EnqueueParent(productLifetimeScope));

        // 言語初期化（Language 使用時）
        await languageService.SetLanguage("ja", cancellation);

        // 初回シーンへの遷移など
        await launcher.Launch();
    }
}
```

#### 2. Prefab と ScriptableObject を作成する

1. `RootLifetimeScope.prefab` を作成し、`RootLifetimeScope` コンポーネントをアタッチします
2. `RootLifetimeScopeData.asset` (`RootLifetimeScopeSettings`) を `Assets > Create > Scriptable Objects` から作成します
3. 同様に `ProductLifetimeScope.prefab` と `ProductLifetimeScopeSettings.asset` を作成します
4. `RootLifetimeScopeData.asset` の **Product Lifetime Scope Prefab** に `ProductLifetimeScope.prefab` を設定します
5. `RootLifetimeScope.prefab` の Inspector で **Root Lifetime Scope Settings** に `RootLifetimeScopeData.asset` を設定します

#### 3. VContainerSettings を設定する

`Assets > Create > VContainer > VContainerSettings` から `VContainerSettings.asset` を作成し、  
**Root Lifetime Scope** に `RootLifetimeScope.prefab` を設定します。

> **補足:** `VContainerSettings.asset` は `Resources` フォルダまたは任意のフォルダに配置し、  
> Project Settings の VContainer セクションで参照させることもできます。詳細は VContainer のドキュメントを参照してください。
