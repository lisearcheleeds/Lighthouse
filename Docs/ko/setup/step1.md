# Step 1: 설정 에셋 생성

## 세팅 가이드 — 목차

- [Step 1: 설정 에셋 생성](./step1.md)
- [Step 2: ScreenStack 설정](./step2.md)
- [Step 3: TextTable 설정](./step3.md)
- [Step 4: VContainer LifetimeScope 구현](./step4.md)
- [Step 5: Launcher 씬 및 Launcher 구현](./step5.md)
- [Step 6: 씬 파일 생성 및 Build Settings 등록](./step6.md)
- [Step 7: 씬 스크립트 생성](./step7.md)
- [Step 8: 씬 LifetimeScope 및 기반 클래스 구현](./step8.md)

---

### 1-1. GenerateSettings 생성

Unity 메뉴에서 **Lighthouse > Settings > GenerateSettings** 를 선택합니다.

`Assets/Settings/Lighthouse/GenerateSettings.asset` 이 생성됩니다.

Inspector에서 다음 항목을 설정합니다:

| 필드 | 설명 |
|---|---|
| Product Name Space | 생성되는 클래스의 루트 네임스페이스 (예: `SampleProduct`) |
| Generated File Directory | 생성 파일 출력 폴더 (아래에서 설명할 `LighthouseGenerated` 폴더 지정) |
| Main Scene Id Prefix | MainSceneId 클래스 이름의 접두사 (예: `Sample`) |
| Module Scene Id Prefix | ModuleSceneId 클래스 이름의 접두사 (예: `Sample`) |
| Scene Script Output Directory | 씬 스크립트 출력 폴더 |
| Scene Script Templates | 씬 스크립트 생성에 사용할 템플릿 에셋 |

### 1-2. LighthouseGenerated 폴더 생성

프로젝트의 Assets 하위에 `LighthouseGenerated` 폴더를 생성합니다.

```
Assets/
└── {YourProductFolder}/
    └── LighthouseGenerated/   ← 여기에 생성
```

생성 후, 이 폴더를 `GenerateSettings`의 **Generated File Directory** 필드에 드래그 앤 드롭하여 지정합니다.

### 1-3. SceneEditSettings 생성

Unity 메뉴에서 **Lighthouse > Settings > SceneEditSettings** 를 선택합니다.

`Assets/Settings/Lighthouse/SceneEditSettings.asset` 이 생성됩니다.

> **참고:** `SceneEditSettings`는 에디터 시작 시 자동 생성될 수 있습니다.  
> 이미 파일이 존재하는 경우 덮어쓰지 않습니다.

Inspector에서 다음 항목을 확인·설정합니다:

| 필드 | 설명 | 기본값 |
|---|---|---|
| Enable Scene Edit Process | 씬 편집 중 자동 업데이트 활성화/비활성화 | `true` |
| Canvas Scene Editor Only Object | Canvas 씬 편집용 에디터 전용 오브젝트 Prefab (아래 참고) | 없음 |
| Editor Only Object Name | 씬에 배치되는 오브젝트 이름 | `__EditorOnly__` |

### 1-4. CanvasSceneEditorOnlyObject Prefab 생성 및 설정

에디터에서 씬을 편집할 때 Canvas 동작을 보조하는 Prefab을 생성합니다.

**Prefab 생성 절차:**

1. 임의의 폴더에 빈 GameObject를 생성하고 Prefab으로 만듭니다
2. Prefab에 다음 두 컴포넌트를 추가합니다
   - `LHCanvasSceneObject` — SceneCamera와 EventSystem을 갖는 컴포넌트
   - `DefaultCanvasSceneEditorOnlyObject` — `IEditorOnlyObjectCanvasScene`의 표준 구현
3. `DefaultCanvasSceneEditorOnlyObject`의 **Lh Canvas Scene Object** 필드에 같은 GameObject의 `LHCanvasSceneObject`를 설정합니다
4. `LHCanvasSceneObject`의 **UI Camera** 및 **UI Event System** 필드에 적절한 컴포넌트를 설정합니다

**SceneEditSettings에 등록:**

생성한 Prefab을 `SceneEditSettings`의 **Canvas Scene Editor Only Object** 필드에 설정합니다.

> 이 Prefab은 `ICanvasSceneBase`를 구현한 씬이 에디터에서 열릴 때 자동으로 씬에 배치되며, 재생 또는 저장 시 자동으로 제거됩니다.
