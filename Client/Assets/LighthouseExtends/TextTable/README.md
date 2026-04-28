# Lighthouse Extends — TextTable

Localization service that loads per-language TSV files via `ITextTableLoader` and resolves `{param}` placeholders at runtime. Includes an editor window for managing keys and translations across all scenes and prefabs.

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| lighthouse-extends.language | >= 1.0.0 |
| R3 | >= 1.3.0 |
| UniTask | >= 2.5.10 |
| VContainer | >= 1.17.0 |

---

## Quick Start

### Implement ITextTableLoader

```csharp
public class MyTextTableLoader : ITextTableLoader
{
    public async UniTask<IReadOnlyDictionary<string, string>> LoadAsync(
        string languageCode, CancellationToken ct)
    {
        var asset = await Addressables.LoadAssetAsync<TextAsset>($"TextTable/{languageCode}").ToUniTask(cancellationToken: ct);
        return ParseTsv(asset.text);
    }
}
```

### Register with VContainer

```csharp
builder.Register<MyTextTableLoader>(Lifetime.Singleton).As<ITextTableLoader>();
builder.Register<TextTableService>(Lifetime.Singleton).As<ITextTableService>();
```

### Usage

```csharp
// Simple lookup
string text = textTableService.GetText(new TextData("HomeTitle"));

// With parameters  (TSV: "Retries: {count}" → "Retries: 3")
string text = textTableService.GetText(
    new TextData("RetryCount", new Dictionary<string, object> { ["count"] = 3 }));
```

### Editor Window

`Window > Lighthouse > TextTable Editor` — view, edit, and validate all keys and translations.

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
