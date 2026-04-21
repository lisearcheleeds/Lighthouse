# Step 5: Launcher Scene and Launcher Implementation

## Setup Guide — Table of Contents

- [Step 1: Generate Settings Assets](./step1.md)
- [Step 2: ScreenStack Setup](./step2.md)
- [Step 3: TextTable Setup](./step3.md)
- [Step 4: VContainer LifetimeScope Implementation](./step4.md)
- [Step 5: Launcher Scene and Launcher Implementation](./step5.md)
- [Step 6: Create Scene Files and Register to Build Settings](./step6.md)
- [Step 7: Generate Scene Scripts](./step7.md)
- [Step 8: Scene LifetimeScope and Base Class Implementation](./step8.md)

---

### Role of the Launcher Scene

The Launcher scene is the **entry point scene** for the application.  
It is loaded first when Unity starts and serves as the origin from which VContainer creates the `RootLifetimeScope`.  
Keep the scene itself as empty as possible — do not put game logic in it.

```
Build Settings:
  0: Launcher      ← loaded first on startup
  1: (game scenes)
```

### Role of ILauncher / Launcher

`ILauncher` is the interface responsible for the initial launch and reboot of the application.

```csharp
public interface ILauncher
{
    UniTask Launch();   // initial launch
    void Reboot();      // restart the app (return to Launcher scene and re-execute)
}
```

Register `Launcher` in `ProductLifetimeScope` so that it can be called from `ProductEntryPoint`.

```csharp
builder.Register<Launcher>(Lifetime.Singleton).AsImplementedInterfaces();
```

### Implementation Example

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
        await FirstLaunchProcess();  // one-time initialization (cache building, etc.)
        await LaunchProcess();       // per-launch initialization (master data loading, etc.)
        TransitionNextScene();       // transition to the first game scene
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

    UniTask FirstLaunchProcess() => UniTask.CompletedTask;  // implement as needed
    UniTask LaunchProcess() => UniTask.CompletedTask;       // implement as needed

    void TransitionNextScene()
    {
        UniTask.Void(async () =>
        {
            await sceneManager.TransitionScene(new SplashTransitionData());

            // Unload the Launcher scene after transition completes
            if (!string.IsNullOrEmpty(
                    UnityEngine.SceneManagement.SceneManager.GetSceneByName(LauncherSceneName).name))
            {
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(LauncherSceneName);
            }
        });
    }
}
```

### Full Startup Flow

```
Unity starts
  └── Load Launcher scene
        └── VContainerSettings auto-instantiates RootLifetimeScope Prefab
              └── RootEntryPoint.Start()
                    └── Instantiate ProductLifetimeScope Prefab as child scope
                          └── ProductEntryPoint.StartAsync()
                                ├── Register parent scope with scene managers
                                ├── Language initialization
                                └── Launcher.Launch()
                                      ├── First-launch process
                                      ├── Launch process
                                      └── Transition to first scene → Unload Launcher scene
```
