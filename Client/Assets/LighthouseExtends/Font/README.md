# Lighthouse Extends — Font

Automatically switches `TMP_FontAsset` based on the current language. Subscribes to `ILanguageService` and updates `CurrentFont` before `CurrentLanguage` is published, so `LHTextMeshPro` always receives the correct font.

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

### Register with VContainer

```csharp
builder.RegisterComponentInHierarchy<LanguageFontSettings>();
builder.Register<FontService>(Lifetime.Singleton).As<IFontService>();
```

### LanguageFontSettings

Create via `Create > Lighthouse > Font > Language Font Settings` and map each language code to a `TMP_FontAsset`.

### Usage

```csharp
// Subscribe to font changes
fontService.CurrentFont.Subscribe(font => myText.font = font);

// Get font for a specific language directly
TMP_FontAsset font = fontService.GetFont("ko");
```

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
