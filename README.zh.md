[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
![Version](https://img.shields.io/badge/version-1.0.0-brightgreen)

**[English](README.md) | [日本語](README.ja.md) | [한국어](README.ko.md)**

# Lighthouse

基于 [VContainer](https://github.com/hadashiA/VContainer) 和 [UniTask](https://github.com/Cysharp/UniTask) 构建的 Unity 应用框架，提供结构化的场景管理系统、对话框栈以及可扩展的运行时模块。

当前版本为 **1.0.0**。

<img width="1677" height="938" alt="lighthouse" src="https://github.com/user-attachments/assets/f0e9c5de-f858-4e0d-be63-7e57a4d6558c" />

---

## 环境要求

| 依赖库 | 版本 |
|---|---|
| Unity | 6000.0 或更高 (URP) |
| VContainer | >= 1.17.0 |
| UniTask | >= 2.5.10 |
| R3 | >= 1.3.0 |
| TextMeshPro | 3.x |
| Input System | >= 1.17.0 |

---

## 仓库

| 仓库 | 说明 |
|---|---|
| [Lighthouse](https://github.com/lisearcheleeds/Lighthouse) | Unity 架构框架 |
| [LighthouseSample](https://github.com/lisearcheleeds/LighthouseSample) | 展示框架用法的示例项目 |
| [LighthouseQuickPackage](https://github.com/lisearcheleeds/LighthouseQuickPackage) | 预配置的空项目 |

---

## WebGL 演示
https://lisearcheleeds.github.io/LighthouseSample/

---

## 安装

### 方式 1: Unity Package Manager — Git URL

打开 `Packages/manifest.json`，添加所需的包：

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

> **注意:** 使用 Git URL 方式时，依赖库（UniTask、VContainer、R3 等）不会自动安装。请手动添加或通过 [OpenUPM](https://openupm.com) 安装。

### 方式 2: OpenUPM _(即将支持)_

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

### 方式 3: UnityPackage

从 [Releases](https://github.com/lisearcheleeds/Lighthouse/releases) 下载最新的 `.unitypackage`，通过 `Assets > Import Package > Custom Package` 导入。

---

## 功能

### 场景系统 (`Lighthouse`)

- **场景栈管理** — 每次切换场景时，场景都会被压入栈中，提供返回上一个画面的功能。
- **主场景 + 模块场景模型** — 游戏画面由一个或多个主场景与多个模块场景组合表现。可以在保持页眉、背景等共用 UI 不变的同时，仅替换画面中央的内容。
- **场景缓存** — 场景以 `SceneGroup` 为单位进行加载/卸载。切换到某个场景时，属于同一 `SceneGroup` 的其他主场景和模块场景也会同时加载并缓存。由于 `SceneGroup` 在产品侧定义，可通过配置单一 SceneGroup 来平衡用户体验与性能。
- **阶段 / 步骤管道** — 场景切换由有序的阶段（如 `OutAnimationPhase`、`LoadSceneGroupPhase`、`EnterScenePhase`）组成，每个阶段包含并行步骤。可在产品侧定义 `ISceneTransitionSequenceProvider` 来替换序列。
- **切换类型** — 内置 `Exclusive`（带出/入动画的切换）和 `Cross`（淡入淡出）。`Auto` 根据场景组自动选择合适的类型。
- **场景基类** — `MainSceneBase<TTransitionData>` 和 `ModuleSceneBase` 处理激活、动画钩子和 Canvas 注入。Canvas 变体（`CanvasMainSceneBase`、`CanvasModuleSceneBase`）添加 CanvasGroup 透明度控制。
- **摄像机栈管理** — `SceneCameraManager` 在每次切换时重建 URP 摄像机栈，自动分配 Base/Overlay 角色和深度顺序。
- **返回导航** — `SceneManager` 维护切换历史栈，跳过 `CanBackTransition = false` 的条目，解析正确的返回目标。

### 屏幕栈 (`LighthouseExtends.ScreenStack`)

- **对话框 / 覆盖层管理** — 实现 `ScreenStackBase` 的屏幕被推入栈中。Open / Close / ClearAll 操作进入队列并按顺序执行。
- **System / Default 层** — 每个屏幕或覆盖层通过 `IScreenStackData.IsSystem` 放置在系统 Canvas 层或默认层。
- **输入阻断** — `ScreenStackBackgroundInputBlocker` 在最顶层屏幕后方放置全屏射线检测目标，防止点击穿透。
- **场景暂停 & 恢复** — `ScreenStackModuleSceneBase` 钩入场景切换事件，在前进离开时暂停栈，在返回进入时恢复，跨场景切换保留状态。
- **代理模式** — `ScreenStackModuleProxy`（场景作用域）桥接到 `ScreenStackModule`（模块作用域），使栈 API 可从任意 DI 作用域访问。
- **代码生成** — `ScreenStackDialogScriptGeneratorWindow` 和 `ScreenStackEntityFactoryGenerator` 从模板生成对话框脚本样板和 `ScreenStackEntityFactory` 分支表。

### 扩展模块 (`LighthouseExtends`)

| 模块 | 说明 |
|---|---|
| **Addressable** | `AssetManager` — 引用计数式 Addressables 封装，支持基于作用域的资源生命周期管理和并行加载。 |
| **Animation** | `LHTransitionAnimator` / `LHSceneTransitionAnimator` — 通过 `PlayableGraph` 播放 In/Out 动画片段，支持按方向设置开始延迟和互斥控制。 |
| **Language** | `LanguageService` — 响应式语言切换；在 `CurrentLanguage` 更新前并行调用已注册的处理器。 |
| **Font** | `FontService` — 订阅 `ILanguageService`，在每次语言变更时从 `LanguageFontSettings` 更新 `CurrentFont`（TMP_FontAsset）。 |
| **TextTable** | `TextTableService` — 通过 `ITextTableLoader` 加载各语言 TSV 文件，在运行时解析 `{param}` 占位符。附带可跨场景和 Prefab 编辑键值与翻译的编辑器窗口。 |
| **TextMeshPro** | `LHTextMeshPro` — 扩展 `TextMeshProUGUI`，通过 `TextTableService` 和 `FontService` 自动反映语言和字体变更。 |
| **InputLayer** | `InputLayerController` — 基于 `InputActionAsset` 的栈式输入分发；层可以消耗事件，或通过 `BlocksAllInput` 阻断所有下层输入。 |

---

## 架构

```
LighthouseArchitecture/
├── Lighthouse/                     # 核心框架
│   └── Scene/
│       ├── SceneBase/              # 场景用 MonoBehaviour 基类
│       ├── SceneCamera/            # URP 摄像机栈管理
│       ├── SceneTransitionPhase/   # 切换阶段定义
│       ├── SceneTransitionStep/    # 独立异步步骤
│       └── *.cs                    # SceneManager、上下文、数据模型
│
└── LighthouseExtends/              # 可选运行时 & 编辑器模块
    ├── Addressable/
    ├── Animation/
    ├── Font/
    ├── InputLayer/
    ├── Language/
    ├── ScreenStack/
    ├── TextMeshPro/
    └── TextTable/
```

**依赖方向**

```
游戏代码
    └── Lighthouse (ISceneManager, IScreenStackModule, ILanguageService, ...)
            └── LighthouseExtends (Animation, Font, TextTable, ScreenStack, ...)
```

---

## 实现示例

### 定义场景

```csharp
// 1. 定义切换数据类
public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;
}

// 2. 实现场景
public class HomeScene : MainSceneBase<HomeTransitionData>
{
    protected override UniTask OnEnter(HomeTransitionData data,
        ISceneTransitionContext context, CancellationToken ct)
    {
        // 在此初始化 UI
        return UniTask.CompletedTask;
    }
}

// 3. 切换
await sceneManager.TransitionScene(new HomeTransitionData());
```

### 打开对话框

```csharp
// 打开屏幕并传递数据
await screenStackModule.Open(new SampleDialogData());

// 关闭最顶层屏幕
await screenStackModule.Close();
```

### 切换语言

```csharp
// TextTableService 和 FontService 会自动更新。
// LHTextMeshPro 接收事件后，会自动切换为预先设定的适当字体和语言。
await languageService.SetLanguage("zh", cancellationToken);
```

### 本地化文本组件

在 Inspector 中为任意 `LHTextMeshPro` 组件指定 **Text Key**。  
运行时语言切换后，文本和字体会自动更新。

通过代码注册键：

```csharp
// 仅编辑器：生成 TSV 条目并重写为 new TextData("key")
var data = TextData.CreateTextData("Home", "HomeTitle", "欢迎");
lhText.SetTextData(data);
```

### 扩展功能

所有功能均通过 DI 管理，可在项目中自由选择所需功能，并对全部或部分进行扩展。

---

## 指南

| 语言 | 链接 |
|---|---|
| English | [Docs/en](Docs/en/readme.md) |
| 日本語 | [Docs/ja](Docs/ja/readme.md) |
| 한국어 | [Docs/ko](Docs/ko/readme.md) |
| 中文 | [Docs/zh](Docs/zh/readme.md) |

---

## 作者

**Lise** — [GitHub](https://github.com/lisearcheleeds) / [X](https://x.com/archeleeds)

---

## 许可证

本项目基于 [MIT 许可证](LICENSE) 开源。
