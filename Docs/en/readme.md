# Lighthouse — Feature Reference

## Table of Contents

- [Scene System](#scene-system)
  - [SceneGroup](#scenegroup)
  - [TransitionDataBase](#transitiondatabase)
  - [Scene Base Classes](#scene-base-classes)
  - [SceneBase Lifecycle](#scenebase-lifecycle)
  - [Transition Types](#transition-types)
  - [Transition Phase Pipeline](#transition-phase-pipeline)
  - [Scene Interception](#scene-interception)
  - [Back Navigation](#back-navigation)
  - [SceneCameraManager](#scenecameramanager)
- [ScreenStack](#screenstack)
  - [IScreenStackModule API](#iscreenstackmodule-api)
  - [IScreenStackData](#iscreenstackdata)
  - [ScreenStackBase Lifecycle](#screenstackbase-lifecycle)
  - [Suspend / Resume](#suspend--resume)
  - [ScreenStackModuleProxy](#screenstackmoduleproxy)
  - [Code Generation](#code-generation)
- [Addressable](#addressable)
- [Animation](#animation)
- [Language](#language)
- [Font](#font)
- [TextTable](#texttable)
- [TextMeshPro](#textmeshpro)
- [InputLayer](#inputlayer)
- [UIComponent](#uicomponent)

---

## Scene System

**Package:** `com.lisearcheleeds.lighthouse`  
**Additional Dependencies:** VContainer · UniTask

The scene system is the core of the framework. It manages scenes in a stack and executes transitions through an asynchronous phase pipeline.

### SceneGroup

A `SceneGroup` represents a collection of scenes that are loaded and unloaded together. It consists of one or more **main scenes** paired with their corresponding **module scenes**.

```csharp
// Example: defining a SceneGroup (implement ISceneGroupProvider and register with DI)
new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
{
    { SceneIds.Home,     new[] { SceneIds.Header, SceneIds.Footer } },
    { SceneIds.Setting,  new[] { SceneIds.Header, SceneIds.Footer } },
    { SceneIds.Detail,   new[] { SceneIds.Header } },
});
```

Scenes belonging to the same `SceneGroup` are cached. Scene loading and unloading only occurs when crossing group boundaries, so the granularity of groups lets you balance performance and memory usage.

---

### TransitionDataBase

The base class for data classes that specify the transition destination. Create a derived class for each transition.

```csharp
public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;

    // Set CanTransition = false to skip the transition entirely
    // Set CanBackTransition = false to exclude from the back navigation stack

    // Override if async processing is needed before the transition
    // Throw LHSceneInterceptException to redirect to a different scene
    public override async UniTask LoadSceneState(
        TransitionDirectionType direction, CancellationToken ct)
    {
        // Example: check authentication and redirect to login
        if (!await authService.IsLoggedIn(ct))
            throw new LHSceneInterceptException(new LoginTransitionData());
    }
}
```

| Property / Method | Description |
|---|---|
| `MainSceneId` | The ID of the destination main scene |
| `CanTransition` | Set to `false` to skip transitions to this data |
| `CanBackTransition` | Set to `false` to exclude from BackScene candidates |
| `LoadSceneState(direction, ct)` | Async processing just before the transition. Also used for interception |

---

### Scene Base Classes

#### MainSceneBase\<TTransitionData\>

The base MonoBehaviour class for main scenes.

```csharp
public class HomeScene : MainSceneBase<HomeTransitionData>
{
    protected override async UniTask OnEnter(
        HomeTransitionData data, ISceneTransitionContext context, CancellationToken ct)
    {
        // Initialization using transition data
    }

    // Override to save state before leaving
    // Throw LHSceneInterceptException here to redirect to another scene
    public override UniTask SaveSceneState(CancellationToken ct) => UniTask.CompletedTask;
}
```

#### CanvasMainSceneBase\<TTransitionData\>

Base class for main scenes with a UI Canvas. Automatically controls `CanvasGroup` alpha (0 on load, 1 on enter, 0 on leave). Requires `CanvasGroup` and `SceneCanvasInitializer` components.

#### ModuleSceneBase

Base class for module scenes (headers, footers, etc.) shared across multiple main scenes. Automatically toggles the GameObject's active state based on `SceneTransitionDiff`.

#### CanvasModuleSceneBase

Module scene base class with `CanvasGroup` alpha control.

---

### SceneBase Lifecycle

Lifecycle hooks available in all scene base classes.

| Method | Timing | Notes |
|---|---|---|
| `OnSetup()` | First entry only | One-time initialization, independent of DI |
| `OnLoad()` | When the scene group is loaded | Calls `gameObject.SetActive(false)` by default |
| `OnEnter(context, ct)` | On every entry | Called every time. Receive TransitionData here |
| `InAnimation(context)` | When playing the in-animation | `OnBeginInAnimation` / `OnCompleteInAnimation` also available |
| `OutAnimation(context)` | When playing the out-animation | `OnBeginOutAnimation` / `OnCompleteOutAnimation` also available |
| `OnLeave(context, ct)` | On leave | |
| `OnUnload()` | When the scene group is unloaded | |
| `OnSceneTransitionFinished(diff)` | After the transition fully completes | |
| `ResetInAnimation(context)` | Reset to the initial state of InAnimation | Used during Cross transitions |

From `context` (`ISceneTransitionContext`) you can access `TransitionData`, `SceneTransitionDiff`, `TransitionDirectionType`, `TransitionType`, and more.

---

### Transition Types

Specified via the `transitionType` argument of `ISceneManager.TransitionScene`.

| Value | Behavior |
|---|---|
| `Auto` (default) | Automatically selects `Cross` within the same SceneGroup, or `Exclusive` when crossing groups |
| `Exclusive` | Sequentially: OutAnimation → old scene leave → load/unload → new scene enter → InAnimation |
| `Cross` | Swaps old and new scenes in parallel. Use when you want a crossfade animation |

---

### Transition Phase Pipeline

Scene transitions have a two-layer structure of **phases** and **steps**.

**Exclusive Sequence (default)**

```
SaveCurrentSceneStatePhase  → calls SaveSceneState on the current scene
LoadNextSceneStatePhase     → calls LoadSceneState on the next scene (interceptable)
OutAnimationPhase           → runs OutAnimation on the current scene in parallel
LeaveScenePhase             → calls OnLeave on the current scene
LoadSceneGroupPhase         → additively loads the next scene group
UnloadSceneGroupPhase       → unloads unnecessary scene groups
EnterScenePhase             → calls OnEnter on the next scene
InAnimationPhase            → runs InAnimation on the next scene in parallel
FinishTransitionPhase       → notifies OnSceneTransitionFinished
CleanupPhase                → rebuilds the camera stack
```

**Cross Sequence (default)**

```
SaveCurrentSceneStatePhase
LoadNextSceneStatePhase
LoadSceneGroupPhase
EnterScenePhase
CrossAnimationPhase   → runs old scene Out and new scene In in parallel
LeaveScenePhase
UnloadSceneGroupPhase
FinishTransitionPhase
```

You can freely customize the order and content of phases by implementing `ISceneTransitionSequenceProvider` and registering it with DI.

---

### Scene Interception

Use `LHSceneInterceptException` to redirect to a different scene during a transition.

```csharp
// Throw inside TransitionDataBase.LoadSceneState
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    if (!isLoggedIn)
        throw new LHSceneInterceptException(new LoginTransitionData());
}
```

Interception is only valid during `LoadNextSceneStatePhase`. Throwing in any other phase results in an `InvalidOperationException`.

---

### Back Navigation

Calling `ISceneManager.BackScene()` walks back through the transition history stack to resolve the return destination.

- Entries with `CanTransition = false` or `CanBackTransition = false` are skipped
- If no valid return destination exists in the stack, the operation is ignored
- An invalid state where no valid destination can be found throws `InvalidOperationException`

```csharp
// Go back to the previous scene
await sceneManager.BackScene();

// Go back with a specific transition type
await sceneManager.BackScene(TransitionType.Cross);
```

You can also pass a `backMainSceneId` argument to `TransitionScene` to trim the stack down to a specified scene after transitioning (useful for shortcut navigation to a specific scene).

---

### SceneCameraManager

Automatically rebuilds the URP camera stack on every transition.

- Cameras collected from each scene's `GetSceneCameraList()` are sorted by `SceneCameraType` and `CameraDefaultDepth`
- The first camera is set as **Base**, the rest as **Overlay**
- The UI camera (used by Canvas) is always added as the last Overlay

To control camera order, implement `ISceneCamera` and return the appropriate `SceneCameraType` and `CameraDefaultDepth`.

---

## ScreenStack

**Package:** `com.lisearcheleeds.lighthouse-extends.screenstack`  
**Dependencies:** `com.lisearcheleeds.lighthouse` (core)

A module for managing dialogs and overlays in a stack. Open / Close operations are queued and executed sequentially after the previous operation completes.

### IScreenStackModule API

```csharp
// Add data to the queue only (does not open immediately)
await screenStackModule.Enqueue(new SampleDialogData());

// Open the front of the queue (requires a preceding Enqueue)
await screenStackModule.Open();

// Enqueue and Open in one call (typically use this)
await screenStackModule.Open(new SampleDialogData());

// Close the frontmost screen
await screenStackModule.Close();

// Close the screen corresponding to specific data
await screenStackModule.Close(myDialogData);

// Close all screens in the current scene's stack
await screenStackModule.ClearCurrentAll();

// Clear the entire stack across all scenes
await screenStackModule.ClearAll();
```

---

### IScreenStackData

The interface implemented by each screen's data class.

| Property | Type | Description |
|---|---|---|
| `IsSystem` | `bool` | If `true`, placed on the system Canvas layer (renders above normal UI) |
| `IsOverlayOpen` | `bool` | If `true`, opens without playing the OutAnimation on the previous screen (overlay display) |

```csharp
public class SampleDialogData : IScreenStackData
{
    public bool IsSystem => false;
    public bool IsOverlayOpen => false;

    public string Message { get; }
    public SampleDialogData(string message) => Message = message;
}
```

---

### ScreenStackBase Lifecycle

The base MonoBehaviour class attached to each dialog Prefab.

| Method | Timing |
|---|---|
| `OnInitialize()` | Immediately after Prefab instantiation, before display |
| `OnEnter(isResume)` | When this screen becomes the top of the stack. `isResume=true` when returning after another screen closes |
| `PlayInAnimation()` | In-animation. Initialize with `ResetInAnimation()` |
| `OnLeave()` | Just before being removed from the stack |
| `PlayOutAnimation()` | Out-animation. Initialize with `ResetOutAnimation()` |
| `Dispose()` | Destroy the GameObject after closing |

---

### Suspend / Resume

Scenes inheriting from `ScreenStackModuleSceneBase` preserve the stack state across scene transitions.

- **Forward transition**: `Suspend` (saves) the current stack before leaving
- **Back transition**: `Resume` (restores) the saved stack on entry

This enables a workflow where a dialog remains open when navigating to another scene, and is restored upon returning.

---

### ScreenStackModuleProxy

A proxy class for accessing the stack API across DI scopes.

- `ScreenStackModuleProxy` (scene scope) holds a reference to `ScreenStackModule` (module scope)
- The caller injected with `IScreenStackModule` from the scene scope can use the API without worrying about scope differences

---

### Code Generation

Boilerplate code for dialogs can be automatically generated from the editor menu.

- **ScreenStackDialogScriptGeneratorWindow**: Generates View / Data classes for dialogs from templates
- **ScreenStackEntityFactoryGenerator**: Automatically generates switch branches for `IScreenStackEntityFactory`

---

## Addressable

**Package:** `com.lisearcheleeds.lighthouse-extends.addressable`  
**Dependencies:** UniTask · Unity Addressables

A ref-counted Addressables wrapper with scoped asset lifetime management and parallel loading support. Assets are tracked per address; multiple scopes sharing the same address reuse the underlying `AsyncOperationHandle`, and `Addressables.Release` is called only when the last handle is disposed.

### IAssetManager

Register `AssetManager` with DI and inject `IAssetManager`. Call `CreateScope()` to obtain an `IAssetScope` for loading assets.

```csharp
// VContainer example
builder.Register<AssetManager>(Lifetime.Singleton).As<IAssetManager>();
```

### IAssetScope

Create one scope per scene (or per logical loading context). Disposing the scope releases all handles it holds at once.

```csharp
IAssetScope scope = assetManager.CreateScope();

// Load a single asset by address
IAssetHandle<Sprite> handle = await scope.LoadAsync<Sprite>("ui/icon_home", ct);
icon.sprite = handle.Asset;

// Load multiple assets by address list (sequential)
IReadOnlyList<IAssetHandle<Sprite>> handles =
    await scope.LoadAsync<Sprite>(new[] { "ui/icon_a", "ui/icon_b" }, ct);

// Load all assets matching a label
IReadOnlyList<AudioClip> clips = await scope.LoadByLabelAsync<AudioClip>("bgm", ct);

// Release all handles acquired by this scope
scope.Dispose();
```

| Method | Description |
|---|---|
| `LoadAsync<T>(string address, ct)` | Loads a single asset by address. Returns `IAssetHandle<T>`. |
| `LoadAsync<T>(IReadOnlyList<string> addresses, ct)` | Loads multiple assets sequentially. Returns `IReadOnlyList<IAssetHandle<T>>`. |
| `LoadByLabelAsync<T>(string label, ct)` | Loads all assets matching a label. Returns `IReadOnlyList<T>`. |
| `TryLoadAsync(ParallelLoadData data, ct)` | Loads multiple heterogeneous assets in parallel. Partial failures are tolerated. Returns `ParallelLoadResult`. |
| `Dispose()` | Releases all handles acquired through this scope. |

### IAssetHandle\<T\>

| Member | Description |
|---|---|
| `Asset` | The loaded `UnityEngine.Object` |
| `IsDisposed` | `true` after `Dispose()` is called |
| `Dispose()` | Decrements the ref count; `Addressables.Release` fires when the count reaches 0 |

Individual handles can be disposed early (before scope disposal) to free memory sooner.

### Parallel Loading

`TryLoadAsync` starts all loads simultaneously and collects individual results. Unlike a standard `WhenAll`, a failure in one request does not cancel the others.

```csharp
var data = new ParallelLoadData();
var iconReq  = data.Add<Sprite>("ui/icon_home");
var bgReq    = data.Add<Texture2D>("ui/background");
var audioReq = data.Add<AudioClip>("audio/bgm_home");

ParallelLoadResult result = await scope.TryLoadAsync(data, ct);

if (result.IsSuccess(iconReq))
    icon.sprite = result.Get(iconReq).Asset;

if (result.IsSuccess(bgReq))
    background.texture = result.Get(bgReq).Asset;

if (!result.IsSuccess(audioReq))
    Debug.LogWarning("BGM failed to load");
```

| Member | Description |
|---|---|
| `ParallelLoadData.Add<T>(string address)` | Registers a load request. Returns an `AssetRequest<T>` token. |
| `ParallelLoadResult.IsSuccess<T>(request)` | Whether the request succeeded. |
| `ParallelLoadResult.Get<T>(request)` | Returns the `IAssetHandle<T>`, or `null` if the request failed. |

### Cancellation Behaviour

Passing a `CancellationToken` cancels the **await** but does not cancel the underlying Addressables operation — because that handle may be shared with other callers. If the token fires, an `OperationCanceledException` is thrown and the reference count for that address is decremented. The handle is not added to the scope.

---

## Animation

**Package:** `com.lisearcheleeds.lighthouse-extends.animation`  
**Dependencies:** None (can be used standalone without the core package)

Components for playing `AnimationClip` In/Out animations via `PlayableGraph`.

### LHTransitionAnimator

A component that connects to the `InAnimation` / `OutAnimation` hooks of scenes and dialogs.

```csharp
// Example: override SceneBase.InAnimation to call it
protected override UniTask InAnimation(ISceneTransitionContext context)
    => transitionAnimator.InAnimation();

protected override UniTask OutAnimation(ISceneTransitionContext context)
    => transitionAnimator.OutAnimation();
```

| Inspector Setting | Description |
|---|---|
| `inAnimationClips` | Array of AnimationClips played in sequence on entry |
| `inDelayMilliSec` | Wait time before the in-animation starts (milliseconds) |
| `outAnimationClips` | Array of AnimationClips played in sequence on leave |
| `outDelayMilliSec` | Wait time before the out-animation starts |

In and Out are mutually exclusive — calling one while the other is playing automatically stops it.

### LHSceneTransitionAnimator / LHSceneTransitionAnimatorManager

Components for managing more complex transition effects spanning multiple scenes. `LHSceneTransitionAnimatorManager` bundles and controls multiple `LHSceneTransitionAnimator` instances.

---

## Language

**Package:** `com.lisearcheleeds.lighthouse-extends.language`  
**Dependencies:** R3

A service for switching the application language.

### Usage

```csharp
// Switch language (registered handlers run in parallel, then CurrentLanguage is updated)
await languageService.SetLanguage("ja", cancellationToken);

// Subscribe to the current language as an R3 ReactiveProperty
languageService.CurrentLanguage.Subscribe(lang => Debug.Log(lang));

// Register a handler called before language changes (used for pre-loading fonts and text)
languageService.RegisterChangeHandler(async (lang, ct) =>
{
    await LoadResourcesForLanguage(lang, ct);
});
```

`SetLanguage` updates `CurrentLanguage` only after all registered handlers complete. This guarantees that font and text resources are fully loaded before being reflected in the UI.

### SupportedLanguageSettings

A ScriptableObject managed in the Inspector. Configure the list of supported language codes and the default language.

```
Create > Lighthouse > Language > Supported Language Settings
```

| Setting | Description |
|---|---|
| `supportedLanguages` | List of valid language codes (e.g. `["ja", "en", "ko", "zh"]`) |
| `defaultLanguage` | Language code used on first launch (e.g. `"en"`) |

### ISupportedLanguageService

A service interface that wraps `SupportedLanguageSettings` for DI injection.

---

## Font

**Package:** `com.lisearcheleeds.lighthouse-extends.font`  
**Dependencies:** `com.lisearcheleeds.lighthouse-extends.language` · TextMeshPro

A service that automatically switches `TMP_FontAsset` based on the current language.

### Usage

```csharp
// Subscribe to the current font
fontService.CurrentFont.Subscribe(font => myText.font = font);

// Directly get the font for a specific language
TMP_FontAsset font = fontService.GetFont("ja");
```

`FontService` registers a change handler with `ILanguageService`, so `CurrentFont` is automatically updated whenever `SetLanguage` is called.

### LanguageFontSettings

A ScriptableObject that maps language codes to `TMP_FontAsset` instances.

```
Create > Lighthouse > Font > Language Font Settings
```

| Setting | Description |
|---|---|
| `entries` | List of language code and FontAsset pairs |
| `defaultFont` | Font used when no matching entry is found for a language code |

---

## TextTable

**Package:** `com.lisearcheleeds.lighthouse-extends.texttable`  
**Dependencies:** `com.lisearcheleeds.lighthouse-extends.language`

A localization service that loads per-language TSV files and resolves text from keys.

### Usage

```csharp
// Get text by passing ITextData ({param} placeholders are resolved)
string text = textTableService.GetText(new TextData("HomeTitle"));

// Example with parameters (TSV: "Retries left: {count}" → runtime: "Retries left: 3")
var data = new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 });
string text = textTableService.GetText(data);
```

### ITextTableLoader

An interface that abstracts the actual TSV loading logic. Implement on the project side and register with DI.

```csharp
public class MyTextTableLoader : ITextTableLoader
{
    public async UniTask<IReadOnlyDictionary<string, string>> LoadAsync(
        string languageCode, CancellationToken ct)
    {
        // Load and parse the TSV from Resources or Addressables and return it
    }
}
```

### TextData.CreateTextData (Editor only)

A helper method for adding new keys while implementing in the editor.

```csharp
// Editor only: after writing this code and running the Lighthouse extension,
// an entry is appended to the TSV and this code is rewritten to new TextData("HomeTitle")
var data = TextData.CreateTextData("Home", "HomeTitle", "Welcome");
```

### Editor Window

Open the window from `Window > Lighthouse > TextTable Editor` to:

- View and edit all keys and translation texts across scenes and Prefabs
- Detect duplicate keys and missing translations with dedicated sub-windows

---

## TextMeshPro

**Package:** `com.lisearcheleeds.lighthouse-extends.textmeshpro`  
**Dependencies:** `com.lisearcheleeds.lighthouse-extends.texttable` · `com.lisearcheleeds.lighthouse-extends.font`

An extended component inheriting from `TextMeshProUGUI` that automatically updates text and font when the language changes.

### Usage

**Setting in the Inspector:**

Simply enter a key string in the **Text Key** field of the `LHTextMeshPro` component. When the language changes, it automatically fetches the text from `TextTableService` and updates.

**Setting in code:**

```csharp
// Set text by passing ITextData
lhText.SetTextData(new TextData("HomeTitle"));

// With parameters
lhText.SetTextData(new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 }));
```

If `TextTableService` is not registered, the key string is displayed as-is. If `FontService` is not registered, automatic font updates are skipped (the manually set font is preserved).

---

## InputLayer

**Package:** `com.lisearcheleeds.lighthouse-extends.inputlayer`  
**Dependencies:** Input System

Provides stack-based input dispatching over an `InputActionAsset`. Events are passed from the top layer downward and stop when consumed or blocked.

### Usage

```csharp
// Push a layer (ActionMap is automatically enabled)
inputLayerController.PushLayer(myLayer, myActionMap);

// Pop the top layer
inputLayerController.PopLayer();

// Pop a specific layer by reference
inputLayerController.PopLayer(myLayer);
```

### Implementing IInputLayer

```csharp
public class HomeInputLayer : IInputLayer
{
    // If true, blocks all event propagation to lower layers
    public bool BlocksAllInput => false;

    public bool OnActionStarted(InputAction.CallbackContext ctx)
    {
        // Return true to consume the event (it won't reach lower layers)
        return false;
    }

    public bool OnActionPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.action.name == "Back")
        {
            // Handle back button
            return true; // consumed
        }
        return false;
    }

    public bool OnActionCanceled(InputAction.CallbackContext ctx) => false;
}
```

| Behavior | Description |
|---|---|
| `return true` | Consumes the event. Does not reach lower layers |
| `return false` | Passes the event through to lower layers |
| `BlocksAllInput = true` | Blocks all propagation to lower layers regardless of return value |

When popping a layer, the `ActionMap` is not disabled if other remaining layers still reference it.

---

## UIComponent

**Package:** `com.lisearcheleeds.lighthouse-extends.uicomponent`  
**Dependencies:** None (can be used standalone without the core package)

A collection of general-purpose UI components.

### LHButton

A button component inheriting from `UnityEngine.UI.Button`. Works with `ExclusiveInputService` to prevent simultaneous taps in multi-touch environments.

- If multiple `LHButton` instances are tapped simultaneously, only the first tap is accepted
- Automatically resets the pressed state when the app goes to the background
- Behaves identically to a standard `Button` if `ExclusiveInputService` is not registered

### ExclusiveInputService

A service that exclusively manages pointer IDs. Used automatically by `LHButton`; direct access is rarely needed.

### LHRaycastTargetObject

A transparent `Graphic` component that only receives raycasts without rendering anything. Use this to define hit areas for buttons without an image.

Requires a `CanvasRenderer` component (attached automatically).

### LHCanvasSceneObject

A component that manages the binding between a Canvas and a scene camera. Used in combination with `CanvasMainSceneBase`.

### DefaultCanvasSceneEditorOnlyObject

A component that manages objects visible only in the editor. Used with components implementing `IEditorOnlyObjectCanvasScene` for debug UI that should be excluded from production builds.
