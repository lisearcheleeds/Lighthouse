# Lighthouse — 세팅 가이드

패키지를 Unity 프로젝트에 추가한 후, 아래 단계에 따라 초기 설정을 완료하세요.

## 스타터 킷 (UnityPackage)

설정 작업의 대부분을 건너뛸 수 있는 UnityPackage를 [Releases](https://github.com/lisearcheleeds/Lighthouse/releases)에서 다운로드할 수 있습니다.

### 건너뛸 수 있는 단계

| 단계 | 건너뛰기 | 비고 |
|---|---|---|
| Step 1: 설정 에셋 생성 | 대부분 생략 가능 | `ProductNameSpace`·`MainSceneIdPrefix`·`ModuleSceneIdPrefix`·출력 디렉토리는 프로젝트에 맞게 변경 필요 |
| Step 2: ScreenStack 설정 | 대부분 생략 가능 | namespace와 출력 디렉토리 변경만 필요 |
| Step 3: TextTable 설정 | 완전 생략 가능 | 기본 경로를 유지하는 경우 변경 불필요 |
| Step 4: VContainer LifetimeScope 구현 | 완전 생략 가능 | 스크립트·Prefab·ScriptableObject·VContainerSettings 모두 제공 |
| Step 5: Launcher 씬 및 Launcher 구현 | 완전 생략 가능 | Launcher.unity·Launcher.cs 제공 |
| Step 6: Build Settings 등록 | 필요 | Build Settings는 항상 수동 설정 필요 |
| Step 7: 씬 스크립트 생성 | 필요 | 게임별 작업 |
| Step 8: LifetimeScope 및 기반 클래스 구현 | 필요 | 공통 기반 클래스 제공됨. 씬별 LifetimeScope와 SceneGroupProvider 커스터마이징 필요 |

---

## 목차

- [Step 1: 설정 에셋 생성](./setup/step1.md)
- [Step 2: ScreenStack 설정](./setup/step2.md)
- [Step 3: TextTable 설정](./setup/step3.md)
- [Step 4: VContainer LifetimeScope 구현](./setup/step4.md)
- [Step 5: Launcher 씬 및 Launcher 구현](./setup/step5.md)
- [Step 6: 씬 파일 생성 및 Build Settings 등록](./setup/step6.md)
- [Step 7: 씬 스크립트 생성](./setup/step7.md)
- [Step 8: 씬 LifetimeScope 및 기반 클래스 구현](./setup/step8.md)
