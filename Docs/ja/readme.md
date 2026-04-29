# Lighthouse — 機能リファレンス

## 目次

- [シーンシステム](#シーンシステム)
  - [SceneGroup](#scenegroup)
  - [TransitionDataBase](#transitiondatabase)
  - [シーン基底クラス](#シーン基底クラス)
  - [SceneBase ライフサイクル](#scenebase-ライフサイクル)
  - [遷移タイプ](#遷移タイプ)
  - [遷移フェーズパイプライン](#遷移フェーズパイプライン)
  - [シーンのインターセプト](#シーンのインターセプト)
  - [戻るナビゲーション](#戻るナビゲーション)
  - [SceneCameraManager](#scenecameramanager)
- [ScreenStack](#screenstack)
  - [IScreenStackModule API](#iscreenstackmodule-api)
  - [IScreenStackData](#iscreenstackdata)
  - [ScreenStackBase ライフサイクル](#screenstackbase-ライフサイクル)
  - [Suspend / Resume](#suspend--resume)
  - [ScreenStackModuleProxy](#screenstackmoduleproxy)
  - [コード生成](#コード生成)
- [Addressable](#addressable)
- [Animation](#animation)
- [Language](#language)
- [Font](#font)
- [TextTable](#texttable)
- [TextMeshPro](#textmeshpro)
- [InputLayer](#inputlayer)
- [UIComponent](#uicomponent)

---

## シーンシステム

**パッケージ:** `com.lisearcheleeds.lighthouse`  
**追加の依存パッケージ:** VContainer · UniTask

シーンシステムはフレームワークのコアです。シーンをスタックで管理し、遷移を非同期フェーズパイプラインで実行します。

### SceneGroup

`SceneGroup` は、同時にロード・アンロードされるシーンのまとまりを表すクラスです。1 つ以上の **メインシーン** と、それぞれに対応する **モジュールシーン** の組み合わせで構成されます。

```csharp
// 例: SceneGroup の定義 (ISceneGroupProvider を実装してDIに登録)
new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
{
    { SceneIds.Home,     new[] { SceneIds.Header, SceneIds.Footer } },
    { SceneIds.Setting,  new[] { SceneIds.Header, SceneIds.Footer } },
    { SceneIds.Detail,   new[] { SceneIds.Header } },
});
```

同じ `SceneGroup` に属するシーンはキャッシュされます。グループをまたぐ遷移でのみシーンのロード・アンロードが発生するため、グループの粒度によってパフォーマンスとメモリのバランスを調整できます。

---

### TransitionDataBase

遷移先を指定するデータクラスの基底クラスです。遷移ごとに派生クラスを作成します。

```csharp
public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;

    // CanTransition = false にすると遷移そのものをスキップできる
    // CanBackTransition = false にすると戻る遷移のスタック対象から除外される

    // 遷移前に非同期処理が必要な場合はオーバーライドする
    // LHSceneInterceptException をスローすると別シーンへリダイレクトできる
    public override async UniTask LoadSceneState(
        TransitionDirectionType direction, CancellationToken ct)
    {
        // 例: 認証チェックしてログイン画面へリダイレクト
        if (!await authService.IsLoggedIn(ct))
            throw new LHSceneInterceptException(new LoginTransitionData());
    }
}
```

| プロパティ / メソッド | 説明 |
|---|---|
| `MainSceneId` | 遷移先のメインシーンID |
| `CanTransition` | `false` にするとこのデータへの遷移をスキップ |
| `CanBackTransition` | `false` にすると BackScene で戻り先候補から除外 |
| `LoadSceneState(direction, ct)` | 遷移直前の非同期処理。インターセプトもここで行う |

---

### シーン基底クラス

#### MainSceneBase\<TTransitionData\>

メインシーンに付与する MonoBehaviour の基底クラスです。

```csharp
public class HomeScene : MainSceneBase<HomeTransitionData>
{
    protected override async UniTask OnEnter(
        HomeTransitionData data, ISceneTransitionContext context, CancellationToken ct)
    {
        // 遷移データを使った初期化
    }

    // 離脱前に状態を保存したい場合はオーバーライド
    // LHSceneInterceptException をスローして別シーンへリダイレクトも可能
    public override UniTask SaveSceneState(CancellationToken ct) => UniTask.CompletedTask;
}
```

#### CanvasMainSceneBase\<TTransitionData\>

UI Canvas を持つメインシーン向けの基底クラスです。`CanvasGroup` のアルファ値を自動制御します（ロード時=0、入場時=1、離脱時=0）。`CanvasGroup` と `SceneCanvasInitializer` コンポーネントが必要です。

#### ModuleSceneBase

複数のメインシーンで共有されるモジュールシーン（ヘッダー・フッター等）の基底クラスです。`SceneTransitionDiff` に基づいてゲームオブジェクトの Active を自動で切り替えます。

#### CanvasModuleSceneBase

`CanvasGroup` アルファ制御付きのモジュールシーン基底クラスです。

---

### SceneBase ライフサイクル

全シーン基底クラスが持つライフサイクルフックです。

| メソッド | タイミング | 備考 |
|---|---|---|
| `OnSetup()` | 初回入場時のみ | DI 非依存の一度きりの初期化 |
| `OnLoad()` | シーングループのロード時 | デフォルトで `gameObject.SetActive(false)` |
| `OnEnter(context, ct)` | 入場時 | 毎回呼ばれる。TransitionData はここで受け取る |
| `InAnimation(context)` | 入場アニメーション再生時 | `OnBeginInAnimation` / `OnCompleteInAnimation` もあり |
| `OutAnimation(context)` | 離脱アニメーション再生時 | `OnBeginOutAnimation` / `OnCompleteOutAnimation` もあり |
| `OnLeave(context, ct)` | 離脱時 | |
| `OnUnload()` | シーングループのアンロード時 | |
| `OnSceneTransitionFinished(diff)` | 遷移が完全に完了した後 | |
| `ResetInAnimation(context)` | InAnimation の初期状態にリセット | Cross 遷移時に使用 |

`context` (`ISceneTransitionContext`) からは `TransitionData`・`SceneTransitionDiff`・`TransitionDirectionType`・`TransitionType` などが参照できます。

---

### 遷移タイプ

`ISceneManager.TransitionScene` の `transitionType` 引数で指定します。

| 値 | 動作 |
|---|---|
| `Auto` (デフォルト) | 同一 SceneGroup 内なら `Cross`、グループをまたぐなら `Exclusive` を自動選択 |
| `Exclusive` | OutAnimation → 旧シーン離脱 → シーンのロード/アンロード → 新シーン入場 → InAnimation の順番で実行 |
| `Cross` | 旧シーンと新シーンを並行して入れ替え。アニメーションをクロスフェードさせる場合に使う |

---

### 遷移フェーズパイプライン

シーン遷移は **フェーズ** と **ステップ** の 2 層構造になっています。

**Exclusive シーケンス（デフォルト）**

```
SaveCurrentSceneStatePhase  → 現シーンの SaveSceneState を呼ぶ
LoadNextSceneStatePhase     → 次シーンの LoadSceneState を呼ぶ（インターセプト可）
OutAnimationPhase           → 現シーンの OutAnimation を並列実行
LeaveScenePhase             → 現シーンの OnLeave を呼ぶ
LoadSceneGroupPhase         → 次シーングループをアディティブロード
UnloadSceneGroupPhase       → 不要なシーングループをアンロード
EnterScenePhase             → 次シーンの OnEnter を呼ぶ
InAnimationPhase            → 次シーンの InAnimation を並列実行
FinishTransitionPhase       → OnSceneTransitionFinished を通知
CleanupPhase                → カメラスタックの再構築
```

**Cross シーケンス（デフォルト）**

```
SaveCurrentSceneStatePhase
LoadNextSceneStatePhase
LoadSceneGroupPhase
EnterScenePhase
CrossAnimationPhase   → 旧シーンの Out と新シーンの In を並列実行
LeaveScenePhase
UnloadSceneGroupPhase
FinishTransitionPhase
```

`ISceneTransitionSequenceProvider` を独自実装して DI に登録することで、フェーズの順序・内容を自由にカスタマイズできます。

---

### シーンのインターセプト

`LHSceneInterceptException` を使うと、遷移中に別のシーンへリダイレクトできます。

```csharp
// TransitionDataBase.LoadSceneState 内でスロー
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    if (!isLoggedIn)
        throw new LHSceneInterceptException(new LoginTransitionData());
}
```

インターセプトは `LoadNextSceneStatePhase` の実行中のみ有効です。他のフェーズでスローした場合は `InvalidOperationException` になります。

---

### 戻るナビゲーション

`ISceneManager.BackScene()` を呼ぶと、遷移履歴スタックを遡って戻り先を解決します。

- `CanTransition = false` または `CanBackTransition = false` のエントリはスキップされます
- スタックに戻り先が存在しない場合は操作を無視します
- 戻り先が見つからない不正な状態では `InvalidOperationException` をスローします

```csharp
// 前のシーンに戻る
await sceneManager.BackScene();

// 遷移タイプを指定して戻る
await sceneManager.BackScene(TransitionType.Cross);
```

また `TransitionScene` の `backMainSceneId` 引数を指定すると、遷移後にスタックを指定シーンまで切り詰めることができます（特定シーンへのショートカット遷移に便利）。

---

### SceneCameraManager

URP のカメラスタックを遷移のたびに自動で再構築します。

- 各シーンの `GetSceneCameraList()` から収集したカメラを `SceneCameraType` と `CameraDefaultDepth` でソート
- 先頭のカメラを **Base**、残りを **Overlay** として設定
- UI 用カメラ（Canvas が使う）は常に末尾の Overlay として追加

カメラの順序を制御したい場合は `ISceneCamera` を実装し、`SceneCameraType` と `CameraDefaultDepth` を返すようにします。

---

## ScreenStack

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.screenstack`  
**依存パッケージ:** `com.lisearcheleeds.lighthouse` (コア)

ダイアログやオーバーレイをスタックで管理するモジュールです。Open / Close 操作はキューに積まれ、前の操作が完了してから次が実行されます。

### IScreenStackModule API

```csharp
// データをキューに積むだけ（即座には開かない）
await screenStackModule.Enqueue(new SampleDialogData());

// キューの先頭を開く（直前に Enqueue が必要）
await screenStackModule.Open();

// Enqueue と Open を一度に行う（通常はこちらを使う）
await screenStackModule.Open(new SampleDialogData());

// 最前面のスクリーンを閉じる
await screenStackModule.Close();

// 特定のデータに対応するスクリーンを閉じる
await screenStackModule.Close(myDialogData);

// 現在シーンのスタックをすべて閉じる
await screenStackModule.ClearCurrentAll();

// シーンをまたいだスタック全体をクリア
await screenStackModule.ClearAll();
```

---

### IScreenStackData

各スクリーンのデータクラスに実装するインターフェースです。

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsSystem` | `bool` | `true` にするとシステム Canvas レイヤーに配置（通常 UI の上に重なる） |
| `IsOverlayOpen` | `bool` | `true` にすると直前のスクリーンを OutAnimation せず残したまま開く（オーバーレイ表示） |

```csharp
public class SampleDialogData : IScreenStackData
{
    public bool IsSystem => false;
    public bool IsOverlayOpen => false;

    public string Message { get; }
    public SampleDialogData(string message) => Message = message;
}
```

---

### ScreenStackBase ライフサイクル

各ダイアログ Prefab に付与する MonoBehaviour の基底クラスです。

| メソッド | タイミング |
|---|---|
| `OnInitialize()` | Prefab のインスタンス化直後、表示前 |
| `OnEnter(isResume)` | スタックの先頭になった時。`isResume=true` は別スクリーンが閉じて戻ってきた時 |
| `PlayInAnimation()` | 入場アニメーション。`ResetInAnimation()` で初期化 |
| `OnLeave()` | スタックから外れる直前 |
| `PlayOutAnimation()` | 離脱アニメーション。`ResetOutAnimation()` で初期化 |
| `Dispose()` | クローズ後の GameObject 破棄 |

---

### Suspend / Resume

`ScreenStackModuleSceneBase` を継承したシーンを使うと、シーン遷移をまたいでスタックの状態が保持されます。

- **Forward 遷移（前進）**: 現在のスタックを `Suspend`（保存）してから離脱
- **Back 遷移（戻る）**: 入場時に保存されたスタックを `Resume`（復元）

これにより「ダイアログを開いたまま別シーンへ遷移し、戻ってきたらダイアログが復元されている」という動作を実現できます。

---

### ScreenStackModuleProxy

DI スコープをまたいでスタック API にアクセスするためのプロキシクラスです。

- `ScreenStackModuleProxy`（シーンスコープ）が `ScreenStackModule`（モジュールスコープ）への参照を保持
- シーンスコープから `IScreenStackModule` として注入を受けた側は、スコープの違いを意識せず利用できます

---

### コード生成

エディターメニューからダイアログのボイラープレートコードを自動生成できます。

- **ScreenStackDialogScriptGeneratorWindow**: ダイアログの View / Data クラスをテンプレートから生成
- **ScreenStackEntityFactoryGenerator**: `IScreenStackEntityFactory` の switch 分岐を自動生成

---

## Addressable

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.addressable`  
**依存パッケージ:** UniTask · Unity Addressables

参照カウント方式の Addressables ラッパーです。スコープ単位のアセットライフタイム管理と並列ロードをサポートします。アドレスごとに参照カウントを管理し、同一アドレスを複数のスコープが共有する場合は同じ `AsyncOperationHandle` を再利用します。最後のハンドルが解放されたときに初めて `Addressables.Release` が呼ばれます。

### IAssetManager

DI に `AssetManager` を登録し、`IAssetManager` として注入します。`CreateScope()` を呼ぶとアセットロード用の `IAssetScope` が得られます。

```csharp
// VContainer の例
builder.Register<AssetManager>(Lifetime.Singleton).As<IAssetManager>();
```

### IAssetScope

シーンごと（または論理的なロードコンテキストごと）にスコープを1つ作成します。スコープを Dispose すると、取得済みのすべてのハンドルが一括解放されます。

```csharp
IAssetScope scope = assetManager.CreateScope();

// アドレスを指定して単体ロード
IAssetHandle<Sprite> handle = await scope.LoadAsync<Sprite>("ui/icon_home", ct);
icon.sprite = handle.Asset;

// アドレスリストを指定して複数ロード（順次）
IReadOnlyList<IAssetHandle<Sprite>> handles =
    await scope.LoadAsync<Sprite>(new[] { "ui/icon_a", "ui/icon_b" }, ct);

// ラベルに一致する全アセットをロード
IReadOnlyList<AudioClip> clips = await scope.LoadByLabelAsync<AudioClip>("bgm", ct);

// スコープが保持する全ハンドルを解放
scope.Dispose();
```

| メソッド | 説明 |
|---|---|
| `LoadAsync<T>(string address, ct)` | アドレスを指定して単体ロード。`IAssetHandle<T>` を返す |
| `LoadAsync<T>(IReadOnlyList<string> addresses, ct)` | アドレスリストを指定して順次ロード。`IReadOnlyList<IAssetHandle<T>>` を返す |
| `LoadByLabelAsync<T>(string label, ct)` | ラベルに一致する全アセットをロード。`IReadOnlyList<T>` を返す |
| `TryLoadAsync(ParallelLoadData data, ct)` | 異なる型のアセットを並列ロード。一部失敗を許容。`ParallelLoadResult` を返す |
| `Dispose()` | スコープが取得した全ハンドルを解放 |

### IAssetHandle\<T\>

| メンバー | 説明 |
|---|---|
| `Asset` | ロード済みの `UnityEngine.Object` |
| `IsDisposed` | `Dispose()` 後は `true` |
| `Dispose()` | 参照カウントをデクリメント。カウントが 0 になると `Addressables.Release` が呼ばれる |

スコープを Dispose する前に個別ハンドルを先に Dispose することで、早期にメモリを解放できます。

### 並列ロード

`TryLoadAsync` はすべてのロードを同時に開始し、結果をまとめて返します。一つのリクエストが失敗しても他のリクエストはキャンセルされません。

```csharp
var data = new ParallelLoadData();
var iconReq  = data.Add<Sprite>("ui/icon_home");
var bgReq    = data.Add<Texture2D>("ui/background");
var audioReq = data.Add<AudioClip>("audio/bgm_home");

ParallelLoadResult result = await scope.TryLoadAsync(data, ct);

if (result.IsSuccess(iconReq))
    icon.sprite = result.Get(iconReq).Asset;

if (result.IsSuccess(bgReq))
    background.texture = result.Get(bgReq).Asset;

if (!result.IsSuccess(audioReq))
    Debug.LogWarning("BGM のロードに失敗しました");
```

| メンバー | 説明 |
|---|---|
| `ParallelLoadData.Add<T>(string address)` | ロードリクエストを登録。`AssetRequest<T>` トークンを返す |
| `ParallelLoadResult.IsSuccess<T>(request)` | リクエストが成功したかどうか |
| `ParallelLoadResult.Get<T>(request)` | 成功した場合は `IAssetHandle<T>` を、失敗した場合は `null` を返す |

### キャンセルの挙動

`CancellationToken` を渡すと **await** がキャンセルされますが、同一ハンドルを他の呼び出し元が共有している可能性があるため、Addressables の内部ロードはキャンセルされません。トークンが発火すると `OperationCanceledException` がスローされ、そのアドレスの参照カウントがデクリメントされます。ハンドルはスコープに追加されません。

---

## Animation

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.animation`  
**依存パッケージ:** なし（コアパッケージ不要で単体利用可）

`PlayableGraph` 経由で `AnimationClip` を再生する In/Out アニメーションコンポーネント群です。

### LHTransitionAnimator

シーンやダイアログの InAnimation / OutAnimation フックに接続して使うコンポーネントです。

```csharp
// SceneBase.InAnimation をオーバーライドして呼び出す例
protected override UniTask InAnimation(ISceneTransitionContext context)
    => transitionAnimator.InAnimation();

protected override UniTask OutAnimation(ISceneTransitionContext context)
    => transitionAnimator.OutAnimation();
```

| インスペクター設定 | 説明 |
|---|---|
| `inAnimationClips` | 入場時に順番に再生する AnimationClip 配列 |
| `inDelayMilliSec` | 入場アニメーション開始前の待機時間（ミリ秒） |
| `outAnimationClips` | 離脱時に順番に再生する AnimationClip 配列 |
| `outDelayMilliSec` | 離脱アニメーション開始前の待機時間 |

In と Out は排他制御されており、一方が再生中に他方を呼ぶと自動で停止します。

### LHSceneTransitionAnimator / LHSceneTransitionAnimatorManager

複数のシーンにまたがるより複雑なトランジション演出を管理するコンポーネントです。`LHSceneTransitionAnimatorManager` が複数の `LHSceneTransitionAnimator` を束ねて制御します。

---

## Language

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.language`  
**依存パッケージ:** R3

アプリの言語を切り替えるサービスです。

### 使い方

```csharp
// 言語を切り替える（登録済みのハンドラーが並列実行されてから CurrentLanguage が更新される）
await languageService.SetLanguage("ja", cancellationToken);

// 現在の言語を R3 の ReactiveProperty として購読する
languageService.CurrentLanguage.Subscribe(lang => Debug.Log(lang));

// 言語変更前に呼ばれるハンドラーを登録する（フォントやテキストの事前ロードに使う）
languageService.RegisterChangeHandler(async (lang, ct) =>
{
    await LoadResourcesForLanguage(lang, ct);
});
```

`SetLanguage` は、登録されたすべてのハンドラーが完了してから `CurrentLanguage` を更新します。これにより、フォントやテキストリソースのロードが完了してから UI に反映されることが保証されます。

### SupportedLanguageSettings

インスペクターで管理する ScriptableObject です。対応言語コードの一覧とデフォルト言語を設定します。

```
Create > Lighthouse > Language > Supported Language Settings
```

| 設定 | 説明 |
|---|---|
| `supportedLanguages` | 有効な言語コード一覧（例: `["ja", "en", "ko", "zh"]`） |
| `defaultLanguage` | 初期起動時に使用する言語コード（例: `"en"`） |

### ISupportedLanguageService

`SupportedLanguageSettings` をラップし、DI 経由で利用できるサービスインターフェースです。

---

## Font

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.font`  
**依存パッケージ:** `com.lisearcheleeds.lighthouse-extends.language` · TextMeshPro

言語に応じた `TMP_FontAsset` を自動で切り替えるサービスです。

### 使い方

```csharp
// 現在のフォントを購読する
fontService.CurrentFont.Subscribe(font => myText.font = font);

// 特定言語のフォントを直接取得する
TMP_FontAsset font = fontService.GetFont("ja");
```

`FontService` は `ILanguageService` に変更ハンドラーを登録しており、`SetLanguage` が呼ばれると自動で `CurrentFont` が更新されます。

### LanguageFontSettings

言語コードと `TMP_FontAsset` のマッピングを管理する ScriptableObject です。

```
Create > Lighthouse > Font > Language Font Settings
```

| 設定 | 説明 |
|---|---|
| `entries` | 言語コードと FontAsset のペアのリスト |
| `defaultFont` | 対応エントリがない言語コードで使われるフォント |

---

## TextTable

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.texttable`  
**依存パッケージ:** `com.lisearcheleeds.lighthouse-extends.language`

言語ごとの TSV ファイルをロードし、キーからテキストを解決するローカライズサービスです。

### 使い方

```csharp
// ITextData を渡してテキストを取得（{param} プレースホルダーも解決される）
string text = textTableService.GetText(new TextData("HomeTitle"));

// パラメーター付きの例（TSV 側: "残り{count}回" → 実行時: "残り3回"）
var data = new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 });
string text = textTableService.GetText(data);
```

### ITextTableLoader

TSV の実際のロード処理を抽象化するインターフェースです。プロジェクト側で実装して DI に登録します。

```csharp
public class MyTextTableLoader : ITextTableLoader
{
    public async UniTask<IReadOnlyDictionary<string, string>> LoadAsync(
        string languageCode, CancellationToken ct)
    {
        // Resources や Addressables から TSV をロードしてパースして返す
    }
}
```

### TextData.CreateTextData（エディター専用）

エディター上でキーを新規追加しながら実装する際の補助メソッドです。

```csharp
// エディター専用: このコードを書いて Lighthouse の拡張機能を実行すると、
// TSV にエントリが追記され、このコード自体が new TextData("HomeTitle") に書き換えられる
var data = TextData.CreateTextData("Home", "HomeTitle", "ようこそ");
```

### エディターウィンドウ

`Window > Lighthouse > TextTable Editor` からウィンドウを開くと：

- シーンや Prefab をまたいで全キーと翻訳テキストを一覧・編集できる
- 重複キーや未翻訳キーの検出ウィンドウも提供される

---

## TextMeshPro

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.textmeshpro`  
**依存パッケージ:** `com.lisearcheleeds.lighthouse-extends.texttable` · `com.lisearcheleeds.lighthouse-extends.font`

`TextMeshProUGUI` を継承した拡張コンポーネントで、言語切り替えに応じてテキストとフォントを自動更新します。

### 使い方

**インスペクターで設定する場合:**

`LHTextMeshPro` コンポーネントの **Text Key** フィールドにキー文字列を入力するだけです。言語が変わると `TextTableService` からテキストを取得して自動更新されます。

**コードで設定する場合:**

```csharp
// ITextData を渡してテキストを設定する
lhText.SetTextData(new TextData("HomeTitle"));

// パラメーター付き
lhText.SetTextData(new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 }));
```

`TextTableService` が登録されていない場合はキー文字列をそのまま表示します。`FontService` が登録されていない場合はフォントの自動更新が行われません（個別に設定したフォントが維持されます）。

---

## InputLayer

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.inputlayer`  
**依存パッケージ:** Input System

`InputActionAsset` 上でスタック型の入力ディスパッチを提供します。最前面のレイヤーから順にイベントを渡し、消費またはブロックされたら下のレイヤーには届きません。

### 使い方

```csharp
// レイヤーをプッシュ（ActionMap が自動で有効化される）
inputLayerController.PushLayer(myLayer, myActionMap);

// 最前面のレイヤーをポップ
inputLayerController.PopLayer();

// 特定のレイヤーを指定してポップ
inputLayerController.PopLayer(myLayer);
```

### IInputLayer の実装

```csharp
public class HomeInputLayer : IInputLayer
{
    // true にすると下位レイヤーへのイベント伝播をすべて遮断する
    public bool BlocksAllInput => false;

    public bool OnActionStarted(InputAction.CallbackContext ctx)
    {
        // true を返すとそのイベントを消費（下位レイヤーには渡らない）
        return false;
    }

    public bool OnActionPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.action.name == "Back")
        {
            // 戻るボタン処理
            return true; // 消費
        }
        return false;
    }

    public bool OnActionCanceled(InputAction.CallbackContext ctx) => false;
}
```

| 動作 | 説明 |
|---|---|
| `return true` | イベントを消費。下位レイヤーには届かない |
| `return false` | イベントを通過させる。下位レイヤーに渡る |
| `BlocksAllInput = true` | 消費/非消費に関わらず、下位レイヤーへの伝播をすべて遮断 |

同じ `ActionMap` を参照するレイヤーが残っている場合、ポップ時に `ActionMap` は無効化されません。

---

## UIComponent

**パッケージ:** `com.lisearcheleeds.lighthouse-extends.uicomponent`  
**依存パッケージ:** なし（コアパッケージ不要で単体利用可）

UI に関する汎用コンポーネント群です。

### LHButton

`UnityEngine.UI.Button` を継承したボタンコンポーネントです。`ExclusiveInputService` と連携し、マルチタッチ環境での同時タップを防ぎます。

- 同時に複数の `LHButton` が押された場合、最初のタップのみ有効になります
- アプリがバックグラウンドになった場合、押下状態を自動でリセットします
- `ExclusiveInputService` が未登録の場合は通常の `Button` と同じ動作をします

### ExclusiveInputService

ポインター ID を排他管理するサービスです。`LHButton` が自動で利用するため、通常は直接操作不要です。
ボタン以外にも入力の排他制御を行いたい場合はこちらを利用します。

### LHRaycastTargetObject

描画を行わずレイキャストのみを受け付ける透明な `Graphic` コンポーネントです。ボタンなどのヒット領域を画像なしで定義したい場合に使います。

`CanvasRenderer` コンポーネントが必要です（自動でアタッチされます）。

### LHCanvasSceneObject

Canvas とシーンカメラの紐付けを管理するコンポーネントです。`CanvasMainSceneBase` と組み合わせて使います。
下記のUnityEditor編集中にEditorOnly属性で生成される `CanvasSceneEditorOnlyObject`にPrefabとして内包することで、
Canvas/Cameraそのものはシーンに保存せずにランタイムと同じCanvas/Cameraの設定でプレビューすることが出来ます。

### DefaultCanvasSceneEditorOnlyObject

エディター上でのみ表示されるオブジェクトを管理するコンポーネントです。
`IEditorOnlyObjectCanvasScene` を実装したコンポーネントで、Canvas/Cameraそのものはシーンに保存せずにランタイムと同じCanvas/Cameraの設定でプレビューすることが出来ます。
