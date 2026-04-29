using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.Addressable
{
    public interface IAssetScope : IDisposable
    {
        UniTask<IAssetHandle<T>> LoadAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object;

        UniTask<IReadOnlyList<IAssetHandle<T>>> LoadAsync<T>(IReadOnlyList<string> addresses, CancellationToken ct = default) where T : UnityEngine.Object;

        UniTask<IReadOnlyList<T>> LoadByLabelAsync<T>(string label, CancellationToken ct = default) where T : UnityEngine.Object;

        UniTask<ParallelLoadResult> TryLoadAsync(ParallelLoadData data, CancellationToken ct = default);
    }
}
