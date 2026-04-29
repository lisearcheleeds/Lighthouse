# Lighthouse — 功能参考

## 目录

- [场景系统](#场景系统)
  - [SceneGroup](#scenegroup)
  - [TransitionDataBase](#transitiondatabase)
  - [场景基类](#场景基类)
  - [SceneBase 生命周期](#scenebase-生命周期)
  - [过渡类型](#过渡类型)
  - [过渡阶段管线](#过渡阶段管线)
  - [场景拦截](#场景拦截)
  - [返回导航](#返回导航)
  - [SceneCameraManager](#scenecameramanager)
- [ScreenStack](#screenstack)
  - [IScreenStackModule API](#iscreenstackmodule-api)
  - [IScreenStackData](#iscreenstackdata)
  - [ScreenStackBase 生命周期](#screenstackbase-生命周期)
  - [Suspend / Resume](#suspend--resume)
  - [ScreenStackModuleProxy](#screenstackmoduleproxy)
  - [代码生成](#代码生成)
- [Addressable](#addressable)
- [Animation](#animation)
- [Language](#language)
- [Font](#font)
- [TextTable](#texttable)
- [TextMeshPro](#textmeshpro)
- [InputLayer](#inputlayer)
- [UIComponent](#uicomponent)

---

## 场景系统

**包名：** `com.lisearcheleeds.lighthouse`  
**额外依赖包：** VContainer · UniTask

场景系统是框架的核心。以栈的方式管理场景，并通过异步阶段管线执行过渡。

### SceneGroup

`SceneGroup` 是表示同时加载与卸载的场景集合的类。由一个或多个**主场景**及其对应的**模块场景**组合而成。

```csharp
// 示例：SceneGroup 的定义（实现 ISceneGroupProvider 并注册到 DI）
new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
{
    { SceneIds.Home,     new[] { SceneIds.Header, SceneIds.Footer } },
    { SceneIds.Setting,  new[] { SceneIds.Header, SceneIds.Footer } },
    { SceneIds.Detail,   new[] { SceneIds.Header } },
});
```

属于同一 `SceneGroup` 的场景会被缓存。只有在跨组跳转时才会发生场景的加载与卸载，因此可以通过调整组的粒度来平衡性能与内存占用。

---

### TransitionDataBase

指定过渡目标的数据类的基类。每次过渡都需要创建对应的派生类。

```csharp
public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;

    // 将 CanTransition 设为 false 可跳过该过渡
    // 将 CanBackTransition 设为 false 可将其从返回导航栈中排除

    // 若过渡前需要异步处理，可重写此方法
    // 抛出 LHSceneInterceptException 可重定向到其他场景
    public override async UniTask LoadSceneState(
        TransitionDirectionType direction, CancellationToken ct)
    {
        // 示例：检查认证并重定向到登录页面
        if (!await authService.IsLoggedIn(ct))
            throw new LHSceneInterceptException(new LoginTransitionData());
    }
}
```

| 属性 / 方法 | 说明 |
|---|---|
| `MainSceneId` | 过渡目标的主场景 ID |
| `CanTransition` | 设为 `false` 时跳过到此数据的过渡 |
| `CanBackTransition` | 设为 `false` 时从 BackScene 的候选中排除 |
| `LoadSceneState(direction, ct)` | 过渡前的异步处理。拦截也在此处执行 |

---

### 场景基类

#### MainSceneBase\<TTransitionData\>

主场景所挂载的 MonoBehaviour 基类。

```csharp
public class HomeScene : MainSceneBase<HomeTransitionData>
{
    protected override async UniTask OnEnter(
        HomeTransitionData data, ISceneTransitionContext context, CancellationToken ct)
    {
        // 使用过渡数据进行初始化
    }

    // 若需要在离开前保存状态，可重写此方法
    // 也可以抛出 LHSceneInterceptException 重定向到其他场景
    public override UniTask SaveSceneState(CancellationToken ct) => UniTask.CompletedTask;
}
```

#### CanvasMainSceneBase\<TTransitionData\>

适用于拥有 UI Canvas 的主场景的基类。自动控制 `CanvasGroup` 的 Alpha 值（加载时=0，进入时=1，离开时=0）。需要 `CanvasGroup` 和 `SceneCanvasInitializer` 组件。

#### ModuleSceneBase

多个主场景共享的模块场景（Header、Footer 等）的基类。根据 `SceneTransitionDiff` 自动切换游戏对象的 Active 状态。

#### CanvasModuleSceneBase

带有 `CanvasGroup` Alpha 控制的模块场景基类。

---

### SceneBase 生命周期

所有场景基类所拥有的生命周期钩子。

| 方法 | 时机 | 备注 |
|---|---|---|
| `OnSetup()` | 仅首次进入时 | 与 DI 无关的一次性初始化 |
| `OnLoad()` | 场景组加载时 | 默认调用 `gameObject.SetActive(false)` |
| `OnEnter(context, ct)` | 进入时 | 每次都会调用。在此处接收 TransitionData |
| `InAnimation(context)` | 进入动画播放时 | 还有 `OnBeginInAnimation` / `OnCompleteInAnimation` |
| `OutAnimation(context)` | 离开动画播放时 | 还有 `OnBeginOutAnimation` / `OnCompleteOutAnimation` |
| `OnLeave(context, ct)` | 离开时 | |
| `OnUnload()` | 场景组卸载时 | |
| `OnSceneTransitionFinished(diff)` | 过渡完全完成后 | |
| `ResetInAnimation(context)` | 重置为 InAnimation 的初始状态 | Cross 过渡时使用 |

从 `context`（`ISceneTransitionContext`）可以访问 `TransitionData`、`SceneTransitionDiff`、`TransitionDirectionType`、`TransitionType` 等。

---

### 过渡类型

通过 `ISceneManager.TransitionScene` 的 `transitionType` 参数指定。

| 值 | 行为 |
|---|---|
| `Auto`（默认） | 在同一 SceneGroup 内选择 `Cross`，跨组时自动选择 `Exclusive` |
| `Exclusive` | 按顺序执行：OutAnimation → 旧场景离开 → 场景加载/卸载 → 新场景进入 → InAnimation |
| `Cross` | 并行交换旧场景和新场景。用于交叉淡入淡出动画 |

---

### 过渡阶段管线

场景过渡由**阶段（Phase）**和**步骤（Step）**的两层结构组成。

**Exclusive 序列（默认）**

```
SaveCurrentSceneStatePhase  → 调用当前场景的 SaveSceneState
LoadNextSceneStatePhase     → 调用下一场景的 LoadSceneState（可拦截）
OutAnimationPhase           → 并行执行当前场景的 OutAnimation
LeaveScenePhase             → 调用当前场景的 OnLeave
LoadSceneGroupPhase         → 叠加加载下一场景组
UnloadSceneGroupPhase       → 卸载不需要的场景组
EnterScenePhase             → 调用下一场景的 OnEnter
InAnimationPhase            → 并行执行下一场景的 InAnimation
FinishTransitionPhase       → 通知 OnSceneTransitionFinished
CleanupPhase                → 重建摄像机栈
```

**Cross 序列（默认）**

```
SaveCurrentSceneStatePhase
LoadNextSceneStatePhase
LoadSceneGroupPhase
EnterScenePhase
CrossAnimationPhase   → 并行执行旧场景的 Out 和新场景的 In
LeaveScenePhase
UnloadSceneGroupPhase
FinishTransitionPhase
```

通过自定义实现 `ISceneTransitionSequenceProvider` 并注册到 DI，可以自由定制阶段的顺序和内容。

---

### 场景拦截

使用 `LHSceneInterceptException` 可以在过渡过程中重定向到另一个场景。

```csharp
// 在 TransitionDataBase.LoadSceneState 中抛出
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    if (!isLoggedIn)
        throw new LHSceneInterceptException(new LoginTransitionData());
}
```

拦截仅在 `LoadNextSceneStatePhase` 执行期间有效。在其他阶段抛出会导致 `InvalidOperationException`。

---

### 返回导航

调用 `ISceneManager.BackScene()` 会遍历过渡历史栈以确定返回目标。

- `CanTransition = false` 或 `CanBackTransition = false` 的条目会被跳过
- 栈中不存在返回目标时，操作将被忽略
- 无法找到有效目标的非法状态下会抛出 `InvalidOperationException`

```csharp
// 返回上一个场景
await sceneManager.BackScene();

// 指定过渡类型后返回
await sceneManager.BackScene(TransitionType.Cross);
```

另外，通过为 `TransitionScene` 指定 `backMainSceneId` 参数，可以在过渡后将栈截断到指定场景（适用于跳转到特定场景的快捷导航）。

---

### SceneCameraManager

每次过渡时自动重建 URP 摄像机栈。

- 从各场景的 `GetSceneCameraList()` 收集摄像机，按 `SceneCameraType` 和 `CameraDefaultDepth` 排序
- 第一个摄像机设为 **Base**，其余设为 **Overlay**
- UI 摄像机（Canvas 使用）始终作为最后一个 Overlay 添加

若要控制摄像机顺序，实现 `ISceneCamera` 并返回对应的 `SceneCameraType` 和 `CameraDefaultDepth` 即可。

---

## ScreenStack

**包名：** `com.lisearcheleeds.lighthouse-extends.screenstack`  
**依赖包：** `com.lisearcheleeds.lighthouse`（核心）

以栈的方式管理对话框和遮罩层的模块。Open / Close 操作被加入队列，在前一个操作完成后依次执行。

### IScreenStackModule API

```csharp
// 仅将数据加入队列（不立即打开）
await screenStackModule.Enqueue(new SampleDialogData());

// 打开队列最前端（需要在之前执行 Enqueue）
await screenStackModule.Open();

// 一次性完成 Enqueue 和 Open（通常使用这个）
await screenStackModule.Open(new SampleDialogData());

// 关闭最前端的屏幕
await screenStackModule.Close();

// 关闭与特定数据对应的屏幕
await screenStackModule.Close(myDialogData);

// 关闭当前场景栈中的所有屏幕
await screenStackModule.ClearCurrentAll();

// 清除跨场景的整个栈
await screenStackModule.ClearAll();
```

---

### IScreenStackData

各屏幕数据类所实现的接口。

| 属性 | 类型 | 说明 |
|---|---|---|
| `IsSystem` | `bool` | 设为 `true` 时放置在系统 Canvas 层（显示在普通 UI 上方） |
| `IsOverlayOpen` | `bool` | 设为 `true` 时不对前一个屏幕播放 OutAnimation，直接叠加显示（覆盖层显示） |

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

### ScreenStackBase 生命周期

挂载在各对话框 Prefab 上的 MonoBehaviour 基类。

| 方法 | 时机 |
|---|---|
| `OnInitialize()` | Prefab 实例化后、显示前 |
| `OnEnter(isResume)` | 成为栈顶时。`isResume=true` 表示因另一个屏幕关闭而返回 |
| `PlayInAnimation()` | 进入动画。通过 `ResetInAnimation()` 初始化 |
| `OnLeave()` | 从栈中移除前 |
| `PlayOutAnimation()` | 离开动画。通过 `ResetOutAnimation()` 初始化 |
| `Dispose()` | 关闭后销毁 GameObject |

---

### Suspend / Resume

继承了 `ScreenStackModuleSceneBase` 的场景可以在场景过渡中保持栈的状态。

- **Forward 过渡（前进）**: 离开前 `Suspend`（保存）当前栈
- **Back 过渡（返回）**: 进入时 `Resume`（恢复）已保存的栈

这样可以实现「打开对话框后跳转到其他场景，返回时对话框恢复显示」的效果。

---

### ScreenStackModuleProxy

用于跨 DI 作用域访问栈 API 的代理类。

- `ScreenStackModuleProxy`（场景作用域）持有对 `ScreenStackModule`（模块作用域）的引用
- 从场景作用域以 `IScreenStackModule` 方式注入的一方无需关心作用域差异即可使用

---

### 代码生成

可以从编辑器菜单自动生成对话框的样板代码。

- **ScreenStackDialogScriptGeneratorWindow**: 从模板生成对话框的 View / Data 类
- **ScreenStackEntityFactoryGenerator**: 自动生成 `IScreenStackEntityFactory` 的 switch 分支

---

## Addressable

**包名：** `com.lisearcheleeds.lighthouse-extends.addressable`  
**依赖包：** UniTask · Unity Addressables

引用计数式 Addressables 封装，支持基于作用域的资源生命周期管理和并行加载。按地址追踪引用计数；多个作用域共享同一地址时复用同一 `AsyncOperationHandle`，仅在最后一个句柄被释放时才调用 `Addressables.Release`。

### IAssetManager

将 `AssetManager` 注册到 DI 并以 `IAssetManager` 注入。调用 `CreateScope()` 获取用于加载资源的 `IAssetScope`。

```csharp
// VContainer 示例
builder.Register<AssetManager>(Lifetime.Singleton).As<IAssetManager>();
```

### IAssetScope

每个场景（或每个逻辑加载上下文）创建一个作用域。释放作用域时，其持有的所有句柄将被一并释放。

```csharp
IAssetScope scope = assetManager.CreateScope();

// 按地址加载单个资源
IAssetHandle<Sprite> handle = await scope.LoadAsync<Sprite>("ui/icon_home", ct);
icon.sprite = handle.Asset;

// 按地址列表顺序加载多个资源
IReadOnlyList<IAssetHandle<Sprite>> handles =
    await scope.LoadAsync<Sprite>(new[] { "ui/icon_a", "ui/icon_b" }, ct);

// 加载匹配标签的所有资源
IReadOnlyList<AudioClip> clips = await scope.LoadByLabelAsync<AudioClip>("bgm", ct);

// 释放此作用域持有的所有句柄
scope.Dispose();
```

| 方法 | 说明 |
|---|---|
| `LoadAsync<T>(string address, ct)` | 按地址加载单个资源，返回 `IAssetHandle<T>` |
| `LoadAsync<T>(IReadOnlyList<string> addresses, ct)` | 顺序加载多个资源，返回 `IReadOnlyList<IAssetHandle<T>>` |
| `LoadByLabelAsync<T>(string label, ct)` | 加载匹配标签的所有资源，返回 `IReadOnlyList<T>` |
| `TryLoadAsync(ParallelLoadData data, ct)` | 并行加载多种类型资源，允许部分失败，返回 `ParallelLoadResult` |
| `Dispose()` | 释放此作用域获取的所有句柄 |

### IAssetHandle\<T\>

| 成员 | 说明 |
|---|---|
| `Asset` | 已加载的 `UnityEngine.Object` |
| `IsDisposed` | 调用 `Dispose()` 后为 `true` |
| `Dispose()` | 递减引用计数；计数归零时调用 `Addressables.Release` |

可在作用域释放前提前 Dispose 单个句柄以更早释放内存。

### 并行加载

`TryLoadAsync` 同时启动所有加载并汇总各自结果。与普通 `WhenAll` 不同，其中一个请求失败不会取消其他请求。

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
    Debug.LogWarning("BGM 加载失败");
```

| 成员 | 说明 |
|---|---|
| `ParallelLoadData.Add<T>(string address)` | 注册加载请求，返回 `AssetRequest<T>` 令牌 |
| `ParallelLoadResult.IsSuccess<T>(request)` | 请求是否成功 |
| `ParallelLoadResult.Get<T>(request)` | 成功返回 `IAssetHandle<T>`，失败返回 `null` |

### 取消行为

传入 `CancellationToken` 会取消 **await**，但不会取消底层 Addressables 操作——因为该句柄可能被其他调用方共享。令牌触发后抛出 `OperationCanceledException`，并递减该地址的引用计数。句柄不会被添加到作用域中。

---

## Animation

**包名：** `com.lisearcheleeds.lighthouse-extends.animation`  
**依赖包：** 无（无需核心包，可单独使用）

通过 `PlayableGraph` 播放 `AnimationClip` 的 In/Out 动画组件群。

### LHTransitionAnimator

连接到场景或对话框的 InAnimation / OutAnimation 钩子的组件。

```csharp
// 示例：重写 SceneBase.InAnimation 并调用
protected override UniTask InAnimation(ISceneTransitionContext context)
    => transitionAnimator.InAnimation();

protected override UniTask OutAnimation(ISceneTransitionContext context)
    => transitionAnimator.OutAnimation();
```

| Inspector 设置 | 说明 |
|---|---|
| `inAnimationClips` | 进入时依次播放的 AnimationClip 数组 |
| `inDelayMilliSec` | 进入动画开始前的等待时间（毫秒） |
| `outAnimationClips` | 离开时依次播放的 AnimationClip 数组 |
| `outDelayMilliSec` | 离开动画开始前的等待时间 |

In 和 Out 采用互斥控制，一方播放时调用另一方会自动停止当前播放。

### LHSceneTransitionAnimator / LHSceneTransitionAnimatorManager

管理跨多个场景的复杂过渡演出的组件。`LHSceneTransitionAnimatorManager` 统一控制多个 `LHSceneTransitionAnimator`。

---

## Language

**包名：** `com.lisearcheleeds.lighthouse-extends.language`  
**依赖包：** R3

切换应用语言的服务。

### 使用方法

```csharp
// 切换语言（注册的处理器并行执行完成后，CurrentLanguage 才会更新）
await languageService.SetLanguage("ja", cancellationToken);

// 以 R3 的 ReactiveProperty 订阅当前语言
languageService.CurrentLanguage.Subscribe(lang => Debug.Log(lang));

// 注册语言切换前调用的处理器（用于预加载字体和文本）
languageService.RegisterChangeHandler(async (lang, ct) =>
{
    await LoadResourcesForLanguage(lang, ct);
});
```

`SetLanguage` 会在所有注册处理器完成后才更新 `CurrentLanguage`，从而保证字体和文本资源加载完毕后再反映到 UI。

### SupportedLanguageSettings

在 Inspector 中管理的 ScriptableObject。配置支持的语言代码列表和默认语言。

```
Create > Lighthouse > Language > Supported Language Settings
```

| 设置 | 说明 |
|---|---|
| `supportedLanguages` | 有效语言代码列表（例：`["ja", "en", "ko", "zh"]`） |
| `defaultLanguage` | 初次启动时使用的语言代码（例：`"en"`） |

### ISupportedLanguageService

封装 `SupportedLanguageSettings` 并可通过 DI 使用的服务接口。

---

## Font

**包名：** `com.lisearcheleeds.lighthouse-extends.font`  
**依赖包：** `com.lisearcheleeds.lighthouse-extends.language` · TextMeshPro

根据语言自动切换 `TMP_FontAsset` 的服务。

### 使用方法

```csharp
// 订阅当前字体
fontService.CurrentFont.Subscribe(font => myText.font = font);

// 直接获取特定语言的字体
TMP_FontAsset font = fontService.GetFont("ja");
```

`FontService` 向 `ILanguageService` 注册了变更处理器，因此每次调用 `SetLanguage` 时 `CurrentFont` 会自动更新。

### LanguageFontSettings

管理语言代码与 `TMP_FontAsset` 映射关系的 ScriptableObject。

```
Create > Lighthouse > Font > Language Font Settings
```

| 设置 | 说明 |
|---|---|
| `entries` | 语言代码与 FontAsset 配对的列表 |
| `defaultFont` | 找不到匹配条目时使用的字体 |

---

## TextTable

**包名：** `com.lisearcheleeds.lighthouse-extends.texttable`  
**依赖包：** `com.lisearcheleeds.lighthouse-extends.language`

加载各语言 TSV 文件并通过键解析文本的本地化服务。

### 使用方法

```csharp
// 传入 ITextData 获取文本（{param} 占位符也会被解析）
string text = textTableService.GetText(new TextData("HomeTitle"));

// 含参数的示例（TSV: "剩余{count}次" → 运行时: "剩余3次"）
var data = new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 });
string text = textTableService.GetText(data);
```

### ITextTableLoader

抽象 TSV 实际加载处理的接口。在项目侧实现并注册到 DI。

```csharp
public class MyTextTableLoader : ITextTableLoader
{
    public async UniTask<IReadOnlyDictionary<string, string>> LoadAsync(
        string languageCode, CancellationToken ct)
    {
        // 从 Resources 或 Addressables 加载并解析 TSV 后返回
    }
}
```

### TextData.CreateTextData（仅限编辑器）

在编辑器中新增键时使用的辅助方法。

```csharp
// 仅限编辑器：编写此代码并运行 Lighthouse 扩展功能后，
// TSV 中会追加条目，此代码本身也会被改写为 new TextData("HomeTitle")
var data = TextData.CreateTextData("Home", "HomeTitle", "欢迎");
```

### 编辑器窗口

从 `Window > Lighthouse > TextTable Editor` 打开窗口后，可以：

- 跨场景和 Prefab 浏览并编辑所有键与翻译文本
- 还提供重复键与未翻译键检测的子窗口

---

## TextMeshPro

**包名：** `com.lisearcheleeds.lighthouse-extends.textmeshpro`  
**依赖包：** `com.lisearcheleeds.lighthouse-extends.texttable` · `com.lisearcheleeds.lighthouse-extends.font`

继承自 `TextMeshProUGUI` 的扩展组件，在语言切换时自动更新文本和字体。

### 使用方法

**在 Inspector 中设置：**

只需在 `LHTextMeshPro` 组件的 **Text Key** 字段中填写键字符串。语言切换时会自动从 `TextTableService` 获取文本并更新。

**在代码中设置：**

```csharp
// 传入 ITextData 设置文本
lhText.SetTextData(new TextData("HomeTitle"));

// 含参数
lhText.SetTextData(new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 }));
```

未注册 `TextTableService` 时直接显示键字符串。未注册 `FontService` 时跳过字体自动更新（手动设置的字体保持不变）。

---

## InputLayer

**包名：** `com.lisearcheleeds.lighthouse-extends.inputlayer`  
**依赖包：** Input System

在 `InputActionAsset` 上提供基于栈的输入分发。事件从最顶层开始依次传递，被消费或阻断后不再传递到下方。

### 使用方法

```csharp
// 推入层（ActionMap 自动启用）
inputLayerController.PushLayer(myLayer, myActionMap);

// 弹出最顶层
inputLayerController.PopLayer();

// 指定层并弹出
inputLayerController.PopLayer(myLayer);
```

### 实现 IInputLayer

```csharp
public class HomeInputLayer : IInputLayer
{
    // 设为 true 则阻断所有向下层的事件传播
    public bool BlocksAllInput => false;

    public bool OnActionStarted(InputAction.CallbackContext ctx)
    {
        // 返回 true 则消费该事件（不传递到下层）
        return false;
    }

    public bool OnActionPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.action.name == "Back")
        {
            // 处理返回按钮
            return true; // 消费
        }
        return false;
    }

    public bool OnActionCanceled(InputAction.CallbackContext ctx) => false;
}
```

| 行为 | 说明 |
|---|---|
| `return true` | 消费事件，不传递到下层 |
| `return false` | 透传事件，传递到下层 |
| `BlocksAllInput = true` | 无论返回值如何，阻断所有向下层的传播 |

若存在其他仍引用同一 `ActionMap` の层，弹出时不会禁用该 `ActionMap`。

---

## UIComponent

**包名：** `com.lisearcheleeds.lighthouse-extends.uicomponent`  
**依赖包：** 无（无需核心包，可单独使用）

与 UI 相关的通用组件群。

### LHButton

继承自 `UnityEngine.UI.Button` 的按钮组件。与 `ExclusiveInputService` 协同工作，防止多点触控环境中的同时点击。

- 多个 `LHButton` 同时被点击时，仅第一个点击有效
- 应用进入后台时自动重置按下状态
- 未注册 `ExclusiveInputService` 时与普通 `Button` 行为相同

### ExclusiveInputService

独占管理指针 ID 的服务。由 `LHButton` 自动使用，通常无需直接操作。

### LHRaycastTargetObject

不进行渲染、仅接受射线检测的透明 `Graphic` 组件。用于在没有图片的情况下定义按钮等的点击区域。

需要 `CanvasRenderer` 组件（自动挂载）。

### LHCanvasSceneObject

管理 Canvas 与场景摄像机绑定关系的组件。与 `CanvasMainSceneBase` 配合使用。

### DefaultCanvasSceneEditorOnlyObject

管理仅在编辑器中显示的对象的组件。与实现了 `IEditorOnlyObjectCanvasScene` 的组件配合使用，适用于不希望包含在正式构建中的调试 UI 等。
