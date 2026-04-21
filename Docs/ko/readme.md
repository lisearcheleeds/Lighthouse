# Lighthouse — 기능 레퍼런스

## 목차

- [씬 시스템](#씬-시스템)
  - [SceneGroup](#scenegroup)
  - [TransitionDataBase](#transitiondatabase)
  - [씬 기반 클래스](#씬-기반-클래스)
  - [SceneBase 라이프사이클](#scenebase-라이프사이클)
  - [전환 타입](#전환-타입)
  - [전환 페이즈 파이프라인](#전환-페이즈-파이프라인)
  - [씬 인터셉트](#씬-인터셉트)
  - [뒤로 가기 내비게이션](#뒤로-가기-내비게이션)
  - [SceneCameraManager](#scenecameramanager)
- [ScreenStack](#screenstack)
  - [IScreenStackModule API](#iscreenstackmodule-api)
  - [IScreenStackData](#iscreenstackdata)
  - [ScreenStackBase 라이프사이클](#screenstackbase-라이프사이클)
  - [Suspend / Resume](#suspend--resume)
  - [ScreenStackModuleProxy](#screenstackmoduleproxy)
  - [코드 생성](#코드-생성)
- [Animation](#animation)
- [Language](#language)
- [Font](#font)
- [TextTable](#texttable)
- [TextMeshPro](#textmeshpro)
- [InputLayer](#inputlayer)
- [UIComponent](#uicomponent)

---

## 씬 시스템

**패키지:** `com.lisearcheleeds.lighthouse`  
**추가 의존 패키지:** VContainer · UniTask

씬 시스템은 프레임워크의 핵심입니다. 씬을 스택으로 관리하고 전환을 비동기 페이즈 파이프라인으로 실행합니다.

### SceneGroup

`SceneGroup`은 동시에 로드·언로드되는 씬의 묶음을 나타내는 클래스입니다. 하나 이상의 **메인 씬**과 각각에 대응하는 **모듈 씬**의 조합으로 구성됩니다.

```csharp
// 예시: SceneGroup 정의 (ISceneGroupProvider를 구현하여 DI에 등록)
new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
{
    { SceneIds.Home,     new[] { SceneIds.Header, SceneIds.Footer } },
    { SceneIds.Setting,  new[] { SceneIds.Header, SceneIds.Footer } },
    { SceneIds.Detail,   new[] { SceneIds.Header } },
});
```

같은 `SceneGroup`에 속하는 씬은 캐시됩니다. 그룹을 넘나드는 전환 시에만 씬의 로드·언로드가 발생하므로 그룹의 단위에 따라 성능과 메모리의 균형을 조절할 수 있습니다.

---

### TransitionDataBase

전환 대상을 지정하는 데이터 클래스의 기반 클래스입니다. 전환마다 파생 클래스를 작성합니다.

```csharp
public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;

    // CanTransition = false 로 하면 전환 자체를 건너뜀
    // CanBackTransition = false 로 하면 뒤로 가기 스택 대상에서 제외됨

    // 전환 전에 비동기 처리가 필요한 경우 오버라이드
    // LHSceneInterceptException 을 throw하면 다른 씬으로 리다이렉트 가능
    public override async UniTask LoadSceneState(
        TransitionDirectionType direction, CancellationToken ct)
    {
        // 예시: 인증 확인 후 로그인 화면으로 리다이렉트
        if (!await authService.IsLoggedIn(ct))
            throw new LHSceneInterceptException(new LoginTransitionData());
    }
}
```

| 프로퍼티 / 메서드 | 설명 |
|---|---|
| `MainSceneId` | 전환 대상 메인 씬 ID |
| `CanTransition` | `false`로 하면 이 데이터로의 전환을 건너뜀 |
| `CanBackTransition` | `false`로 하면 BackScene의 후보에서 제외 |
| `LoadSceneState(direction, ct)` | 전환 직전의 비동기 처리. 인터셉트도 여기서 수행 |

---

### 씬 기반 클래스

#### MainSceneBase\<TTransitionData\>

메인 씬에 부여하는 MonoBehaviour의 기반 클래스입니다.

```csharp
public class HomeScene : MainSceneBase<HomeTransitionData>
{
    protected override async UniTask OnEnter(
        HomeTransitionData data, ISceneTransitionContext context, CancellationToken ct)
    {
        // 전환 데이터를 사용한 초기화
    }

    // 이탈 전에 상태를 저장하려면 오버라이드
    // LHSceneInterceptException을 throw하여 다른 씬으로 리다이렉트도 가능
    public override UniTask SaveSceneState(CancellationToken ct) => UniTask.CompletedTask;
}
```

#### CanvasMainSceneBase\<TTransitionData\>

UI Canvas를 가진 메인 씬을 위한 기반 클래스입니다. `CanvasGroup`의 알파값을 자동 제어합니다(로드 시=0, 진입 시=1, 이탈 시=0). `CanvasGroup`과 `SceneCanvasInitializer` 컴포넌트가 필요합니다.

#### ModuleSceneBase

여러 메인 씬에서 공유되는 모듈 씬(헤더·푸터 등)의 기반 클래스입니다. `SceneTransitionDiff`를 기반으로 게임오브젝트의 Active를 자동으로 전환합니다.

#### CanvasModuleSceneBase

`CanvasGroup` 알파 제어가 포함된 모듈 씬 기반 클래스입니다.

---

### SceneBase 라이프사이클

모든 씬 기반 클래스가 가진 라이프사이클 훅입니다.

| 메서드 | 타이밍 | 비고 |
|---|---|---|
| `OnSetup()` | 최초 진입 시만 | DI 비의존의 한 번만 실행되는 초기화 |
| `OnLoad()` | 씬 그룹 로드 시 | 기본적으로 `gameObject.SetActive(false)` 호출 |
| `OnEnter(context, ct)` | 진입 시 | 매번 호출됨. TransitionData는 여기서 수신 |
| `InAnimation(context)` | 진입 애니메이션 재생 시 | `OnBeginInAnimation` / `OnCompleteInAnimation`도 있음 |
| `OutAnimation(context)` | 이탈 애니메이션 재생 시 | `OnBeginOutAnimation` / `OnCompleteOutAnimation`도 있음 |
| `OnLeave(context, ct)` | 이탈 시 | |
| `OnUnload()` | 씬 그룹 언로드 시 | |
| `OnSceneTransitionFinished(diff)` | 전환이 완전히 완료된 후 | |
| `ResetInAnimation(context)` | InAnimation의 초기 상태로 리셋 | Cross 전환 시 사용 |

`context` (`ISceneTransitionContext`)에서는 `TransitionData`·`SceneTransitionDiff`·`TransitionDirectionType`·`TransitionType` 등을 참조할 수 있습니다.

---

### 전환 타입

`ISceneManager.TransitionScene`의 `transitionType` 인수로 지정합니다.

| 값 | 동작 |
|---|---|
| `Auto` (기본값) | 동일 SceneGroup 내라면 `Cross`, 그룹을 넘으면 `Exclusive`를 자동 선택 |
| `Exclusive` | OutAnimation → 이전 씬 이탈 → 씬 로드/언로드 → 새 씬 진입 → InAnimation 순서로 실행 |
| `Cross` | 이전 씬과 새 씬을 병렬로 교체. 크로스페이드 애니메이션을 사용할 때 활용 |

---

### 전환 페이즈 파이프라인

씬 전환은 **페이즈**와 **스텝**의 2계층 구조로 이루어집니다.

**Exclusive 시퀀스 (기본값)**

```
SaveCurrentSceneStatePhase  → 현재 씬의 SaveSceneState 호출
LoadNextSceneStatePhase     → 다음 씬의 LoadSceneState 호출 (인터셉트 가능)
OutAnimationPhase           → 현재 씬의 OutAnimation 병렬 실행
LeaveScenePhase             → 현재 씬의 OnLeave 호출
LoadSceneGroupPhase         → 다음 씬 그룹을 어디티브 로드
UnloadSceneGroupPhase       → 불필요한 씬 그룹 언로드
EnterScenePhase             → 다음 씬의 OnEnter 호출
InAnimationPhase            → 다음 씬의 InAnimation 병렬 실행
FinishTransitionPhase       → OnSceneTransitionFinished 알림
CleanupPhase                → 카메라 스택 재구성
```

**Cross 시퀀스 (기본값)**

```
SaveCurrentSceneStatePhase
LoadNextSceneStatePhase
LoadSceneGroupPhase
EnterScenePhase
CrossAnimationPhase   → 이전 씬의 Out과 새 씬의 In을 병렬 실행
LeaveScenePhase
UnloadSceneGroupPhase
FinishTransitionPhase
```

`ISceneTransitionSequenceProvider`를 직접 구현하여 DI에 등록하면 페이즈의 순서·내용을 자유롭게 커스터마이즈할 수 있습니다.

---

### 씬 인터셉트

`LHSceneInterceptException`을 사용하면 전환 중에 다른 씬으로 리다이렉트할 수 있습니다.

```csharp
// TransitionDataBase.LoadSceneState 내에서 throw
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    if (!isLoggedIn)
        throw new LHSceneInterceptException(new LoginTransitionData());
}
```

인터셉트는 `LoadNextSceneStatePhase` 실행 중에만 유효합니다. 다른 페이즈에서 throw하면 `InvalidOperationException`이 됩니다.

---

### 뒤로 가기 내비게이션

`ISceneManager.BackScene()`을 호출하면 전환 이력 스택을 거슬러 올라가 돌아갈 대상을 결정합니다.

- `CanTransition = false` 또는 `CanBackTransition = false`인 항목은 건너뜁니다
- 스택에 돌아갈 대상이 없으면 동작을 무시합니다
- 유효한 대상을 찾을 수 없는 비정상 상태에서는 `InvalidOperationException`을 throw합니다

```csharp
// 이전 씬으로 돌아가기
await sceneManager.BackScene();

// 전환 타입을 지정하여 돌아가기
await sceneManager.BackScene(TransitionType.Cross);
```

또한 `TransitionScene`의 `backMainSceneId` 인수를 지정하면 전환 후 스택을 지정된 씬까지 잘라낼 수 있습니다(특정 씬으로의 단축 전환에 유용).

---

### SceneCameraManager

전환 시마다 URP 카메라 스택을 자동으로 재구성합니다.

- 각 씬의 `GetSceneCameraList()`에서 수집한 카메라를 `SceneCameraType`과 `CameraDefaultDepth`로 정렬
- 첫 번째 카메라를 **Base**, 나머지를 **Overlay**로 설정
- UI용 카메라(Canvas가 사용)는 항상 마지막 Overlay로 추가

카메라 순서를 제어하려면 `ISceneCamera`를 구현하고 `SceneCameraType`과 `CameraDefaultDepth`를 반환하면 됩니다.

---

## ScreenStack

**패키지:** `com.lisearcheleeds.lighthouse-extends.screenstack`  
**의존 패키지:** `com.lisearcheleeds.lighthouse` (코어)

다이얼로그나 오버레이를 스택으로 관리하는 모듈입니다. Open / Close 작업은 큐에 쌓이고 이전 작업이 완료된 후 다음이 실행됩니다.

### IScreenStackModule API

```csharp
// 데이터를 큐에만 추가 (즉시 열지 않음)
await screenStackModule.Enqueue(new SampleDialogData());

// 큐의 맨 앞을 열기 (직전에 Enqueue 필요)
await screenStackModule.Open();

// Enqueue와 Open을 한 번에 수행 (일반적으로 이것을 사용)
await screenStackModule.Open(new SampleDialogData());

// 가장 앞의 스크린 닫기
await screenStackModule.Close();

// 특정 데이터에 대응하는 스크린 닫기
await screenStackModule.Close(myDialogData);

// 현재 씬의 스택을 모두 닫기
await screenStackModule.ClearCurrentAll();

// 씬을 넘어선 전체 스택 초기화
await screenStackModule.ClearAll();
```

---

### IScreenStackData

각 스크린의 데이터 클래스에 구현하는 인터페이스입니다.

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `IsSystem` | `bool` | `true`로 하면 시스템 Canvas 레이어에 배치 (일반 UI 위에 표시) |
| `IsOverlayOpen` | `bool` | `true`로 하면 이전 스크린의 OutAnimation 없이 위에 열기 (오버레이 표시) |

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

### ScreenStackBase 라이프사이클

각 다이얼로그 Prefab에 부여하는 MonoBehaviour의 기반 클래스입니다.

| 메서드 | 타이밍 |
|---|---|
| `OnInitialize()` | Prefab 인스턴스화 직후, 표시 전 |
| `OnEnter(isResume)` | 스택의 맨 앞이 되었을 때. `isResume=true`는 다른 스크린이 닫혀서 돌아온 때 |
| `PlayInAnimation()` | 진입 애니메이션. `ResetInAnimation()`으로 초기화 |
| `OnLeave()` | 스택에서 제거되기 직전 |
| `PlayOutAnimation()` | 이탈 애니메이션. `ResetOutAnimation()`으로 초기화 |
| `Dispose()` | 닫힌 후 GameObject 파괴 |

---

### Suspend / Resume

`ScreenStackModuleSceneBase`를 상속한 씬을 사용하면 씬 전환을 넘어서 스택의 상태가 유지됩니다.

- **Forward 전환(전진)**: 현재 스택을 `Suspend`(저장)하고 이탈
- **Back 전환(뒤로)**: 진입 시 저장된 스택을 `Resume`(복원)

이를 통해 「다이얼로그를 열고 다른 씬으로 이동한 뒤 돌아오면 다이얼로그가 복원되는」 동작을 구현할 수 있습니다.

---

### ScreenStackModuleProxy

DI 스코프를 넘어 스택 API에 접근하기 위한 프록시 클래스입니다.

- `ScreenStackModuleProxy`(씬 스코프)가 `ScreenStackModule`(모듈 스코프)에 대한 참조를 보유
- 씬 스코프에서 `IScreenStackModule`로 주입받은 쪽은 스코프 차이를 의식하지 않고 사용 가능

---

### 코드 생성

에디터 메뉴에서 다이얼로그의 보일러플레이트 코드를 자동 생성할 수 있습니다.

- **ScreenStackDialogScriptGeneratorWindow**: 템플릿에서 다이얼로그의 View / Data 클래스 생성
- **ScreenStackEntityFactoryGenerator**: `IScreenStackEntityFactory`의 switch 분기를 자동 생성

---

## Animation

**패키지:** `com.lisearcheleeds.lighthouse-extends.animation`  
**의존 패키지:** 없음 (코어 패키지 없이 단독 사용 가능)

`PlayableGraph`를 통해 `AnimationClip`을 재생하는 In/Out 애니메이션 컴포넌트 군입니다.

### LHTransitionAnimator

씬이나 다이얼로그의 InAnimation / OutAnimation 훅에 연결하여 사용하는 컴포넌트입니다.

```csharp
// 예시: SceneBase.InAnimation을 오버라이드하여 호출
protected override UniTask InAnimation(ISceneTransitionContext context)
    => transitionAnimator.InAnimation();

protected override UniTask OutAnimation(ISceneTransitionContext context)
    => transitionAnimator.OutAnimation();
```

| 인스펙터 설정 | 설명 |
|---|---|
| `inAnimationClips` | 진입 시 순서대로 재생할 AnimationClip 배열 |
| `inDelayMilliSec` | 진입 애니메이션 시작 전 대기 시간 (밀리초) |
| `outAnimationClips` | 이탈 시 순서대로 재생할 AnimationClip 배열 |
| `outDelayMilliSec` | 이탈 애니메이션 시작 전 대기 시간 |

In과 Out은 배타 제어되어 있어 하나가 재생 중에 다른 하나를 호출하면 자동으로 정지됩니다.

### LHSceneTransitionAnimator / LHSceneTransitionAnimatorManager

여러 씬에 걸친 복잡한 트랜지션 연출을 관리하는 컴포넌트입니다. `LHSceneTransitionAnimatorManager`가 여러 `LHSceneTransitionAnimator`를 묶어서 제어합니다.

---

## Language

**패키지:** `com.lisearcheleeds.lighthouse-extends.language`  
**의존 패키지:** R3

앱의 언어를 전환하는 서비스입니다.

### 사용 방법

```csharp
// 언어 전환 (등록된 핸들러가 병렬 실행된 후 CurrentLanguage 업데이트)
await languageService.SetLanguage("ja", cancellationToken);

// 현재 언어를 R3의 ReactiveProperty로 구독
languageService.CurrentLanguage.Subscribe(lang => Debug.Log(lang));

// 언어 변경 전에 호출될 핸들러 등록 (폰트나 텍스트의 사전 로드에 사용)
languageService.RegisterChangeHandler(async (lang, ct) =>
{
    await LoadResourcesForLanguage(lang, ct);
});
```

`SetLanguage`는 등록된 모든 핸들러가 완료된 후에 `CurrentLanguage`를 업데이트합니다. 이를 통해 폰트와 텍스트 리소스의 로드가 완료된 후 UI에 반영되는 것이 보장됩니다.

### SupportedLanguageSettings

인스펙터에서 관리하는 ScriptableObject입니다. 지원 언어 코드 목록과 기본 언어를 설정합니다.

```
Create > Lighthouse > Language > Supported Language Settings
```

| 설정 | 설명 |
|---|---|
| `supportedLanguages` | 유효한 언어 코드 목록 (예: `["ja", "en", "ko", "zh"]`) |
| `defaultLanguage` | 초기 실행 시 사용할 언어 코드 (예: `"en"`) |

### ISupportedLanguageService

`SupportedLanguageSettings`를 래핑하여 DI를 통해 사용할 수 있는 서비스 인터페이스입니다.

---

## Font

**패키지:** `com.lisearcheleeds.lighthouse-extends.font`  
**의존 패키지:** `com.lisearcheleeds.lighthouse-extends.language` · TextMeshPro

언어에 따라 `TMP_FontAsset`을 자동으로 전환하는 서비스입니다.

### 사용 방법

```csharp
// 현재 폰트 구독
fontService.CurrentFont.Subscribe(font => myText.font = font);

// 특정 언어의 폰트를 직접 가져오기
TMP_FontAsset font = fontService.GetFont("ja");
```

`FontService`는 `ILanguageService`에 변경 핸들러를 등록하므로 `SetLanguage`가 호출되면 자동으로 `CurrentFont`가 업데이트됩니다.

### LanguageFontSettings

언어 코드와 `TMP_FontAsset`의 매핑을 관리하는 ScriptableObject입니다.

```
Create > Lighthouse > Font > Language Font Settings
```

| 설정 | 설명 |
|---|---|
| `entries` | 언어 코드와 FontAsset 쌍의 목록 |
| `defaultFont` | 해당 항목이 없는 언어 코드에서 사용되는 폰트 |

---

## TextTable

**패키지:** `com.lisearcheleeds.lighthouse-extends.texttable`  
**의존 패키지:** `com.lisearcheleeds.lighthouse-extends.language`

언어별 TSV 파일을 로드하고 키로 텍스트를 해결하는 로컬라이제이션 서비스입니다.

### 사용 방법

```csharp
// ITextData를 전달하여 텍스트 취득 ({param} 플레이스홀더도 해결됨)
string text = textTableService.GetText(new TextData("HomeTitle"));

// 파라미터 포함 예시 (TSV: "남은 횟수 {count}회" → 런타임: "남은 횟수 3회")
var data = new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 });
string text = textTableService.GetText(data);
```

### ITextTableLoader

TSV의 실제 로드 처리를 추상화하는 인터페이스입니다. 프로젝트 쪽에서 구현하여 DI에 등록합니다.

```csharp
public class MyTextTableLoader : ITextTableLoader
{
    public async UniTask<IReadOnlyDictionary<string, string>> LoadAsync(
        string languageCode, CancellationToken ct)
    {
        // Resources나 Addressables에서 TSV를 로드하여 파싱 후 반환
    }
}
```

### TextData.CreateTextData (에디터 전용)

에디터에서 키를 새로 추가하면서 구현할 때의 보조 메서드입니다.

```csharp
// 에디터 전용: 이 코드를 작성하고 Lighthouse 확장 기능을 실행하면
// TSV에 항목이 추가되고 이 코드 자체가 new TextData("HomeTitle")로 재작성됨
var data = TextData.CreateTextData("Home", "HomeTitle", "환영합니다");
```

### 에디터 윈도우

`Window > Lighthouse > TextTable Editor`에서 윈도우를 열면:

- 씬이나 Prefab을 넘어서 모든 키와 번역 텍스트를 일람·편집 가능
- 중복 키나 미번역 키 검출 윈도우도 제공

---

## TextMeshPro

**패키지:** `com.lisearcheleeds.lighthouse-extends.textmeshpro`  
**의존 패키지:** `com.lisearcheleeds.lighthouse-extends.texttable` · `com.lisearcheleeds.lighthouse-extends.font`

`TextMeshProUGUI`를 상속한 확장 컴포넌트로 언어 전환에 따라 텍스트와 폰트를 자동 업데이트합니다.

### 사용 방법

**인스펙터에서 설정하는 경우:**

`LHTextMeshPro` 컴포넌트의 **Text Key** 필드에 키 문자열을 입력하면 됩니다. 언어가 바뀌면 `TextTableService`에서 텍스트를 가져와 자동 업데이트됩니다.

**코드에서 설정하는 경우:**

```csharp
// ITextData를 전달하여 텍스트 설정
lhText.SetTextData(new TextData("HomeTitle"));

// 파라미터 포함
lhText.SetTextData(new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 }));
```

`TextTableService`가 등록되지 않은 경우 키 문자열을 그대로 표시합니다. `FontService`가 등록되지 않은 경우 폰트 자동 업데이트는 건너뜁니다(수동으로 설정한 폰트가 유지됨).

---

## InputLayer

**패키지:** `com.lisearcheleeds.lighthouse-extends.inputlayer`  
**의존 패키지:** Input System

`InputActionAsset` 위에서 스택 기반의 입력 디스패치를 제공합니다. 이벤트는 가장 위의 레이어부터 순서대로 전달되며 소비 또는 차단되면 아래 레이어에는 전달되지 않습니다.

### 사용 방법

```csharp
// 레이어 푸시 (ActionMap이 자동으로 활성화됨)
inputLayerController.PushLayer(myLayer, myActionMap);

// 가장 위의 레이어 팝
inputLayerController.PopLayer();

// 특정 레이어를 지정하여 팝
inputLayerController.PopLayer(myLayer);
```

### IInputLayer 구현

```csharp
public class HomeInputLayer : IInputLayer
{
    // true로 하면 하위 레이어로의 이벤트 전파를 모두 차단
    public bool BlocksAllInput => false;

    public bool OnActionStarted(InputAction.CallbackContext ctx)
    {
        // true를 반환하면 해당 이벤트를 소비 (하위 레이어에 전달되지 않음)
        return false;
    }

    public bool OnActionPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.action.name == "Back")
        {
            // 뒤로 가기 버튼 처리
            return true; // 소비
        }
        return false;
    }

    public bool OnActionCanceled(InputAction.CallbackContext ctx) => false;
}
```

| 동작 | 설명 |
|---|---|
| `return true` | 이벤트 소비. 하위 레이어에 전달되지 않음 |
| `return false` | 이벤트 통과. 하위 레이어에 전달됨 |
| `BlocksAllInput = true` | 반환값에 관계없이 하위 레이어로의 전파를 모두 차단 |

같은 `ActionMap`을 참조하는 레이어가 남아 있으면 팝 시 `ActionMap`은 비활성화되지 않습니다.

---

## UIComponent

**패키지:** `com.lisearcheleeds.lighthouse-extends.uicomponent`  
**의존 패키지:** 없음 (코어 패키지 없이 단독 사용 가능)

UI에 관한 범용 컴포넌트 군입니다.

### LHButton

`UnityEngine.UI.Button`을 상속한 버튼 컴포넌트입니다. `ExclusiveInputService`와 연동하여 멀티터치 환경에서의 동시 탭을 방지합니다.

- 여러 `LHButton`이 동시에 탭된 경우 첫 번째 탭만 유효
- 앱이 백그라운드로 전환되면 누름 상태를 자동 리셋
- `ExclusiveInputService`가 미등록인 경우 일반 `Button`과 같은 동작

### ExclusiveInputService

포인터 ID를 배타적으로 관리하는 서비스입니다. `LHButton`이 자동으로 사용하므로 직접 조작은 거의 필요 없습니다.

### LHRaycastTargetObject

렌더링 없이 레이캐스트만 받는 투명한 `Graphic` 컴포넌트입니다. 이미지 없이 버튼 등의 히트 영역을 정의할 때 사용합니다.

`CanvasRenderer` 컴포넌트가 필요합니다(자동으로 어태치됨).

### LHCanvasSceneObject

Canvas와 씬 카메라의 연결을 관리하는 컴포넌트입니다. `CanvasMainSceneBase`와 조합하여 사용합니다.

### DefaultCanvasSceneEditorOnlyObject

에디터에서만 표시되는 오브젝트를 관리하는 컴포넌트입니다. `IEditorOnlyObjectCanvasScene`을 구현한 컴포넌트와 조합하여 실제 빌드에서 제외하고 싶은 디버그 UI 등에 사용합니다.
