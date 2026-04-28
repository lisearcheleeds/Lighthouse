# Lighthouse Extends — ScreenStack

Dialog and overlay stack management with system/default layers, input blocking, and scene suspend/resume. Open/Close operations are queued and executed sequentially.

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| lighthouse | >= 1.0.0 |
| lighthouse-extends.uicomponent | >= 1.0.0 |
| UniTask | >= 2.5.10 |
| VContainer | >= 1.17.0 |

---

## Quick Start

### Define Screen Data

```csharp
public class SampleDialogData : IScreenStackData
{
    public bool IsSystem => false;
    public bool IsOverlayOpen => false;
    public string Message { get; }
    public SampleDialogData(string message) => Message = message;
}
```

### Implement a Dialog

```csharp
public class SampleDialog : ScreenStackBase
{
    public override async UniTask OnEnter(bool isResume)
    {
        // Initialize UI
    }
}
```

### Usage

```csharp
// Open a dialog
await screenStackModule.Open(new SampleDialogData("Hello"));

// Close the top dialog
await screenStackModule.Close();

// Close a specific dialog by its data reference
await screenStackModule.Close(myDialogData);

// Clear all dialogs
await screenStackModule.ClearAll();
```

### Scene Suspend / Resume

Inherit from `ScreenStackModuleSceneBase` to automatically save and restore the stack across scene transitions.

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
