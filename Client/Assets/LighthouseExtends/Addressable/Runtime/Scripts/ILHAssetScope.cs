using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.Addressable
{
    public interface ILHAssetScope : IDisposable
    {
        UniTask<IAssetHandle<T>> LoadAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object;

        UniTask<IReadOnlyList<T>> LoadByLabelAsync<T>(string label, CancellationToken ct = default) where T : UnityEngine.Object;

        UniTask<IReadOnlyList<T>> LoadAssetsAsync<T>(IReadOnlyList<string> addresses, CancellationToken ct = default) where T : UnityEngine.Object;

        UniTask<ParallelLoadResult> TryLoadAssetsAsync(ParallelLoadData data, CancellationToken ct = default);
    }
}
