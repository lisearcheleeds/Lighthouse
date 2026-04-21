# Lighthouse — セットアップガイド

パッケージを Unity プロジェクトに導入した後、以下の手順で初期設定を行ってください。

## スターターキット（UnityPackage）

セットアップの大部分を省略できる UnityPackage を用意しています。  
[TODO: UnityPackage URL]

### 省略できるステップ

| Step | 省略 | 備考 |
|---|---|---|
| Step 1: 設定ファイルの生成と初期設定 | ほぼ省略可 | `ProductNameSpace`・`MainSceneIdPrefix`・`ModuleSceneIdPrefix`・出力ディレクトリは自分のプロジェクトに合わせて変更が必要 |
| Step 2: ScreenStack の設定 | ほぼ省略可 | namespace と出力ディレクトリの変更のみ必要 |
| Step 3: TextTable の設定 | 完全省略可 | デフォルトパスのままであれば変更不要 |
| Step 4: VContainer LifetimeScope の実装 | 完全省略可 | スクリプト・Prefab・ScriptableObject・VContainerSettings すべて提供済み |
| Step 5: Launcher シーンと Launcher の実装 | 完全省略可 | Launcher.unity・Launcher.cs 提供済み |
| Step 6: Build Settings への登録 | 必要 | Build Settings は常に手動 |
| Step 7: シーンスクリプトの生成 | 必要 | ゲーム固有の作業 |
| Step 8: LifetimeScope と基底クラスの実装 | 必要 | 共通基底クラスは提供済み。シーンごとの LifetimeScope と SceneGroupProvider のカスタマイズは必要 |

---

## 目次

- [Step 1: 設定ファイルの生成と初期設定](./setup/step1.md)
- [Step 2: ScreenStack の設定](./setup/step2.md)
- [Step 3: TextTable の設定](./setup/step3.md)
- [Step 4: VContainer LifetimeScope の実装](./setup/step4.md)
- [Step 5: Launcher シーンと Launcher の実装](./setup/step5.md)
- [Step 6: シーンファイルの作成と Build Settings への登録](./setup/step6.md)
- [Step 7: シーンスクリプトの生成](./setup/step7.md)
- [Step 8: シーンの LifetimeScope と基底クラスの実装](./setup/step8.md)
