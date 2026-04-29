**[日本語](README.ja.md) | [한국어](README.ko.md) | [中文](README.zh.md)**

# Lighthouse

A Unity application framework providing a structured scene management system, dialog stack, and a set of extensible runtime modules — built on [VContainer](https://github.com/hadashiA/VContainer) and [UniTask](https://github.com/Cysharp/UniTask).

The current version is **1.0.0**.

<img width="1677" height="938" alt="lighthouse" src="https://github.com/user-attachments/assets/f0e9c5de-f858-4e0d-be63-7e57a4d6558c" />

---

## Requirements

| Dependency | Version |
|---|---|
| Unity | 6000.0 or later (URP) |
| VContainer | >= 1.17.0 |
| UniTask | >= 2.5.10 |
| R3 | >= 1.3.0 |
| TextMeshPro | 3.x |
| Input System | >= 1.17.0 |

---

## Repository

| Repository | Description |
|---|---|
| [Lighthouse](https://github.com/lisearcheleeds/Lighthouse) | Unity Architecture Framework |
| [LighthouseSample](https://github.com/lisearcheleeds/LighthouseSample) | Sample project demonstrating framework usage |
| [LighthouseQuickPackage](https://github.com/lisearcheleeds/LighthouseQuickPackage) | A pre-configured, empty project |

---

## WebGL Demonstration
https://lisearcheleeds.github.io/LighthouseSample/

---

## Installation

### Option 1: Unity Package Manager — Git URL

Open `Packages/manifest.json` and add the packages you need:

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

> **Note:** Dependencies (UniTask, VContainer, R3, etc.) are not resolved automatically with Git URL. Add them to your project manually or via [OpenUPM](https://openupm.com).

### Option 2: OpenUPM _(coming soon)_

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

### Option 3: UnityPackage

Download the latest `.unitypackage` from [Releases](https://github.com/lisearcheleeds/Lighthouse/releases) and import it via `Assets > Import Package > Custom Package`.

---

## Features

### Scene System (`Lighthouse`)

- **Scene stack management** — Scenes are stacked with each transition, providing the ability to navigate back to the previous screen.
- **Main + Module scene model** — A game screen is expressed by combining one or more main scenes with multiple module scenes. This allows you to swap only the central content while keeping shared UI such as headers and backgrounds in place.
- **Scene caching** — Scenes are loaded and unloaded in units called `SceneGroup`. When transitioning to a scene, other main and module scenes belonging to the same `SceneGroup` are loaded and cached simultaneously. Since `SceneGroup` is defined on the product side, you can balance user experience and performance by configuring a single SceneGroup.
- **Phase / Step pipeline** — Scene transitions are composed of ordered phases (e.g. `OutAnimationPhase`, `LoadSceneGroupPhase`, `EnterScenePhase`), each containing parallel steps. The sequence can be replaced by defining `ISceneTransitionSequenceProvider` on the product side.
- **Transition types** — `Exclusive` (swap with out/in animation) and `Cross` (crossfade) are built in. `Auto` selects the appropriate type based on the scene group.
- **Scene base classes** — `MainSceneBase<TTransitionData>` and `ModuleSceneBase` handle activation, animation hooks, and canvas injection. Canvas variants (`CanvasMainSceneBase`, `CanvasModuleSceneBase`) add CanvasGroup alpha control.
- **Camera stack management** — `SceneCameraManager` rebuilds the URP camera stack on each transition, assigning Base/Overlay roles and depth order automatically.
- **Back navigation** — `SceneManager` maintains a transition history stack and resolves the correct Back target, skipping entries with `CanBackTransition = false`.

### Screen Stack (`LighthouseExtends.ScreenStack`)

- **Dialog / overlay management** — Screens implementing `ScreenStackBase` are pushed onto a stack. Open/Close/ClearAll operations are queued and executed in order.
- **System / Default layers** — Each screen or overlay is placed on either the system canvas layer or the default layer, controlled via `IScreenStackData.IsSystem`.
- **Input blocking** — `ScreenStackBackgroundInputBlocker` places a full-screen raycast target behind the top screen to prevent touch-through.
- **Scene suspension & resume** — `ScreenStackModuleSceneBase` hooks into scene transition events to suspend the stack on Forward leave and resume it on Back enter, preserving state across scene transitions.
- **Proxy pattern** — `ScreenStackModuleProxy` (scene scope) bridges to `ScreenStackModule` (module scope), so the stack API remains accessible from any DI scope.
- **Code generation** — `ScreenStackDialogScriptGeneratorWindow` and `ScreenStackEntityFactoryGenerator` produce boilerplate dialog scripts and the `ScreenStackEntityFactory` switch table from templates.

### Extensions (`LighthouseExtends`)

| Module | Description |
|---|---|
| **Addressable** | `AssetManager` — Ref-counted Addressables wrapper with scoped asset lifetime management and parallel loading support. |
| **Animation** | `LHTransitionAnimator` / `LHSceneTransitionAnimator` — In/Out clip playback via `PlayableGraph`, with per-direction start delay and mutual exclusion. |
| **Language** | `LanguageService` — Reactive language switching; registered handlers are called in parallel before `CurrentLanguage` updates. |
| **Font** | `FontService` — Subscribes to `ILanguageService` and updates `CurrentFont` (TMP_FontAsset) from `LanguageFontSettings` on each language change. |
| **TextTable** | `TextTableService` — Loads per-language TSV files via `ITextTableLoader` and resolves `{param}` placeholders at runtime. Comes with a full editor window for editing keys and translations across Scenes and Prefabs. |
| **TextMeshPro** | `LHTextMeshPro` — Extends `TextMeshProUGUI` to automatically reflect language and font changes via `TextTableService` and `FontService`. |
| **InputLayer** | `InputLayerController` — Stack-based input dispatch over `InputActionAsset`; a layer can consume an event or block all lower layers via `BlocksAllInput`. |

---

## Architecture

```
LighthouseArchitecture/
├── Lighthouse/                     # Core framework
│   └── Scene/
│       ├── SceneBase/              # MonoBehaviour base classes for scenes
│       ├── SceneCamera/            # URP camera stack management
│       ├── SceneTransitionPhase/   # Transition phase definitions
│       ├── SceneTransitionStep/    # Individual async steps
│       └── *.cs                    # SceneManager, context, data models
│
└── LighthouseExtends/              # Optional runtime & editor modules
    ├── Addressable/
    ├── Animation/
    ├── Font/
    ├── InputLayer/
    ├── Language/
    ├── ScreenStack/
    ├── TextMeshPro/
    └── TextTable/
```

**Dependency direction**

```
Game Code
    └── Lighthouse (ISceneManager, IScreenStackModule, ILanguageService, ...)
            └── LighthouseExtends (Animation, Font, TextTable, ScreenStack, ...)
```

---

## Implementation Highlights

### Defining a Scene

```csharp
// 1. Define a transition data class
public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;
}

// 2. Implement a scene
public class HomeScene : MainSceneBase<HomeTransitionData>
{
    protected override UniTask OnEnter(HomeTransitionData data,
        ISceneTransitionContext context, CancellationToken ct)
    {
        // initialize UI here
        return UniTask.CompletedTask;
    }
}

// 3. Transition
await sceneManager.TransitionScene(new HomeTransitionData());
```

### Opening a Dialog

```csharp
// Open a screen and pass data
await screenStackModule.Open(new SampleDialogData());

// Close the top screen
await screenStackModule.Close();
```

### Switching Language

```csharp
// TextTableService and FontService update automatically.
// LHTextMeshPro receives the event and switches to the appropriate font and language automatically.
await languageService.SetLanguage("ja", cancellationToken);
```

### Localizing a Text Component

Assign a **Text Key** to any `LHTextMeshPro` component in the Inspector.  
The text and font update automatically when the language changes at runtime.

To register a new key from code:

```csharp
// Editor-only: generates a TSV entry and rewrites to new TextData("key")
var data = TextData.CreateTextData("Home", "HomeTitle", "Welcome");
lhText.SetTextData(data);
```

### Extending Functionality

All of these are managed via DI, allowing you to choose which features to include and extend any or all of them in your project.

---

## Guide

| Language | Link |
|---|---|
| English | [Docs/en](Docs/en/readme.md) |
| 日本語 | [Docs/ja](Docs/ja/readme.md) |
| 한국어 | [Docs/ko](Docs/ko/readme.md) |
| 中文 | [Docs/zh](Docs/zh/readme.md) |

---

## Author

**Lise** — [GitHub](https://github.com/lisearcheleeds) / [X](https://x.com/archeleeds)

---

## License

This project is licensed under the [MIT License](LICENSE).
