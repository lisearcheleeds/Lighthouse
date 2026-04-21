# Step 4: VContainer LifetimeScope 구현

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

Lighthouse 씬 시스템은 VContainer DI 스코프와 연동하여 동작합니다.  
아래의 스크립트, Prefab, ScriptableObject를 준비하면 애플리케이션 시작 플로우가 구성됩니다.

> VContainer 기본 사용법: **[TODO: VContainer 문서 URL]**  
> Unity 프로젝트 초기 설정 도우미: **[TODO: Preset startup unity package URL]**

### 권장 파일 구성

다음과 같은 배치를 권장합니다.

```
Assets/
└── {YourProduct}/
    └── Runtime/
        ├── Scripts/
        │   └── Core/
        │       ├── RootLifetimeScope.cs
        │       ├── RootLifetimeScopeSettings.cs
        │       ├── RootEntryPoint.cs
        │       ├── ProductLifetimeScope.cs
        │       ├── ProductLifetimeScopeSettings.cs
        │       ├── ProductEntryPoint.cs
        │       └── SceneGroupProvider.cs
        └── StaticResources/
            └── LifetimeScope/
                ├── RootLifetimeScope.prefab
                ├── RootLifetimeScopeData.asset
                ├── ProductLifetimeScope.prefab
                ├── ProductLifetimeScopeSettings.asset
                ├── VContainerSettings.asset
                ├── SupportedLanguageSettings.asset    (Language 사용 시)
                └── LanguageFontSettings.asset         (Font 사용 시)
```

### 스크립트 역할

#### Scripts/Core/

| 파일 | 역할 |
|---|---|
| `RootLifetimeScope.cs` | 앱 전체의 루트 스코프. `RootEntryPoint`와 `RootLifetimeScopeSettings`를 등록한다 |
| `RootLifetimeScopeSettings.cs` | `ProductLifetimeScope` Prefab 참조를 가지는 ScriptableObject |
| `RootEntryPoint.cs` | `IStartable`. 시작 시 `ProductLifetimeScope` Prefab을 자식 스코프로 인스턴스화한다 |
| `ProductLifetimeScope.cs` | Lighthouse 서비스 및 프로덕트 고유 서비스를 모두 등록하는 스코프 |
| `ProductLifetimeScopeSettings.cs` | 프로덕트 고유 설정을 보관하는 ScriptableObject (필요에 따라 필드 추가) |
| `ProductEntryPoint.cs` | `IAsyncStartable`. 씬 매니저에 부모 스코프 설정, 언어 초기화, 앱 시작을 수행한다 |
| `SceneGroupProvider.cs` | `ISceneGroupProvider` 구현. `MainSceneId`와 `SceneGroup`의 매핑을 정의한다 |

#### StaticResources/LifetimeScope/

| 파일 | 역할 |
|---|---|
| `RootLifetimeScope.prefab` | `RootLifetimeScope` 컴포넌트를 부착한 Prefab |
| `RootLifetimeScopeData.asset` | `RootLifetimeScopeSettings`. `ProductLifetimeScope.prefab`을 참조한다 |
| `ProductLifetimeScope.prefab` | `ProductLifetimeScope` 컴포넌트를 부착한 Prefab |
| `ProductLifetimeScopeSettings.asset` | `ProductLifetimeScopeSettings`의 ScriptableObject |
| `VContainerSettings.asset` | VContainer 시작 설정. Root Prefab으로 `RootLifetimeScope.prefab`을 지정한다 |
| `SupportedLanguageSettings.asset` | 지원 언어 코드 목록 (Language 사용 시) |
| `LanguageFontSettings.asset` | 언어별 폰트 매핑 (Font 사용 시) |

### 구현 절차

#### 1. 스크립트 작성

`RootLifetimeScope` / `RootEntryPoint`는 최소한의 구성입니다.

```csharp
public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] RootLifetimeScopeSettings rootLifetimeScopeSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<RootEntryPoint>();
        builder.RegisterInstance(rootLifetimeScopeSettings);
    }
}
```

```csharp
public class RootEntryPoint : IStartable
{
    readonly RootLifetimeScope rootLifetimeScope;
    readonly RootLifetimeScopeSettings rootLifetimeScopeSettings;

    [Inject]
    public RootEntryPoint(RootLifetimeScope rootLifetimeScope, RootLifetimeScopeSettings rootLifetimeScopeSettings)
    {
        this.rootLifetimeScope = rootLifetimeScope;
        this.rootLifetimeScopeSettings = rootLifetimeScopeSettings;
    }

    public void Start()
    {
        using (LifetimeScope.EnqueueParent(rootLifetimeScope))
        {
            var instance = Object.Instantiate(rootLifetimeScopeSettings.ProductLifetimeScopePrefab);
            Object.DontDestroyOnLoad(instance.gameObject);
        }
    }
}
```

`ProductLifetimeScope`에서는 사용할 Lighthouse 서비스를 등록합니다.

```csharp
public class ProductLifetimeScope : LifetimeScope
{
    [SerializeField] ProductLifetimeScopeSettings productLifetimeScopeSettings;
    // ...기타 필드 (LHCanvasSceneObject, SupportedLanguageSettings 등)

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ProductEntryPoint>();
        builder.RegisterInstance(productLifetimeScopeSettings);

        // Lighthouse 코어 서비스
        builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneTransitionController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionContextFactory>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneGroupProvider>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<MainSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ModuleSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneCameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionSequenceProvider>(Lifetime.Singleton).AsImplementedInterfaces();

        // 필요에 따라 LighthouseExtends 서비스 추가
    }
}
```

`ProductEntryPoint`에서는 씬 매니저에 부모 스코프를 설정하고 초기 씬을 시작합니다.

```csharp
public class ProductEntryPoint : IAsyncStartable
{
    // ... 필드 및 [Inject] 생성자

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        mainSceneManager.SetEnqueueParentLifetimeScope(() => LifetimeScope.EnqueueParent(productLifetimeScope));
        moduleSceneManager.SetEnqueueParentLifetimeScope(() => LifetimeScope.EnqueueParent(productLifetimeScope));

        // 언어 초기화 (Language 사용 시)
        await languageService.SetLanguage("ja", cancellation);

        // 초기 씬 전환 등
        await launcher.Launch();
    }
}
```

#### 2. Prefab과 ScriptableObject 작성

1. `RootLifetimeScope.prefab`을 생성하고 `RootLifetimeScope` 컴포넌트를 부착합니다
2. `Assets > Create > Scriptable Objects`에서 `RootLifetimeScopeData.asset` (`RootLifetimeScopeSettings`)을 생성합니다
3. 마찬가지로 `ProductLifetimeScope.prefab`과 `ProductLifetimeScopeSettings.asset`을 생성합니다
4. `RootLifetimeScopeData.asset`의 **Product Lifetime Scope Prefab**에 `ProductLifetimeScope.prefab`을 설정합니다
5. `RootLifetimeScope.prefab`의 Inspector에서 **Root Lifetime Scope Settings**에 `RootLifetimeScopeData.asset`을 설정합니다

#### 3. VContainerSettings 설정

`Assets > Create > VContainer > VContainerSettings`에서 `VContainerSettings.asset`을 생성하고,  
**Root Lifetime Scope**에 `RootLifetimeScope.prefab`을 설정합니다.

> **참고:** `VContainerSettings.asset`은 `Resources` 폴더 또는 임의의 폴더에 배치하고,  
> Project Settings의 VContainer 섹션에서 참조하도록 설정할 수도 있습니다. 자세한 내용은 VContainer 문서를 참조하세요.
