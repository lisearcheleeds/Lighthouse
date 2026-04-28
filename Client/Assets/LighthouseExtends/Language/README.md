# Lighthouse Extends — Language

Reactive language switching service for Lighthouse. Registered handlers run in parallel before `CurrentLanguage` updates, ensuring fonts and text are loaded before the UI reflects the change.

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| R3 | >= 1.3.0 |
| UniTask | >= 2.5.10 |
| VContainer | >= 1.17.0 |

---

## Quick Start

### Register with VContainer

```csharp
builder.RegisterComponentInHierarchy<SupportedLanguageSettings>();
builder.Register<SupportedLanguageService>(Lifetime.Singleton).As<ISupportedLanguageService>();
builder.Register<LanguageService>(Lifetime.Singleton).As<ILanguageService>();
```

### Usage

```csharp
// Switch language — all handlers complete before CurrentLanguage updates
await languageService.SetLanguage("ja", cancellationToken);

// Subscribe to changes
languageService.CurrentLanguage.Subscribe(lang => Debug.Log(lang));

// Register a pre-load handler (e.g. load fonts or text tables before the switch)
languageService.RegisterChangeHandler(async (lang, ct) =>
{
    await LoadResourcesForLanguage(lang, ct);
});
```

### SupportedLanguageSettings

Create via `Create > Lighthouse > Language > Supported Language Settings` and configure the list of supported language codes and the default language.

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
