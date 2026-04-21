# Step 7: 씬 스크립트 생성

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

Lighthouse에는 씬 보일러플레이트를 생성하는 에디터 창이 포함되어 있습니다.

**Lighthouse > Generate > Open Scene scripts generator**를 열고, 다음 항목을 설정한 후 **Generate**를 누릅니다.

| 항목 | 설명 |
|---|---|
| Scene Type | `MainScene` 또는 `ModuleScene` |
| Template Preset | `GenerateSettings`에 등록한 템플릿 |
| Base Class | 상속할 기반 클래스 (예: `CanvasMainSceneBase`) |
| Scene Name | 씬 이름 (예: `Home`) |

`GenerateSettings`의 **Scene Script Output Directory**에 지정한 폴더에 씬 스크립트가 생성됩니다.

> 템플릿을 커스터마이즈하면 프로덕트 공통 기반 클래스를 상속하는 스크립트를 생성할 수 있습니다.  
> 자세한 내용은 `GenerateSettings`의 **Scene Script Templates**를 참조하세요.
