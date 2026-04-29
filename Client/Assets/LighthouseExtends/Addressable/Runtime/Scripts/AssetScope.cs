using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.Addressable
{
    internal sealed class AssetScope : IAssetScope
    {
        readonly AssetManager manager;
        readonly List<IDisposable> handles = new();

        bool disposed;

        internal AssetScope(AssetManager manager)
        {
            this.manager = manager;
        }

        public async UniTask<IAssetHandle<T>> LoadAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AssetScope));
            }

            var handle = await manager.LoadInternalAsync<T>(address, ct);

            // Scope may have been disposed while awaiting; release the handle immediately.
            if (disposed)
            {
                handle.Dispose();
                throw new ObjectDisposedException(nameof(AssetScope));
            }

            handles.Add(handle);
            return handle;
        }

        public async UniTask<IReadOnlyList<IAssetHandle<T>>> LoadAsync<T>(IReadOnlyList<string> addresses, CancellationToken ct = default) where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AssetScope));
            }

            var acquired = new List<IAssetHandle<T>>(addresses.Count);

            try
            {
                for (var i = 0; i < addresses.Count; i++)
                {
                    var handle = await manager.LoadInternalAsync<T>(addresses[i], ct);

                    if (disposed)
                    {
                        handle.Dispose();
                        throw new ObjectDisposedException(nameof(AssetScope));
                    }

                    acquired.Add(handle);
                }
            }
            catch
            {
                foreach (var h in acquired)
                {
                    h.Dispose();
                }
                throw;
            }

            foreach (var h in acquired)
            {
                handles.Add(h);
            }

            return acquired;
        }

        public async UniTask<IReadOnlyList<T>> LoadByLabelAsync<T>(string label, CancellationToken ct = default) where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AssetScope));
            }

            var handle = await manager.LoadAssetsInternalAsync<T>(label, ct);

            // Scope may have been disposed while awaiting; release the handle immediately.
            if (disposed)
            {
                handle.Dispose();
                throw new ObjectDisposedException(nameof(AssetScope));
            }

            handles.Add(handle);
            return handle.Assets;
        }

        public async UniTask<ParallelLoadResult> TryLoadAsync(ParallelLoadData data, CancellationToken ct = default)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AssetScope));
            }

            var count = data.loaders.Count;

            var tasks = new UniTask<IAssetHandle>[count];
            for (var i = 0; i < count; i++)
            {
                tasks[i] = data.loaders[i](manager, ct);
            }

            var loaded = new IAssetHandle[count];
            var succeeded = new bool[count];

            for (var i = 0; i < count; i++)
            {
                try
                {
                    loaded[i] = await tasks[i];
                    succeeded[i] = true;
                }
                catch
                {
                    succeeded[i] = false;
                }
            }

            if (disposed)
            {
                foreach (var h in loaded)
                {
                    h?.Dispose();
                }
                throw new ObjectDisposedException(nameof(AssetScope));
            }

            foreach (var h in loaded)
            {
                if (h != null)
                {
                    handles.Add(h);
                }
            }

            return new ParallelLoadResult(loaded, succeeded);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            foreach (var handle in handles)
            {
                handle.Dispose();
            }

            handles.Clear();
        }
    }
}
