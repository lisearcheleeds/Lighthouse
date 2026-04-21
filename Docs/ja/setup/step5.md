# Step 5: Launcher シーンと Launcher の実装

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

### Launcher シーンの役割

Launcher シーンはアプリケーションの**エントリーポイントとなるシーン**です。  
Unity 起動時に最初にロードされ、VContainer が `RootLifetimeScope` を生成する起点となります。  
シーン自体にゲームロジックは持たせず、極力空に保ちます。

```
Build Settings:
  0: Launcher      ← 起動時に最初にロードされるシーン
  1: (各ゲームシーン)
```

### ILauncher / Launcher の役割

`ILauncher` はアプリの初回起動と再起動（リブート）を担うインターフェースです。

```csharp
public interface ILauncher
{
    UniTask Launch();   // 初回起動
    void Reboot();      // アプリの再起動（Launcher シーンへ戻り再実行）
}
```

`Launcher` を `ProductLifetimeScope` に登録することで、`ProductEntryPoint` から呼び出せるようになります。

```csharp
builder.Register<Launcher>(Lifetime.Singleton).AsImplementedInterfaces();
```

### 実装例

```csharp
public sealed class Launcher : ILauncher
{
    static readonly string LauncherSceneName = "Launcher";

    readonly ISceneManager sceneManager;

    [Inject]
    public Launcher(ISceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
    }

    async UniTask ILauncher.Launch()
    {
        await FirstLaunchProcess();  // 初回起動時のみ必要な処理（キャッシュ構築など）
        await LaunchProcess();       // 毎回の起動処理（マスタデータ読み込みなど）
        TransitionNextScene();       // 最初のゲームシーンへ遷移
    }

    void ILauncher.Reboot()
    {
        RebootProcess().Forget();

        async UniTask RebootProcess()
        {
            await sceneManager.PreReboot();
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                LauncherSceneName, LoadSceneMode.Single);
            await LaunchProcess();
            TransitionNextScene();
        }
    }

    UniTask FirstLaunchProcess() => UniTask.CompletedTask;  // 必要に応じて実装
    UniTask LaunchProcess() => UniTask.CompletedTask;       // 必要に応じて実装

    void TransitionNextScene()
    {
        UniTask.Void(async () =>
        {
            await sceneManager.TransitionScene(new SplashTransitionData());

            // 遷移完了後に Launcher シーンをアンロード
            if (!string.IsNullOrEmpty(
                    UnityEngine.SceneManagement.SceneManager.GetSceneByName(LauncherSceneName).name))
            {
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(LauncherSceneName);
            }
        });
    }
}
```

### 起動フローの全体像

```
Unity 起動
  └── Launcher シーンをロード
        └── VContainerSettings が RootLifetimeScope Prefab を自動生成
              └── RootEntryPoint.Start()
                    └── ProductLifetimeScope Prefab を子スコープとして生成
                          └── ProductEntryPoint.StartAsync()
                                ├── シーンマネージャーへ親スコープを登録
                                ├── 言語初期化
                                └── Launcher.Launch()
                                      ├── 初回起動処理
                                      ├── 起動処理
                                      └── 最初のシーンへ遷移 → Launcher シーンをアンロード
```
