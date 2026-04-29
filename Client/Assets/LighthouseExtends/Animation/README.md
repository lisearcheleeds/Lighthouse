# Lighthouse Extends — Animation

In/Out `AnimationClip` playback via `PlayableGraph` for Lighthouse scene transitions and dialogs. Supports per-direction start delay and mutual exclusion between In and Out.

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| UniTask | >= 2.5.10 |

> Can be used standalone without the Lighthouse core package.

---

## Quick Start

### LHTransitionAnimator

Attach `LHTransitionAnimator` to a scene or dialog root. Connect it to the animation hooks of `SceneBase` or `ScreenStackBase`:

```csharp
public class HomeScene : CanvasMainSceneBase<HomeTransitionData>
{
    [SerializeField] LHTransitionAnimator transitionAnimator;

    protected override UniTask InAnimation(ISceneTransitionContext context)
        => transitionAnimator.InAnimation();

    protected override UniTask OutAnimation(ISceneTransitionContext context)
        => transitionAnimator.OutAnimation();
}
```

### Inspector Settings

| Field | Description |
|---|---|
| `inAnimationClips` | Clips played sequentially on entry |
| `inDelayMilliSec` | Delay before in-animation starts (ms) |
| `outAnimationClips` | Clips played sequentially on leave |
| `outDelayMilliSec` | Delay before out-animation starts (ms) |

In and Out are mutually exclusive — starting one stops the other automatically.

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
