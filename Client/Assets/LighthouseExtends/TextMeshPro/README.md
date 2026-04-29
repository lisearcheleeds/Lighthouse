# Lighthouse Extends — TextMeshPro

Extends `TextMeshProUGUI` to automatically reflect language and font changes via `TextTableService` and `FontService`. Assign a Text Key in the Inspector, or set `ITextData` from code.

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| lighthouse-extends.font | >= 1.0.0 |
| lighthouse-extends.texttable | >= 1.0.0 |
| R3 | >= 1.3.0 |

---

## Quick Start

### In the Inspector

Attach `LHTextMeshPro` (instead of `TextMeshProUGUI`) to any UI GameObject and fill in the **Text Key** field. The text and font update automatically whenever the language changes.

### In Code

```csharp
// Set text by key
lhText.SetTextData(new TextData("HomeTitle"));

// With runtime parameters
lhText.SetTextData(new TextData("RetryCount",
    new Dictionary<string, object> { ["count"] = 3 }));
```

If `TextTableService` is not registered, the key string is displayed as-is.  
If `FontService` is not registered, automatic font switching is skipped.

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
