# Step 5: Launcher 씬 및 Launcher 구현

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

### Launcher 씬의 역할

Launcher 씬은 애플리케이션의 **엔트리 포인트가 되는 씬**입니다.  
Unity 시작 시 가장 먼저 로드되며, VContainer가 `RootLifetimeScope`를 생성하는 기점이 됩니다.  
씬 자체에는 게임 로직을 두지 않고 최대한 비어 있는 상태로 유지합니다.

```
Build Settings:
  0: Launcher      ← 시작 시 가장 먼저 로드되는 씬
  1: (각 게임 씬)
```

### ILauncher / Launcher의 역할

`ILauncher`는 앱의 초기 실행과 재시작(리부트)을 담당하는 인터페이스입니다.

```csharp
public interface ILauncher
{
    UniTask Launch();   // 초기 실행
    void Reboot();      // 앱 재시작 (Launcher 씬으로 돌아가 재실행)
}
```

`Launcher`를 `ProductLifetimeScope`에 등록함으로써 `ProductEntryPoint`에서 호출할 수 있게 됩니다.

```csharp
builder.Register<Launcher>(Lifetime.Singleton).AsImplementedInterfaces();
```

### 구현 예시

```csharp
public sealed class Launcher : ILauncher
{
    static readonly string LauncherSceneName = "Launcher";

    readonly ISceneManager sceneManager;

    [Inject]
    public Launcher(ISceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
    }

    async UniTask ILauncher.Launch()
    {
        await FirstLaunchProcess();  // 최초 실행 시에만 필요한 처리 (캐시 구축 등)
        await LaunchProcess();       // 매 실행마다 필요한 처리 (마스터 데이터 로드 등)
        TransitionNextScene();       // 첫 번째 게임 씬으로 전환
    }

    void ILauncher.Reboot()
    {
        RebootProcess().Forget();

        async UniTask RebootProcess()
        {
            await sceneManager.PreReboot();
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                LauncherSceneName, LoadSceneMode.Single);
            await LaunchProcess();
            TransitionNextScene();
        }
    }

    UniTask FirstLaunchProcess() => UniTask.CompletedTask;  // 필요에 따라 구현
    UniTask LaunchProcess() => UniTask.CompletedTask;       // 필요에 따라 구현

    void TransitionNextScene()
    {
        UniTask.Void(async () =>
        {
            await sceneManager.TransitionScene(new SplashTransitionData());

            // 전환 완료 후 Launcher 씬 언로드
            if (!string.IsNullOrEmpty(
                    UnityEngine.SceneManagement.SceneManager.GetSceneByName(LauncherSceneName).name))
            {
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(LauncherSceneName);
            }
        });
    }
}
```

### 전체 시작 플로우

```
Unity 시작
  └── Launcher 씬 로드
        └── VContainerSettings가 RootLifetimeScope Prefab을 자동 생성
              └── RootEntryPoint.Start()
                    └── ProductLifetimeScope Prefab을 자식 스코프로 생성
                          └── ProductEntryPoint.StartAsync()
                                ├── 씬 매니저에 부모 스코프 등록
                                ├── 언어 초기화
                                └── Launcher.Launch()
                                      ├── 최초 실행 처리
                                      ├── 실행 처리
                                      └── 첫 번째 씬으로 전환 → Launcher 씬 언로드
```
