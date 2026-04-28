# Lighthouse Extends — Addressable

Ref-counted Addressables wrapper with scoped asset lifetime management and parallel loading support. Multiple scopes sharing the same address reuse the same underlying handle; `Addressables.Release` is called only when the last holder disposes.

**Version:** 1.0.0 · **Unity:** 6000.0+

---

## Dependencies

| Package | Version |
|---|---|
| UniTask | >= 2.5.10 |
| Addressables | >= 2.9.1 |

---

## Quick Start

### Register with VContainer

```csharp
builder.Register<AssetManager>(Lifetime.Singleton).As<IAssetManager>();
```

### Load Assets in a Scope

```csharp
IAssetScope scope = assetManager.CreateScope();

// Single asset
IAssetHandle<Sprite> handle = await scope.LoadAsync<Sprite>("ui/icon_home", ct);
icon.sprite = handle.Asset;

// Multiple assets by label
IReadOnlyList<AudioClip> clips = await scope.LoadByLabelAsync<AudioClip>("bgm", ct);

// Release all assets held by this scope
scope.Dispose();
```

### Parallel Loading

```csharp
var data = new ParallelLoadData();
var iconReq  = data.Add<Sprite>("ui/icon_home");
var audioReq = data.Add<AudioClip>("audio/bgm_home");

ParallelLoadResult result = await scope.TryLoadAsync(data, ct);

if (result.IsSuccess(iconReq))
    icon.sprite = result.Get(iconReq).Asset;
```

Partial failures are tolerated — one failed request does not cancel the others.

---

## Documentation

[https://github.com/lisearcheleeds/Lighthouse](https://github.com/lisearcheleeds/Lighthouse)

## License

[MIT License](https://github.com/lisearcheleeds/Lighthouse/blob/master/LICENSE)
