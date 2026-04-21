# Step 1: 設定ファイルの生成と初期設定

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

### 1-1. GenerateSettings の生成

Unity メニューから **Lighthouse > Settings > GenerateSettings** を選択します。

`Assets/Settings/Lighthouse/GenerateSettings.asset` が生成されます。

Inspector で以下の項目を設定します。

| フィールド | 説明 |
|---|---|
| Product Name Space | 生成されるクラスのルート名前空間（例: `SampleProduct`） |
| Generated File Directory | 生成ファイルの出力先フォルダ（後述の `LighthouseGenerated` フォルダを指定） |
| Main Scene Id Prefix | MainSceneId クラスのプレフィックス（例: `Sample`） |
| Module Scene Id Prefix | ModuleSceneId クラスのプレフィックス（例: `Sample`） |
| Scene Script Output Directory | シーンスクリプトの出力先フォルダ |
| Scene Script Templates | シーン生成に使用するテンプレートアセット |

### 1-2. LighthouseGenerated フォルダの作成

プロダクト側の Assets 以下に `LighthouseGenerated` フォルダを作成します。

```
Assets/
└── {YourProductFolder}/
    └── LighthouseGenerated/   ← ここに作成
```

作成後、`GenerateSettings` の **Generated File Directory** にこのフォルダをドラッグ＆ドロップして指定します。

### 1-3. SceneEditSettings の生成

Unity メニューから **Lighthouse > Settings > SceneEditSettings** を選択します。

`Assets/Settings/Lighthouse/SceneEditSettings.asset` が生成されます。

> **補足:** `SceneEditSettings` はエディター起動時に自動生成されることもあります。  
> 既にファイルが存在する場合は上書きされません。

Inspector で以下の項目を確認・設定します。

| フィールド | 説明 | デフォルト |
|---|---|---|
| Enable Scene Edit Process | シーン編集時の自動更新の有効/無効 | `true` |
| Canvas Scene Editor Only Object | Canvasシーン編集時のエディター専用オブジェクトの Prefab（後述） | なし |
| Editor Only Object Name | シーンに配置されるオブジェクト名 | `__EditorOnly__` |

### 1-4. CanvasSceneEditorOnlyObject Prefab の作成と設定

エディター上でシーンを編集する際に、Canvas の動作を補助するための Prefab を作成します。

**Prefab の作成手順:**

1. 任意のフォルダに空の GameObject を作成し、Prefab 化します
2. Prefab に以下の 2 つのコンポーネントをアタッチします
   - `LHCanvasSceneObject` — カメラ（SceneCamera）と EventSystem を持つコンポーネント
   - `DefaultCanvasSceneEditorOnlyObject` — `IEditorOnlyObjectCanvasScene` の標準実装
3. `DefaultCanvasSceneEditorOnlyObject` の **Lh Canvas Scene Object** フィールドに、同じ GameObject の `LHCanvasSceneObject` を設定します
4. `LHCanvasSceneObject` の **UI Camera** および **UI Event System** に適切なコンポーネントを設定します

**SceneEditSettings への登録:**

作成した Prefab を `SceneEditSettings` の **Canvas Scene Editor Only Object** フィールドに設定します。

> この Prefab は、`ICanvasSceneBase` を実装したシーンがエディターで開かれると自動的にシーンに配置され、再生時や保存時には自動的に除去されます。
