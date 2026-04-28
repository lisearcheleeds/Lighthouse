# Lighthouse Extends — UIComponent

General-purpose UI components for Lighthouse-based projects. Includes an exclusive-tap button, input blocker, transparent raycast target, and canvas scene object binding.

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| lighthouse | >= 1.0.0 |
| VContainer | >= 1.17.0 |

> Can be used standalone without the Lighthouse core package by omitting `ExclusiveInputService`.

---

## Components

### LHButton

Extends `UnityEngine.UI.Button`. Integrates with `ExclusiveInputService` to accept only the first simultaneous tap in multi-touch environments.

```csharp
// Register ExclusiveInputService with VContainer to enable exclusive-tap behaviour
builder.Register<ExclusiveInputService>(Lifetime.Singleton);
```

Without `ExclusiveInputService`, `LHButton` behaves identically to a standard `Button`.

### LHInputBlocker

Places a full-screen input blocker at a configurable canvas sort order. Supports system and normal tiers.

### LHRaycastTargetObject

A transparent `Graphic` that receives raycasts without rendering. Use it to define hit areas without an image asset.

### LHCanvasSceneObject

Binds a Canvas to the scene camera provided by `ISceneCameraManager`. Used with `CanvasMainSceneBase`.

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
