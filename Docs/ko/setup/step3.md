# Step 3: TextTable 설정

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

> TextTable을 사용하지 않는 경우 이 단계를 건너뛰세요.

`TextTableEditorSettings`는 TextTable 에디터 창을 열면 자동으로 생성됩니다.

Unity 메뉴에서 **Lighthouse > TextTable > ...** 을 선택하여 창을 열면  
`Assets/Settings/Lighthouse/TextTableEditorSettings.asset` 이 자동 생성됩니다.

Inspector에서 다음 항목을 확인·변경합니다:

| 필드 | 설명 | 기본값 |
|---|---|---|
| Text Table Folder Path | TSV 파일 저장 경로 | `Assets/StreamingAssets/TextTables` |
