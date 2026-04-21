# Step 6: シーンファイルの作成と Build Settings への登録

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

### シーンファイルの命名規則

Lighthouse の `SceneIdGenerator` は、Build Settings に登録されたシーンのパスに含まれるキーワードでシーンの種別を判別します。

| 種別 | パスに含まれるキーワード | 例 |
|---|---|---|
| メインシーン | `MainScene` | `Assets/.../Scene/MainScene/Home/Home.unity` |
| モジュールシーン | `ModuleScene` | `Assets/.../Scene/ModuleScene/Background/Background.unity` |

> シーンファイルやその親フォルダの名前に `MainScene` / `ModuleScene` が含まれるように配置してください。

### Build Settings への登録

1. Unity メニューから **File > Build Settings** を開きます
2. シーンファイルを **Scenes In Build** リストにドラッグ＆ドロップします
3. `Launcher` シーンを **index 0** に配置します

```
Scenes In Build:
  0: Assets/.../Scene/Launcher.unity          ← 必ず先頭
  1: Assets/.../Scene/MainScene/Splash/Splash.unity
  2: Assets/.../Scene/MainScene/Home/Home.unity
  3: Assets/.../Scene/ModuleScene/Background/Background.unity
  ...
```

> Build Settings に変更を加えると、`SceneIdGenerator` が自動的に `MainSceneId` / `ModuleSceneId` クラスを再生成します。

### 手動生成

自動生成が走らない場合は、以下のメニューから手動で実行できます。

- **Lighthouse > Generate > Auto > Generate "MainSceneId" manually**
- **Lighthouse > Generate > Auto > Generate "ModuleSceneId" manually**
