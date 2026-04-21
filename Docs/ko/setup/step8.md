# Step 8: 씬 LifetimeScope 및 기반 클래스 구현

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

### 씬별 LifetimeScope

각 씬(메인·모듈)에는 해당 씬 전용의 `LifetimeScope`를 준비합니다.  
씬의 `LifetimeScope`는 `ProductLifetimeScope`의 자식 스코프로 자동 구성됩니다.

```csharp
public class HomeLifetimeScope : LifetimeScope
{
    [SerializeField] HomeScene homeScene;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(homeScene);
        builder.Register<HomePresenter>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

씬 GameObject의 **LifetimeScope** 컴포넌트에 이 스크립트를 부착합니다.

### 씬 스크립트 구현

`MainSceneBase<TTransitionData>` 또는 `CanvasMainSceneBase<TTransitionData>`를 상속하여 구현합니다.

```csharp
public class HomeScene : CanvasMainSceneBase<HomeScene.HomeTransitionData>
{
    IHomePresenter homePresenter;

    public override MainSceneId MainSceneId => YourProductMainSceneId.Home;

    public class HomeTransitionData : TransitionDataBase
    {
        public override MainSceneId MainSceneId => YourProductMainSceneId.Home;
    }

    [Inject]
    public void Construct(IHomePresenter homePresenter)
    {
        this.homePresenter = homePresenter;
    }
}
```

Canvas를 사용하지 않는 씬은 `MainSceneBase<TTransitionData>`를, Canvas를 사용하는 씬은 `CanvasMainSceneBase<TTransitionData>`를 상속하세요.

### 모듈 씬 구현

모듈 씬도 메인 씬과 동일한 구조입니다. `ModuleSceneBase` 또는 `CanvasModuleSceneBase`를 상속하고 전용 `LifetimeScope`를 준비합니다.

```csharp
public class BackgroundScene : CanvasModuleSceneBase
{
    IBackgroundPresenter backgroundPresenter;

    [Inject]
    public void Construct(IBackgroundPresenter backgroundPresenter)
    {
        this.backgroundPresenter = backgroundPresenter;
    }
}
```

```csharp
public class BackgroundLifetimeScope : LifetimeScope
{
    [SerializeField] BackgroundScene backgroundScene;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(backgroundScene);
        builder.Register<BackgroundPresenter>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

> 모듈 씬은 `TransitionData`를 갖지 않습니다. 메인 씬의 전환에 따라 로드·언로드됩니다.

### SaveSceneState와 전환 리다이렉트

`MainSceneBase`에는 `SaveSceneState`를 오버라이드할 수 있습니다. 이 메서드는 씬을 떠나기 전(`LoadNextSceneStatePhase`)에 호출되며, 서버 통신이나 도메인 데이터 저장에 사용합니다.

```csharp
public override async UniTask SaveSceneState(CancellationToken cancelToken)
{
    await SaveProgress(cancelToken);
}
```

`LoadSceneState`(`TransitionDataBase` 측)에서 `LHSceneInterceptException`을 던지면 전환을 다른 씬으로 리다이렉트할 수 있습니다. `SaveSceneState`가 완료된 후 `LoadSceneState`가 실행되므로 저장 정합성이 보장됩니다.

```csharp
// TransitionData 내에서 만료를 감지해 다른 화면으로 리다이렉트하는 예시
public override async UniTask LoadSceneState(TransitionDirectionType dir, CancellationToken ct)
{
    var isExpired = await CheckEventExpired(ct);
    if (isExpired)
    {
        throw new LHSceneInterceptException(new HomeTransitionData());
    }
}
```

> `LHSceneInterceptException`은 `LoadNextSceneStatePhase`(`CanTransitionIntercept = true`)에서만 유효합니다.  
> 다른 페이즈에서 던진 경우 `InvalidOperationException`으로 구현자에게 알립니다.

### 프로덕트 공통 기반 클래스 (권장)

애니메이션이나 입력 레이어 등 씬 전체에서 공통된 처리를 모은 중간 기반 클래스를 준비하는 것을 권장합니다.

```csharp
// 프로덕트 전체에서 공통된 처리를 모은 중간 기반 클래스
public abstract class YourProductCanvasMainSceneBase<TTransitionData> : CanvasMainSceneBase<TTransitionData>
    where TTransitionData : TransitionDataBase
{
    // 애니메이션·입력 레이어 등 공통 처리를 여기에 추가
}
```

각 씬에서 이 클래스를 상속함으로써 보일러플레이트를 줄일 수 있습니다.

### SceneGroupProvider 구현

SceneId 생성 후, `ISceneGroupProvider`를 구현하여 SceneId와 `SceneGroup`의 매핑을 정의합니다.  
이것을 `ProductLifetimeScope`에 등록하면 씬 전환 시 올바른 씬 그룹이 로드됩니다.

```csharp
public sealed class SceneGroupProvider : ISceneGroupProvider
{
    // 모든 씬에 반드시 동반하는 모듈 씬
    static readonly ModuleSceneId[] RequireModuleSceneIds =
    {
        YourProductModuleSceneId.Overlay,
    };

    // 메인 씬과 추가 모듈 씬의 매핑
    static readonly IReadOnlyDictionary<MainSceneId, ModuleSceneId[]> SceneModuleMap =
        new Dictionary<MainSceneId, ModuleSceneId[]>
        {
            { YourProductMainSceneId.Splash, null },
            { YourProductMainSceneId.Home,   new[] { YourProductModuleSceneId.Background } },
        };

    // 동시에 캐시할 메인 씬 그룹 정의
    static readonly MainSceneId[][] MainSceneGroupList =
    {
        new[] { YourProductMainSceneId.Splash },
        new[] { YourProductMainSceneId.Home },
    };

    SceneGroup ISceneGroupProvider.GetSceneGroup(MainSceneId mainSceneId)
    {
        return SceneGroupList.First(g => g.MainSceneIds.Contains(mainSceneId));
    }

    // ... (SceneGroup 구축 로직)
}
```

> `MainSceneGroupList` 정의에 따라 씬의 캐시 전략이 달라집니다.  
> 같은 그룹 내의 씬은 동시에 로드되어 캐시됩니다.
