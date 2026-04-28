# Lighthouse Extends — InputLayer

Stack-based input dispatch over `InputActionAsset`. Events are dispatched from the top layer downward and stop when consumed (`return true`) or blocked (`BlocksAllInput = true`). An `ActionMap` is disabled on pop only when no remaining layers reference it.

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| Input System | >= 1.17.0 |
| VContainer | >= 1.17.0 |

---

## Quick Start

### Register with VContainer

```csharp
builder.RegisterComponentInHierarchy<InputActionAsset>();
builder.Register<InputLayerController>(Lifetime.Singleton).As<IInputLayerController>().AsSelf();
```

### Implement IInputLayer

```csharp
public class HomeInputLayer : IInputLayer
{
    public bool BlocksAllInput => false;

    public bool OnActionStarted(InputAction.CallbackContext ctx) => false;

    public bool OnActionPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.action.name == "Back")
        {
            HandleBack();
            return true; // consumed — lower layers do not receive this event
        }
        return false;
    }

    public bool OnActionCanceled(InputAction.CallbackContext ctx) => false;
}
```

### Push / Pop

```csharp
inputLayerController.PushLayer(myLayer, myActionMap);   // ActionMap auto-enabled
inputLayerController.PopLayer();                        // removes top layer
inputLayerController.PopLayer(myLayer);                 // removes specific layer by reference
```

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
