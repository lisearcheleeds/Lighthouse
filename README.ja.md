[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
![Version](https://img.shields.io/badge/version-1.0.0-brightgreen)

**[English](README.md) | [한국어](README.ko.md) | [中文](README.zh.md)**

# Lighthouse

[VContainer](https://github.com/hadashiA/VContainer) と [UniTask](https://github.com/Cysharp/UniTask) を基盤とした、
構造化されたシーン管理システム・ダイアログスタック・拡張可能なランタイムモジュール群を提供する Unity アプリケーションフレームワークです。

現在のバージョンは **1.0.0** です。

<img width="1677" height="938" alt="lighthouse" src="https://github.com/user-attachments/assets/f0e9c5de-f858-4e0d-be63-7e57a4d6558c" />

---

## 動作要件

| 依存ライブラリ | バージョン |
|---|---|
| Unity | 6000.0 以降 (URP) |
| VContainer | 1.17.0 以降 |
| UniTask | 2.5.10 以降 |
| R3 | 1.3.0 以降 |
| TextMeshPro | 3.x |
| Input System | 1.17.0 以降 |

---

## リポジトリ

| リポジトリ | 説明 |
|---|---|
| [Lighthouse](https://github.com/lisearcheleeds/Lighthouse) | Unity アーキテクチャフレームワーク |
| [LighthouseSample](https://github.com/lisearcheleeds/LighthouseSample) | フレームワークの使い方を示すサンプルプロジェクト |
| [LighthouseQuickPackage](https://github.com/lisearcheleeds/LighthouseQuickPackage) | 設定済みの空プロジェクト |

---

## WebGL デモンストレーション
https://lisearcheleeds.github.io/LighthouseSample/

---

## インストール

### 方法 1: Unity Package Manager — Git URL

`Packages/manifest.json` を開き、必要なパッケージを追加します：

```json
{
  "dependencies": {
    "com.lisearcheleeds.lighthouse": "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/Lighthouse#v1.0.0",

    "com.lisearcheleeds.lighthouse-extends.animation":   "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/Animation#v1.0.0",
    "com.lisearcheleeds.lighthouse-extends.inputlayer":  "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/InputLayer#v1.0.0",
    "com.lisearcheleeds.lighthouse-extends.language":    "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/Language#v1.0.0",
    "com.lisearcheleeds.lighthouse-extends.font":        "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/Font#v1.0.0",
    "com.lisearcheleeds.lighthouse-extends.texttable":   "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/TextTable#v1.0.0",
    "com.lisearcheleeds.lighthouse-extends.textmeshpro": "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/TextMeshPro#v1.0.0",
    "com.lisearcheleeds.lighthouse-extends.uicomponent": "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/UIComponent#v1.0.0",
    "com.lisearcheleeds.lighthouse-extends.screenstack": "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/ScreenStack#v1.0.0",
    "com.lisearcheleeds.lighthouse-extends.addressable": "https://github.com/lisearcheleeds/Lighthouse.git?path=Client/Assets/LighthouseExtends/Addressable#v1.0.0"
  }
}
```

> **注意:** Git URL 方式では依存ライブラリ（UniTask・VContainer・R3 など）は自動解決されません。手動で追加するか [OpenUPM](https://openupm.com) を利用してください。

### 方法 2: OpenUPM _(近日対応予定)_

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": ["com.lisearcheleeds", "com.cysharp", "jp.hadashikick"]
    }
  ],
  "dependencies": {
    "com.lisearcheleeds.lighthouse": "1.0.0"
  }
}
```

### 方法 3: UnityPackage

[Releases](https://github.com/lisearcheleeds/Lighthouse/releases) から最新の `.unitypackage` をダウンロードし、`Assets > Import Package > Custom Package` からインポートしてください。

---

## 機能

### シーンシステム (`Lighthouse`)

- **シーンのスタック管理** - シーンは遷移するたびにスタックとして積まれ、前の画面に戻るという機能を提供しています
- **メイン + モジュールシーンモデル** — ゲーム画面は1つ以上のメインシーンと複数のモジュールシーンを組み合わせて表現されます。ヘッダーや背景といった共通UIはそのままに、画面中央のコンテンツのみ入れ替えるといった表現が可能です
- **シーンキャッシュ** - シーンは `SceneGroup` という単位でロード/アンロードされます。シーンに遷移しようとした時、同じ `SceneGroup` に所属している他のメインシーンとモジュールシーンも同時に読み込まれてキャッシュされます。 この `SceneGroup` はプロダクト側で定義するため、単一のSceneGroupを定義することでユーザー体験とパフォーマンスのバランスを調整出来ます
- **フェーズ / ステップ パイプライン** — シーン遷移は順序付きフェーズ（例: `OutAnimationPhase`、`LoadSceneGroupPhase`、`EnterScenePhase`）で構成され、各フェーズは並列ステップを持ちます。シーケンスはプロダクト側で `ISceneTransitionSequenceProvider` を定義することで差し替え可能です
- **遷移タイプ** — `Exclusive`（アウト/インアニメーション付き切り替え）と `Cross`（クロスフェード）が組み込まれています。`Auto` はシーングループに基づいて適切なタイプを自動選択します
- **シーン基底クラス** — `MainSceneBase<TTransitionData>` と `ModuleSceneBase` がアクティブ化・アニメーションフック・Canvas インジェクションを処理します。Canvas バリアント（`CanvasMainSceneBase`、`CanvasModuleSceneBase`）は CanvasGroup のアルファ制御を追加します
- **カメラスタック管理** — `SceneCameraManager` が各遷移時に URP カメラスタックを再構築し、Base/Overlay ロールと深度順を自動で割り当てます
- **戻るナビゲーション** — `SceneManager` が遷移履歴スタックを保持し、`CanBackTransition = false` のエントリをスキップしながら正しい戻り先を解決します

### スクリーンスタック (`LighthouseExtends.ScreenStack`)

- **ダイアログ / オーバーレイ管理** — `ScreenStackBase` を実装したスクリーンがスタックにプッシュされます。Open / Close / ClearAll 操作はキューに入り順番に実行されます。
- **System / Default レイヤー** — 各スクリーンやオーバーレイは `IScreenStackData.IsSystem` によってシステム Canvas レイヤーまたはデフォルトレイヤーに配置されます。
- **入力ブロック** — `ScreenStackBackgroundInputBlocker` が最前面のスクリーンの背後にフルスクリーンのレイキャストターゲットを配置し、タッチスルーを防止します。
- **シーンのサスペンド & リジューム** — `ScreenStackModuleSceneBase` がシーン遷移イベントにフックし、前進離脱時にスタックをサスペンド、戻る入場時にリジュームしてシーン遷移をまたいだ状態を保持します。
- **プロキシパターン** — `ScreenStackModuleProxy`（シーンスコープ）が `ScreenStackModule`（モジュールスコープ）へ橋渡しし、任意の DI スコープからスタック API にアクセスできます。
- **コード生成** — `ScreenStackDialogScriptGeneratorWindow` と `ScreenStackEntityFactoryGenerator` がテンプレートからダイアログスクリプトのボイラープレートと `ScreenStackEntityFactory` スイッチテーブルを生成します。

### 拡張モジュール (`LighthouseExtends`)

| モジュール | 説明 |
|---|---|
| **Addressable** | `AssetManager` — 参照カウント方式の Addressables ラッパー。スコープ単位のアセットライフタイム管理と並列ロードをサポートします。 |
| **Animation** | `LHTransitionAnimator` / `LHSceneTransitionAnimator` — `PlayableGraph` 経由の In/Out クリップ再生。方向ごとの開始ディレイと排他制御をサポート。 |
| **Language** | `LanguageService` — リアクティブな言語切り替え。`CurrentLanguage` 更新前に登録済みハンドラーが並列で呼び出されます。 |
| **Font** | `FontService` — `ILanguageService` を購読し、言語変更のたびに `LanguageFontSettings` から `CurrentFont`（TMP_FontAsset）を更新します。 |
| **TextTable** | `TextTableService` — `ITextTableLoader` 経由で言語ごとの TSV ファイルをロードし、実行時に `{param}` プレースホルダーを解決します。シーンや Prefab をまたいでキーと翻訳を編集できるエディターウィンドウを付属します。 |
| **TextMeshPro** | `LHTextMeshPro` — `TextMeshProUGUI` を拡張し、`TextTableService` と `FontService` を通じて言語・フォントの変更を自動反映します。 |
| **InputLayer** | `InputLayerController` — `InputActionAsset` 上のスタックベースの入力ディスパッチ。レイヤーはイベントを消費したり `BlocksAllInput` で下位レイヤーをすべてブロックできます。 |

---

## アーキテクチャ

```
LighthouseArchitecture/
├── Lighthouse/                     # コアフレームワーク
│   └── Scene/
│       ├── SceneBase/              # シーン用 MonoBehaviour 基底クラス
│       ├── SceneCamera/            # URP カメラスタック管理
│       ├── SceneTransitionPhase/   # 遷移フェーズ定義
│       ├── SceneTransitionStep/    # 個別の非同期ステップ
│       └── *.cs                    # SceneManager、コンテキスト、データモデル
│
└── LighthouseExtends/              # オプションのランタイム & エディターモジュール
    ├── Addressable/
    ├── Animation/
    ├── Font/
    ├── InputLayer/
    ├── Language/
    ├── ScreenStack/
    ├── TextMeshPro/
    └── TextTable/
```

**依存関係・方向**

```
ゲームコード
    └── Lighthouse (ISceneManager, IScreenStackModule, ILanguageService, ...)
            └── LighthouseExtends (Animation, Font, TextTable, ScreenStack, ...)
```

---

## 実装の一部を紹介

### シーンの定義

```csharp
// 1. 遷移データクラスの定義
public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;
}

// 2. シーンの実装
public class HomeScene : MainSceneBase<HomeTransitionData>
{
    protected override UniTask OnEnter(HomeTransitionData data,
        ISceneTransitionContext context, CancellationToken ct)
    {
        // UI の初期化
        return UniTask.CompletedTask;
    }
}

// 3. 遷移
await sceneManager.TransitionScene(new HomeTransitionData());
```

### ダイアログを開く

```csharp
// スクリーンを開いてデータを渡す
await screenStackModule.Open(new SampleDialogData());

// 最前面のスクリーンを閉じる
await screenStackModule.Close();
```

### 言語の切り替え

```csharp
// TextTableService と FontService が自動的に更新されます
// LHTextMeshProはイベントを受け取り自動的に予め設定している適切なフォント・言語に切り替わります
await languageService.SetLanguage("ja", cancellationToken);
```

### テキストコンポーネントのローカライズ

Inspector で任意の `LHTextMeshPro` コンポーネントに **Text Key** を割り当てます。  
実行時に言語が変わるとテキストとフォントが自動的に更新されます。

コードからキーを登録する場合：

```csharp
// エディター専用: TSV エントリを生成し new TextData("key") に書き換えます
var data = TextData.CreateTextData("Home", "HomeTitle", "ようこそ");
lhText.SetTextData(data);
```

### 機能の拡張
これらはすべてDIで管理されており、プロジェクトで機能の取捨選択、全てもしくは一部の拡張が可能です。

---

## ガイド

| 言語 | リンク |
|---|---|
| English | [Docs/en](Docs/en/readme.md) |
| 日本語 | [Docs/ja](Docs/ja/readme.md) |
| 한국어 | [Docs/ko](Docs/ko/readme.md) |
| 中文 | [Docs/zh](Docs/zh/readme.md) |

---

## 作者

**Lise** — [GitHub](https://github.com/lisearcheleeds) / [X](https://x.com/archeleeds)

---

## ライセンス

このプロジェクトは [MIT ライセンス](LICENSE) のもとで公開されています。
