# Lighthouse

A Unity scene management framework providing a structured scene transition pipeline, camera stack management, and back navigation — built on [VContainer](https://github.com/hadashiA/VContainer) and [UniTask](https://github.com/Cysharp/UniTask).

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| UniTask | >= 2.5.10 |
| VContainer | >= 1.17.0 |
| Universal RP | >= 17.0.0 |

---

## Quick Start

### 1. Define a Scene ID and Transition Data

```csharp
public static class SceneIds
{
    public static readonly MainSceneId Home = new MainSceneId(1, "HomeScene");
}

public class HomeTransitionData : TransitionDataBase
{
    public override MainSceneId MainSceneId => SceneIds.Home;
}
```

### 2. Implement a Scene

```csharp
public class HomeScene : CanvasMainSceneBase<HomeTransitionData>
{
    protected override UniTask OnEnter(
        HomeTransitionData data, ISceneTransitionContext context, CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }
}
```

### 3. Transition

```csharp
await sceneManager.TransitionScene(new HomeTransitionData());
await sceneManager.BackScene();
```

---

## Documentation

Full feature reference, setup guide, and architecture overview:  
[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

| Language | Link |
|---|---|
| English | [Docs/en](https://github.com/lisearcheleeds/Lighthouse/blob/master/Docs/en/readme.md) |
| 日本語 | [Docs/ja](https://github.com/lisearcheleeds/Lighthouse/blob/master/Docs/ja/readme.md) |
| 한국어 | [Docs/ko](https://github.com/lisearcheleeds/Lighthouse/blob/master/Docs/ko/readme.md) |
| 中文 | [Docs/zh](https://github.com/lisearcheleeds/Lighthouse/blob/master/Docs/zh/readme.md) |

---

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
