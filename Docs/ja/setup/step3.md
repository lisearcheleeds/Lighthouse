# Step 3: TextTable の設定

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

> TextTable を使用しない場合はこの Step をスキップしてください。

`TextTableEditorSettings` は TextTable エディターウィンドウを開いたときに自動生成されます。

Unity メニューから **Lighthouse > TextTable > ...** を選択してウィンドウを開くと、  
`Assets/Settings/Lighthouse/TextTableEditorSettings.asset` が生成されます。

Inspector で以下の項目を確認・変更します。

| フィールド | 説明 | デフォルト |
|---|---|---|
| Text Table Folder Path | TSV ファイルの配置先パス | `Assets/StreamingAssets/TextTables` |
