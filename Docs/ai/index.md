# Lighthouse — AI Reference Index

This file is intended to be loaded by an AI agent at the start of a session to understand the Lighthouse framework's capabilities, interfaces, and file locations.  
For full implementation details, read the source files referenced in each section directly.

---

## Framework Overview

Lighthouse is a Unity 6 (URP) application framework built on VContainer (DI) and UniTask (async).

| Feature | Package |
|---|---|
| Scene transition & stack management | `com.lisearcheleeds.lighthouse` |
| Dialog / overlay management | `com.lisearcheleeds.lighthouse-extends.screenstack` |
| Multi-language switching | `com.lisearcheleeds.lighthouse-extends.language` |
| Font switching per language | `com.lisearcheleeds.lighthouse-extends.font` |
| Localized text via TSV | `com.lisearcheleeds.lighthouse-extends.texttable` |
| TextMeshPro auto-update | `com.lisearcheleeds.lighthouse-extends.textmeshpro` |
| Input layer stack | `com.lisearcheleeds.lighthouse-extends.inputlayer` |
| UI components | `com.lisearcheleeds.lighthouse-extends.uicomponent` |
| Transition animations | `com.lisearcheleeds.lighthouse-extends.animation` |

---

## Repository File Structure

```
Client/Assets/
├── Lighthouse/
│   ├── Runtime/Scripts/Scene/          # Core scene management
│   │   ├── SceneBase/                  # Scene base classes
│   │   ├── SceneCamera/                # URP camera stack management
│   │   ├── SceneTransitionPhase/       # Transition phase definitions
│   │   └── SceneTransitionStep/        # Transition step definitions
│   └── Editor/Scripts/
│       ├── Menu/LighthouseEditor.cs    # Menu items & settings creation
│       └── ScriptGenerator/            # Code generation tools
│
└── LighthouseExtends/
    ├── Animation/Runtime/Scripts/
    ├── Font/Runtime/Scripts/
    ├── InputLayer/Runtime/Scripts/
    ├── Language/Runtime/Scripts/
    ├── ScreenStack/Runtime/Scripts/
    ├── TextMeshPro/Runtime/Scripts/
    ├── TextTable/Runtime/Scripts/
    └── UIComponent/Runtime/Scripts/
```

---

## Core Interfaces

### ISceneManager
**File:** `Lighthouse/Runtime/Scripts/Scene/ISceneManager.cs`  
**Namespace:** `Lighthouse.Scene`  
**DI registration:** `builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
bool IsTransition { get; }
UniTask TransitionScene(TransitionDataBase nextTransitionData,
    TransitionType transitionType = TransitionType.Auto,
    MainSceneId backMainSceneId = null);
UniTask BackScene(TransitionType transitionType = TransitionType.Auto);
UniTask PreReboot();
```

### IMainSceneManager
**File:** `Lighthouse/Runtime/Scripts/Scene/IMainSceneManager.cs`  
**DI registration:** `builder.Register<MainSceneManager>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
void SetEnqueueParentLifetimeScope(Func<IDisposable> factory);
```

### IModuleSceneManager
**File:** `Lighthouse/Runtime/Scripts/Scene/IModuleSceneManager.cs`  
**DI registration:** `builder.Register<ModuleSceneManager>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
void SetEnqueueParentLifetimeScope(Func<IDisposable> factory);
```

### ISceneGroupProvider
**File:** `Lighthouse/Runtime/Scripts/Scene/ISceneGroupProvider.cs`  
**DI registration:** `builder.Register<SceneGroupProvider>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
SceneGroup GetSceneGroup(MainSceneId mainSceneId);
```

### IScreenStackModule
**File:** `LighthouseExtends/ScreenStack/Runtime/Scripts/IScreenStackModule.cs`  
**Namespace:** `LighthouseExtends.ScreenStack`  
**DI registration:** `builder.Register<ScreenStackModuleProxy>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
UniTask Enqueue(IScreenStackData screenStackData);
UniTask Open();
UniTask Open(IScreenStackData screenStackData);
UniTask Close();
UniTask Close(IScreenStackData screenStackData);
UniTask ClearAll();
UniTask ClearCurrentAll();
```

### ILanguageService
**File:** `LighthouseExtends/Language/Runtime/Scripts/ILanguageService.cs`  
**Namespace:** `LighthouseExtends.Language`  
**DI registration:** `builder.Register<LanguageService>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
ReadOnlyReactiveProperty<string> CurrentLanguage { get; }
void RegisterChangeHandler(Func<string, CancellationToken, UniTask> handler);
UniTask SetLanguage(string languageCode, CancellationToken cancellationToken);
```

### IFontService
**File:** `LighthouseExtends/Font/Runtime/Scripts/IFontService.cs`  
**Namespace:** `LighthouseExtends.Font`  
**DI registration:** `builder.Register<FontService>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
ReadOnlyReactiveProperty<TMP_FontAsset> CurrentFont { get; }
TMP_FontAsset GetFont(string languageCode);
```

### ITextTableService
**File:** `LighthouseExtends/TextTable/Runtime/Scripts/ITextTableService.cs`  
**Namespace:** `LighthouseExtends.TextTable`  
**DI registration:** `builder.Register<TextTableService>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
ReadOnlyReactiveProperty<string> CurrentLanguage { get; }
string GetText(ITextData textData);
```

### IInputLayerController
**File:** `LighthouseExtends/InputLayer/Runtime/Scripts/IInputLayerController.cs`  
**Namespace:** `LighthouseExtends.InputLayer`  
**DI registration:** `builder.Register<InputLayerController>(Lifetime.Singleton).AsImplementedInterfaces()`

```csharp
void PushLayer(IInputLayer layer, InputActionMap actionMap);
void PopLayer();
void PopLayer(IInputLayer target);
```

---

## Scene Base Classes

### MainSceneBase\<TTransitionData\>
**File:** `Lighthouse/Runtime/Scripts/Scene/SceneBase/MainSceneBase.cs`  
**Namespace:** `Lighthouse.Scene.SceneBase`

| Override point | Description |
|---|---|
| `MainSceneId MainSceneId` | Returns this scene's ID (required) |
| `SaveSceneState(CancellationToken)` | Called before leaving this scene. Persist state or call server here. Throw `LHSceneInterceptException` to redirect to a different scene. |
| `OnEnter(TTransitionData, ISceneTransitionContext, CancellationToken)` | Called when the scene is entered |
| `OnLeave(ISceneTransitionContext, CancellationToken)` | Called when the scene is left |
| `OnSceneTransitionFinished(SceneTransitionDiff)` | Called after the full transition completes |

### CanvasMainSceneBase\<TTransitionData\>
**File:** `Lighthouse/Runtime/Scripts/Scene/SceneBase/CanvasMainSceneBase.cs`  
Extends `MainSceneBase<TTransitionData>`. Use this for scenes with a Canvas.

| Additional override point | Description |
|---|---|
| `InAnimation(ISceneTransitionContext)` | Enter animation |
| `OutAnimation(ISceneTransitionContext)` | Exit animation |
| `ResetInAnimation(ISceneTransitionContext)` | Reset enter animation to initial state |

### ModuleSceneBase
**File:** `Lighthouse/Runtime/Scripts/Scene/SceneBase/ModuleSceneBase.cs`

| Override point | Description |
|---|---|
| `ModuleSceneId ModuleSceneId` | Returns this module's ID (required) |

### CanvasModuleSceneBase
**File:** `Lighthouse/Runtime/Scripts/Scene/SceneBase/CanvasModuleSceneBase.cs`  
Extends `ModuleSceneBase`. Use this for module scenes with a Canvas.

### ScreenStackBase
**File:** `LighthouseExtends/ScreenStack/Runtime/Scripts/ScreenStackBase.cs`  
Base class for dialogs and overlays.

---

## TransitionDataBase

**File:** `Lighthouse/Runtime/Scripts/Scene/TransitionDataBase.cs`

```csharp
abstract MainSceneId MainSceneId { get; }           // Target scene ID (required)
bool CanTransition { get; protected set; }           // default: true
bool CanBackTransition { get; protected set; }       // default: true
virtual UniTask LoadSceneState(TransitionDirectionType, CancellationToken)
```

`LoadSceneState` is executed in `LoadNextSceneStatePhase` (after `SaveCurrentSceneStatePhase` completes). To redirect the transition to a different scene from inside `LoadSceneState`, throw `LHSceneInterceptException`.

`TransitionData` is always defined as a nested class inside the scene class:

```csharp
public class HomeScene : CanvasMainSceneBase<HomeScene.HomeTransitionData>
{
    public class HomeTransitionData : TransitionDataBase
    {
        public override MainSceneId MainSceneId => YourProductMainSceneId.Home;
    }
}
```

### TransitionType
**File:** `Lighthouse/Runtime/Scripts/Scene/TransitionType.cs`

| Value | Description |
|---|---|
| `Auto` | Automatically selected based on SceneGroup |
| `Exclusive` | Out animation → In animation swap |
| `Cross` | Crossfade |

---

## LHLogger

**File:** `Lighthouse/Runtime/Scripts/Logger/LHLogger.cs`  
**Namespace:** `Lighthouse`

Static logger that routes all internal Lighthouse logs through a replaceable `ILogger`. Products can suppress or redirect logs without modifying the framework.

```csharp
ILogger Logger { get; }                        // Current logger (default: Debug.unityLogger)
static event Action<LogType, string> OnLog;    // Fires on every log call; use for in-game consoles etc.

static void SetLogger(ILogger logger);         // Replace the logger (null resets to Debug.unityLogger)
static void Log(object message);
static void LogWarning(object message);
static void LogError(object message);
```

All Lighthouse internal `Debug.Log/LogWarning/LogError` calls go through `LHLogger`. The Unity Console tag is `"Lighthouse"`.

## LHSceneInterceptException

**File:** `Lighthouse/Runtime/Scripts/Scene/LHSceneInterceptException.cs`  
**Namespace:** `Lighthouse.Scene`

Throw inside `TransitionDataBase.LoadSceneState` to redirect the transition to a different scene. Only valid in `LoadNextSceneStatePhase` (`CanTransitionIntercept = true`). Throwing in any other phase is a developer error and is reported as `InvalidOperationException`.

```csharp
public LHSceneInterceptException(TransitionDataBase redirectTo)
TransitionDataBase RedirectTo { get; }
```

**Usage example:**
```csharp
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    var isExpired = await CheckEventExpired(ct);
    if (isExpired)
    {
        throw new LHSceneInterceptException(new HomeTransitionData());
    }
}
```

---

## Code Generation Tools (Editor)

### SceneIdGenerator
**File:** `Lighthouse/Editor/Scripts/ScriptGenerator/SceneIdGenerator.cs`  
Runs automatically on `EditorBuildSettings.sceneListChanged`. Can also be triggered manually.

- **Menu:** `Lighthouse > Generate > Auto > Generate "MainSceneId" manually`
- **Menu:** `Lighthouse > Generate > Auto > Generate "ModuleSceneId" manually`
- **How it works:** Scans Build Settings scene paths for entries containing `MainScene` or `ModuleScene` and generates the corresponding SceneId class.

### SceneScriptGenerator
**File:** `Lighthouse/Editor/Scripts/ScriptGenerator/SceneScriptGenerator.cs`  
**Menu:** `Lighthouse > Generate > Open Scene scripts generator`

> **For AI agents generating scene scripts:**  
> You do not need to invoke `SceneScriptGenerator` directly.  
> Instead, read the template `.txt` files listed below, apply the placeholder substitutions from the table below, and write the output files directly to disk. Unity will detect the file changes and reimport automatically.  
> If the project defines custom templates, use the directory referenced by `GenerateSettings.SceneScriptTemplates` instead of the defaults.
>
> **Default template directories:**
> - `Lighthouse/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultMainSceneTemplate/` — for MainScene (Scene.txt / LifetimeScope.txt / Presenter.txt / IPresenter.txt / View.txt / IView.txt)
> - `Lighthouse/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultModuleSceneTemplate/` — for ModuleScene (same files)
>
> **Template file format:** Line 1 is the output filename pattern (`#OUTPUT {{SCENE_FILE_NAME}}.cs`). Line 2 onward is the file content.  
> **Output path:** `GenerateSettings.SceneScriptOutputDirectory/{MainScene|ModuleScene}/{SceneName}/`

**Template placeholders:**

| Placeholder | Resolved value |
|---|---|
| `{{NAMESPACE}}` | `{ProductNameSpace}.View.Scene.{SceneType}.{SceneName}` |
| `{{SCENE_NAME}}` | Scene name as given (e.g. `Home`) |
| `{{SCENE_NAME_CAMEL}}` | camelCase scene name (e.g. `home`) |
| `{{SCENE_FILE_NAME}}` | `HomeScene` for MainScene, `HomeModuleScene` for ModuleScene |
| `{{SCENE_FILE_NAME_CAMEL}}` | `homeScene` / `homeModuleScene` |
| `{{BASE_CLASS}}` | Base class name; if generic: `CanvasMainSceneBase<HomeScene.HomeTransitionData>` |
| `{{BASE_CLASS_NAMESPACE}}` | Namespace of the base class |
| `{{SCENE_ID_TYPE}}` | `MainSceneId` or `ModuleSceneId` |
| `{{SCENE_ID_CLASS}}` | Generated SceneId class name (e.g. `SampleProductMainSceneId`) |
| `{{GENERATED_SCENE_ID_NAMESPACE}}` | Namespace of the generated SceneId class |

### GenerateSettings
**File:** `Lighthouse/Editor/Scripts/ScriptableObject/GenerateSettings.cs`  
**Creation menu:** `Lighthouse > Settings > GenerateSettings`  
**Asset path:** `Assets/Settings/Lighthouse/GenerateSettings.asset`

| Property | Description |
|---|---|
| `ProductNameSpace` | Root namespace for all generated classes |
| `GeneratedFileDirectory` | SceneId output folder (returns path relative to `Assets/`) |
| `SceneScriptOutputDirectory` | Scene script output folder (returns full path including `Assets/`) |
| `MainSceneIdPrefix` / `ModuleSceneIdPrefix` | Prefix for the generated SceneId class name |
| `GeneratedMainSceneIdClassName` | `{prefix}MainSceneId` |
| `GeneratedModuleSceneIdClassName` | `{prefix}ModuleSceneId` |
| `GeneratedSceneIdNamespace` | `{ProductNameSpace}.LighthouseGenerated` |
| `SceneScriptTemplates` | `SceneScriptTemplate[]` — list of available templates |

---

## Editor Settings Assets

| Asset | Location | How to create |
|---|---|---|
| `GenerateSettings.asset` | `Assets/Settings/Lighthouse/` | `Lighthouse > Settings > GenerateSettings` |
| `SceneEditSettings.asset` | `Assets/Settings/Lighthouse/` | `Lighthouse > Settings > SceneEditSettings` or auto-created on editor startup |
| `ScreenStackGenerateSettings.asset` | `Assets/Settings/Lighthouse/` | `Lighthouse > Settings > ScreenStackGenerateSettings` |
| `TextTableEditorSettings.asset` | `Assets/Settings/Lighthouse/` | Auto-created when the TextTable editor window is opened |

---

## DI Registration Pattern (ProductLifetimeScope)

Standard registration set for a project using Lighthouse:

```csharp
// Lighthouse core (required)
builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<SceneTransitionController>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<DefaultSceneTransitionContextFactory>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<SceneGroupProvider>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<MainSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<ModuleSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<SceneCameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<DefaultSceneTransitionSequenceProvider>(Lifetime.Singleton).AsImplementedInterfaces();

// LighthouseExtends (include only what the project uses)
builder.Register<LanguageService>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<FontService>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<TextTableService>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<ScreenStackModuleProxy>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<InputLayerController>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<ExclusiveInputService>(Lifetime.Singleton).AsImplementedInterfaces();
```

---

## Naming Conventions

| Target | Convention | Example |
|---|---|---|
| MainScene Unity scene file path | Must contain `MainScene` in the path | `Scene/MainScene/Home/Home.unity` |
| ModuleScene Unity scene file path | Must contain `ModuleScene` in the path | `Scene/ModuleScene/Background/Background.unity` |
| MainScene script name | `{SceneName}Scene` | `HomeScene.cs` |
| ModuleScene script name | `{SceneName}ModuleScene` | `BackgroundModuleScene.cs` |
| LifetimeScope script name | `{SceneName}LifetimeScope` | `HomeLifetimeScope.cs` |
| TransitionData class name | `{SceneName}TransitionData` (nested inside the scene class) | `HomeScene.HomeTransitionData` |
| Generated SceneId class name | `{Prefix}MainSceneId` / `{Prefix}ModuleSceneId` | `SampleProductMainSceneId` |
| Generated SceneId namespace | `{ProductNameSpace}.LighthouseGenerated` | `SampleProduct.LighthouseGenerated` |

---

## Related Documentation

- Setup guide (Japanese): [`Docs/ja/setup.md`](../ja/setup.md)
- Feature guide (Japanese): [`Docs/ja/index.md`](../ja/index.md)
