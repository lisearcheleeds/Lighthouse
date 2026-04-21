# Step 6: 씬 파일 생성 및 Build Settings 등록

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

### 씬 파일 명명 규칙

Lighthouse의 `SceneIdGenerator`는 Build Settings에 등록된 씬 경로에 포함된 키워드로 씬 종류를 판별합니다.

| 종류 | 경로에 포함되어야 할 키워드 | 예시 |
|---|---|---|
| 메인 씬 | `MainScene` | `Assets/.../Scene/MainScene/Home/Home.unity` |
| 모듈 씬 | `ModuleScene` | `Assets/.../Scene/ModuleScene/Background/Background.unity` |

> 씬 파일 이름 또는 부모 폴더 이름에 `MainScene` / `ModuleScene`이 포함되도록 배치하세요.

### Build Settings 등록

1. Unity 메뉴에서 **File > Build Settings**를 엽니다
2. 씬 파일을 **Scenes In Build** 목록에 드래그 앤 드롭합니다
3. `Launcher` 씬을 **index 0**에 배치합니다

```
Scenes In Build:
  0: Assets/.../Scene/Launcher.unity          ← 반드시 첫 번째
  1: Assets/.../Scene/MainScene/Splash/Splash.unity
  2: Assets/.../Scene/MainScene/Home/Home.unity
  3: Assets/.../Scene/ModuleScene/Background/Background.unity
  ...
```

> Build Settings가 변경되면 `SceneIdGenerator`가 자동으로 `MainSceneId` / `ModuleSceneId` 클래스를 재생성합니다.

### 수동 생성

자동 생성이 동작하지 않는 경우, 다음 메뉴에서 수동으로 실행할 수 있습니다:

- **Lighthouse > Generate > Auto > Generate "MainSceneId" manually**
- **Lighthouse > Generate > Auto > Generate "ModuleSceneId" manually**
