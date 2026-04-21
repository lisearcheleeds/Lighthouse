# Step 2: ScreenStack 설정

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

> ScreenStack을 사용하지 않는 경우 이 단계를 건너뛰세요.

Unity 메뉴에서 **Lighthouse > Settings > ScreenStackGenerateSettings** 를 선택합니다.

`Assets/Settings/Lighthouse/ScreenStackGenerateSettings.asset` 이 생성됩니다.

Inspector에서 다음 항목을 설정합니다:

| 필드 | 설명 |
|---|---|
| Screen Stack Entity Factory Directory | `ScreenStackEntityFactory` 출력 폴더 |
| Screen Stack Entity Factory Class Name | 생성 클래스 이름 (기본값: `ScreenStackEntityFactory`) |
| Screen Stack Entity Factory Namespace | 생성 팩토리 클래스의 네임스페이스 |
| Screen Stack Entity Factory Template | 팩토리 생성용 템플릿 에셋 |
| Screen Stack Dialog Script Output Directory | 다이얼로그 스크립트 출력 폴더 |
| Screen Stack Dialog Script Namespace | 생성 다이얼로그 스크립트의 네임스페이스 |
| Screen Stack Script Templates | 다이얼로그 스크립트 생성용 템플릿 에셋 |
