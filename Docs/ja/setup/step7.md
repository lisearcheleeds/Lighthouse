# Step 7: シーンスクリプトの生成

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

Lighthouse にはシーンのボイラープレートを生成するエディターウィンドウが付属しています。

**Lighthouse > Generate > Open Scene scripts generator** を開き、以下を設定して **Generate** を押します。

| 項目 | 説明 |
|---|---|
| Scene Type | `MainScene` または `ModuleScene` |
| Template Preset | `GenerateSettings` に登録したテンプレート |
| Base Class | 継承元の基底クラス（例: `CanvasMainSceneBase`） |
| Scene Name | シーン名（例: `Home`） |

`GenerateSettings` の **Scene Script Output Directory** に指定したフォルダに、シーンスクリプトが生成されます。

> テンプレートをカスタマイズすることで、プロダクト共通の基底クラスを継承したスクリプトを生成できます。  
> 詳細は `GenerateSettings` の **Scene Script Templates** を参照してください。
