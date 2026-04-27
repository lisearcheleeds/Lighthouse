using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.Addressable
{
    public sealed class ParallelLoadData
    {
        internal readonly List<Func<AssetManager, CancellationToken, UniTask<IAssetHandle>>> loaders = new();

        public AssetRequest<T> Add<T>(string address) where T : UnityEngine.Object
        {
            var index = loaders.Count;
            loaders.Add(async (manager, ct) => await manager.LoadInternalAsync<T>(address, ct));
            return new AssetRequest<T>(index);
        }
    }
}
