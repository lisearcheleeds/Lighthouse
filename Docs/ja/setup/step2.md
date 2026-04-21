# Step 2: ScreenStack の設定

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

> ScreenStack を使用しない場合はこの Step をスキップしてください。

Unity メニューから **Lighthouse > Settings > ScreenStackGenerateSettings** を選択します。

`Assets/Settings/Lighthouse/ScreenStackGenerateSettings.asset` が生成されます。

Inspector で以下の項目を設定します。

| フィールド | 説明 |
|---|---|
| Screen Stack Entity Factory Directory | `ScreenStackEntityFactory` の出力先フォルダ |
| Screen Stack Entity Factory Class Name | 生成クラス名（デフォルト: `ScreenStackEntityFactory`） |
| Screen Stack Entity Factory Namespace | 生成クラスの名前空間 |
| Screen Stack Entity Factory Template | ファクトリ生成テンプレートアセット |
| Screen Stack Dialog Script Output Directory | ダイアログスクリプトの出力先フォルダ |
| Screen Stack Dialog Script Namespace | ダイアログスクリプトの名前空間 |
| Screen Stack Script Templates | ダイアログ生成テンプレートアセット |
