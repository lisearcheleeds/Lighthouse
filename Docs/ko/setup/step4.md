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

> VContainer 기본 사용법: [https://vcontainer.hadashikick.jp](https://vcontainer.hadashikick.jp)

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
    [SerializeField] SupportedLanguageSettings supportedLanguageSettings;  // Language 사용 시
    [SerializeField] LanguageFontSettings languageFontSettings;            // Font 사용 시
    [SerializeField] LHCanvasSceneObject canvasSceneObjectPrefab;
    [SerializeField] LHInputBlocker inputBlockerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ProductEntryPoint>();
        builder.RegisterInstance(productLifetimeScopeSettings);

        // Lighthouse 코어
        builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneTransitionController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionContextFactory>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneGroupProvider>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<MainSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ModuleSceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SceneCameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<DefaultSceneTransitionSequenceProvider>(Lifetime.Singleton).AsImplementedInterfaces();

        // LighthouseExtends — Language / Font / TextTable
        builder.RegisterInstance(supportedLanguageSettings);
        builder.Register<SupportedLanguageService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<LanguageService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterInstance(languageFontSettings);
        builder.Register<FontService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<TextTableService>(Lifetime.Singleton).AsImplementedInterfaces();

        // FontService와 TextTableService는 기본적으로 지연 해결됩니다.
        // SetLanguage가 호출되기 전에 ILanguageService에 핸들러를 등록하기 위해
        // RegisterBuildCallback으로 강제적으로 즉시 해결합니다.
        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<IFontService>();
            container.Resolve<ITextTableService>();
        });

        // LighthouseExtends — UIComponent / InputLayer
        builder.Register<ExclusiveInputService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterComponentInNewPrefab(canvasSceneObjectPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad().AsImplementedInterfaces();
        builder.RegisterComponentInNewPrefab(inputBlockerPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad().AsImplementedInterfaces();

        var inputActions = new InputActions();
        builder.RegisterInstance(inputActions);
        builder.RegisterInstance(inputActions.asset).As<InputActionAsset>();
        builder.Register<InputLayerController>(Lifetime.Singleton).AsImplementedInterfaces();

        // LighthouseExtends — ScreenStack
        builder.Register<ScreenStackModuleProxy>(Lifetime.Singleton).AsImplementedInterfaces();

        // 프로덕트 고유 서비스
        builder.Register<Launcher>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

> **왜 `RegisterBuildCallback`이 필요한가?**  
> VContainer의 싱글턴은 기본적으로 지연 해결됩니다. FontService와 TextTableService는 `ProductEntryPoint`에서 `SetLanguage`가 호출되기 *전에* `ILanguageService`에 핸들러를 등록해 두어야 합니다. 이를 생략하면 첫 번째 언어 전환 완료 시점에 폰트와 텍스트가 아직 로드되지 않아 UI가 잘못된 상태가 됩니다.

`ProductEntryPoint`에서는 씬 매니저에 부모 스코프 설정과 단말기 로케일로부터의 언어 해결을 수행합니다.

```csharp
public class ProductEntryPoint : IAsyncStartable
{
    readonly ProductLifetimeScope productLifetimeScope;
    readonly ILauncher launcher;
    readonly IMainSceneManager mainSceneManager;
    readonly IModuleSceneManager moduleSceneManager;
    readonly ILanguageService languageService;
    readonly ISupportedLanguageService supportedLanguageService;

    [Inject]
    public ProductEntryPoint(
        ProductLifetimeScope productLifetimeScope,
        ILauncher launcher,
        IMainSceneManager mainSceneManager,
        IModuleSceneManager moduleSceneManager,
        ILanguageService languageService,
        ISupportedLanguageService supportedLanguageService)
    {
        this.productLifetimeScope = productLifetimeScope;
        this.launcher = launcher;
        this.mainSceneManager = mainSceneManager;
        this.moduleSceneManager = moduleSceneManager;
        this.languageService = languageService;
        this.supportedLanguageService = supportedLanguageService;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        mainSceneManager.SetEnqueueParentLifetimeScope(
            () => LifetimeScope.EnqueueParent(productLifetimeScope));
        moduleSceneManager.SetEnqueueParentLifetimeScope(
            () => LifetimeScope.EnqueueParent(productLifetimeScope));

        await languageService.SetLanguage(
            ResolveInitialLanguage(Application.systemLanguage), cancellation);

        await launcher.Launch();
    }

    string ResolveInitialLanguage(SystemLanguage systemLanguage)
    {
        var code = systemLanguage switch
        {
            SystemLanguage.Japanese           => "ja",
            SystemLanguage.ChineseSimplified  => "zh",
            SystemLanguage.ChineseTraditional => "zh",
            SystemLanguage.Korean             => "ko",
            _                                 => "en",
        };

        // 지원 언어에 포함되지 않은 경우 기본 언어로 폴백
        var supported = supportedLanguageService.SupportedLanguages;
        for (var i = 0; i < supported.Count; i++)
        {
            if (supported[i] == code) return code;
        }

        return supportedLanguageService.DefaultLanguage;
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
