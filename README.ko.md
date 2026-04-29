[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
![Version](https://img.shields.io/badge/version-1.0.0-brightgreen)

**[English](README.md) | [日本語](README.ja.md) | [中文](README.zh.md)**

# Lighthouse

[VContainer](https://github.com/hadashiA/VContainer)과 [UniTask](https://github.com/Cysharp/UniTask)를 기반으로 구조화된 씬 관리 시스템, 다이얼로그 스택, 확장 가능한 런타임 모듈을 제공하는 Unity 애플리케이션 프레임워크입니다.

현재 버전은 **1.0.0**입니다.

<img width="1677" height="938" alt="lighthouse" src="https://github.com/user-attachments/assets/f0e9c5de-f858-4e0d-be63-7e57a4d6558c" />

---

## 요구 사항

| 의존 라이브러리 | 버전 |
|---|---|
| Unity | 6000.0 이상 (URP) |
| VContainer | 1.17.0 이상 |
| UniTask | 2.5.10 이상 |
| R3 | 1.3.0 이상 |
| TextMeshPro | 3.x |
| Input System | 1.17.0 이상 |

---

## 저장소

| 저장소 | 설명 |
|---|---|
| [Lighthouse](https://github.com/lisearcheleeds/Lighthouse) | Unity 아키텍처 프레임워크 |
| [LighthouseSample](https://github.com/lisearcheleeds/LighthouseSample) | 프레임워크 사용법을 보여주는 샘플 프로젝트 |
| [LighthouseQuickPackage](https://github.com/lisearcheleeds/LighthouseQuickPackage) | 사전 구성된 빈 프로젝트 |

---

## WebGL 데모
https://lisearcheleeds.github.io/LighthouseSample/

---

## 설치

### 방법 1: Unity Package Manager — Git URL

`Packages/manifest.json`을 열어 필요한 패키지를 추가합니다:

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

> **주의:** Git URL 방식에서는 의존 라이브러리(UniTask, VContainer, R3 등)가 자동으로 설치되지 않습니다. 수동으로 추가하거나 [OpenUPM](https://openupm.com)을 사용하세요.

### 방법 2: OpenUPM _(출시 예정)_

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

### 방법 3: UnityPackage

[Releases](https://github.com/lisearcheleeds/Lighthouse/releases)에서 최신 `.unitypackage`를 다운로드하여 `Assets > Import Package > Custom Package`로 임포트하세요.

---

## 기능

### 씬 시스템 (`Lighthouse`)

- **씬 스택 관리** — 씬은 전환할 때마다 스택에 쌓이며, 이전 화면으로 돌아가는 기능을 제공합니다.
- **메인 + 모듈 씬 모델** — 게임 화면은 하나 이상의 메인 씬과 여러 모듈 씬을 조합하여 표현됩니다. 헤더나 배경 같은 공통 UI는 그대로 유지하면서 화면 중앙의 콘텐츠만 교체하는 표현이 가능합니다.
- **씬 캐싱** — 씬은 `SceneGroup`이라는 단위로 로드/언로드됩니다. 씬으로 전환할 때 같은 `SceneGroup`에 속한 다른 메인 씬과 모듈 씬도 동시에 로드되어 캐시됩니다. `SceneGroup`은 프로덕트 측에서 정의하므로, 단일 SceneGroup을 구성하여 사용자 경험과 성능 사이의 균형을 조정할 수 있습니다.
- **페이즈 / 스텝 파이프라인** — 씬 전환은 순서가 있는 페이즈(예: `OutAnimationPhase`, `LoadSceneGroupPhase`, `EnterScenePhase`)로 구성되며, 각 페이즈는 병렬 스텝을 포함합니다. 시퀀스는 프로덕트 측에서 `ISceneTransitionSequenceProvider`를 정의하여 교체할 수 있습니다.
- **전환 타입** — `Exclusive`(아웃/인 애니메이션 전환)와 `Cross`(크로스페이드)가 내장되어 있습니다. `Auto`는 씬 그룹에 따라 적절한 타입을 자동 선택합니다.
- **씬 기반 클래스** — `MainSceneBase<TTransitionData>`와 `ModuleSceneBase`가 활성화, 애니메이션 훅, Canvas 주입을 처리합니다. Canvas 변형(`CanvasMainSceneBase`, `CanvasModuleSceneBase`)은 CanvasGroup 알파 제어를 추가합니다.
- **카메라 스택 관리** — `SceneCameraManager`가 각 전환 시 URP 카메라 스택을 재구성하여 Base/Overlay 역할과 깊이 순서를 자동으로 지정합니다.
- **뒤로 가기 내비게이션** — `SceneManager`가 전환 이력 스택을 유지하고 `CanBackTransition = false` 항목을 건너뛰며 올바른 뒤로 가기 대상을 결정합니다.

### 스크린 스택 (`LighthouseExtends.ScreenStack`)

- **다이얼로그 / 오버레이 관리** — `ScreenStackBase`를 구현한 스크린이 스택에 푸시됩니다. Open / Close / ClearAll 작업은 큐에 들어가 순서대로 실행됩니다.
- **System / Default 레이어** — 각 스크린 또는 오버레이는 `IScreenStackData.IsSystem`에 따라 시스템 Canvas 레이어 또는 기본 레이어에 배치됩니다.
- **입력 차단** — `ScreenStackBackgroundInputBlocker`가 최상단 스크린 뒤에 전체 화면 레이캐스트 타겟을 배치하여 터치 관통을 방지합니다.
- **씬 일시 정지 & 재개** — `ScreenStackModuleSceneBase`가 씬 전환 이벤트에 훅하여 전진 이탈 시 스택을 일시 정지하고 뒤로 가기 진입 시 재개하여 씬 전환 간 상태를 보존합니다.
- **프록시 패턴** — `ScreenStackModuleProxy`(씬 스코프)가 `ScreenStackModule`(모듈 스코프)에 연결하여 모든 DI 스코프에서 스택 API에 접근할 수 있습니다.
- **코드 생성** — `ScreenStackDialogScriptGeneratorWindow`와 `ScreenStackEntityFactoryGenerator`가 템플릿으로부터 다이얼로그 스크립트 보일러플레이트와 `ScreenStackEntityFactory` 스위치 테이블을 생성합니다.

### 확장 모듈 (`LighthouseExtends`)

| 모듈 | 설명 |
|---|---|
| **Addressable** | `AssetManager` — 참조 카운트 방식의 Addressables 래퍼. 스코프 단위의 에셋 수명 관리와 병렬 로드를 지원합니다. |
| **Animation** | `LHTransitionAnimator` / `LHSceneTransitionAnimator` — `PlayableGraph`를 통한 In/Out 클립 재생. 방향별 시작 딜레이와 상호 배타 지원. |
| **Language** | `LanguageService` — 반응형 언어 전환. `CurrentLanguage` 업데이트 전에 등록된 핸들러가 병렬로 호출됩니다. |
| **Font** | `FontService` — `ILanguageService`를 구독하여 언어 변경 시마다 `LanguageFontSettings`에서 `CurrentFont`(TMP_FontAsset)를 업데이트합니다. |
| **TextTable** | `TextTableService` — `ITextTableLoader`를 통해 언어별 TSV 파일을 로드하고 런타임에 `{param}` 플레이스홀더를 해석합니다. 씬과 Prefab에 걸쳐 키와 번역을 편집할 수 있는 에디터 창을 포함합니다. |
| **TextMeshPro** | `LHTextMeshPro` — `TextMeshProUGUI`를 확장하여 `TextTableService`와 `FontService`를 통해 언어 및 폰트 변경을 자동 반영합니다. |
| **InputLayer** | `InputLayerController` — `InputActionAsset` 위의 스택 기반 입력 디스패치. 레이어는 이벤트를 소비하거나 `BlocksAllInput`으로 하위 레이어를 모두 차단할 수 있습니다. |

---

## 아키텍처

```
LighthouseArchitecture/
├── Lighthouse/                     # 코어 프레임워크
│   └── Scene/
│       ├── SceneBase/              # 씬용 MonoBehaviour 기반 클래스
│       ├── SceneCamera/            # URP 카메라 스택 관리
│       ├── SceneTransitionPhase/   # 전환 페이즈 정의
│       ├── SceneTransitionStep/    # 개별 비동기 스텝
│       └── *.cs                    # SceneManager, 컨텍스트, 데이터 모델
│
└── LighthouseExtends/              # 선택적 런타임 & 에디터 모듈
    ├── Addressable/
    ├── Animation/
    ├── Font/
    ├── InputLayer/
    ├── Language/
    ├── ScreenStack/
    ├── TextMeshPro/
    └── TextTable/
```

**의존 방향**

```
게임 코드
    └── Lighthouse (ISceneManager, IScreenStackModule, ILanguageService, ...)
            └── LighthouseExtends (Animation, Font, TextTable, ScreenStack, ...)
```

---

## 구현 일부 소개

### 씬 정의

```csharp
// 1. 전환 데이터 클래스 정의
public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;
}

// 2. 씬 구현
public class HomeScene : MainSceneBase<HomeTransitionData>
{
    protected override UniTask OnEnter(HomeTransitionData data,
        ISceneTransitionContext context, CancellationToken ct)
    {
        // UI 초기화
        return UniTask.CompletedTask;
    }
}

// 3. 전환
await sceneManager.TransitionScene(new HomeTransitionData());
```

### 다이얼로그 열기

```csharp
// 스크린을 열고 데이터 전달
await screenStackModule.Open(new SampleDialogData());

// 최상단 스크린 닫기
await screenStackModule.Close();
```

### 언어 전환

```csharp
// TextTableService와 FontService가 자동으로 업데이트됩니다.
// LHTextMeshPro는 이벤트를 수신하여 미리 설정된 적절한 폰트와 언어로 자동 전환됩니다.
await languageService.SetLanguage("ko", cancellationToken);
```

### 텍스트 컴포넌트 현지화

Inspector에서 임의의 `LHTextMeshPro` 컴포넌트에 **Text Key**를 지정합니다.  
런타임에 언어가 변경되면 텍스트와 폰트가 자동으로 업데이트됩니다.

코드에서 키를 등록하는 경우:

```csharp
// 에디터 전용: TSV 항목을 생성하고 new TextData("key")로 재작성합니다
var data = TextData.CreateTextData("Home", "HomeTitle", "환영합니다");
lhText.SetTextData(data);
```

### 기능 확장

이 모든 기능은 DI로 관리되므로, 프로젝트에서 사용할 기능을 선택하고 전부 또는 일부를 자유롭게 확장할 수 있습니다.

---

## 가이드

| 언어 | 링크 |
|---|---|
| English | [Docs/en](Docs/en/readme.md) |
| 日本語 | [Docs/ja](Docs/ja/readme.md) |
| 한국어 | [Docs/ko](Docs/ko/readme.md) |
| 中文 | [Docs/zh](Docs/zh/readme.md) |

---

## 작성자

**Lise** — [GitHub](https://github.com/lisearcheleeds) / [X](https://x.com/archeleeds)

---

## 라이선스

이 프로젝트는 [MIT 라이선스](LICENSE) 하에 공개되어 있습니다.
