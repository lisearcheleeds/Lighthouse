using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.Addressable
{
    internal sealed class LHAssetScope : ILHAssetScope
    {
        readonly LHAssetManager manager;
        readonly List<IDisposable> handles = new();

        bool disposed;

        internal LHAssetScope(LHAssetManager manager)
        {
            this.manager = manager;
        }

        public async UniTask<IAssetHandle<T>> LoadAsync<T>(string address, CancellationToken ct = default)
            where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(LHAssetScope));
            }

            var handle = await manager.LoadInternalAsync<T>(address, ct);

            // Scope may have been disposed while awaiting; release the handle immediately.
            if (disposed)
            {
                handle.Dispose();
                throw new ObjectDisposedException(nameof(LHAssetScope));
            }

            handles.Add(handle);
            return handle;
        }

        public async UniTask<IReadOnlyList<T>> LoadAssetsAsync<T>(string label, CancellationToken ct = default)
            where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(LHAssetScope));
            }

            var handle = await manager.LoadAssetsInternalAsync<T>(label, ct);

            if (disposed)
            {
                handle.Dispose();
                throw new ObjectDisposedException(nameof(LHAssetScope));
            }

            handles.Add(handle);
            return handle.Assets;
        }

        public async UniTask<IReadOnlyList<T>> LoadAssetsAsync<T>(IReadOnlyList<string> addresses, CancellationToken ct = default)
            where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(LHAssetScope));
            }

            var result = new T[addresses.Count];
            var acquired = new List<LHAssetHandle<T>>(addresses.Count);

            try
            {
                for (var i = 0; i < addresses.Count; i++)
                {
                    var handle = await manager.LoadInternalAsync<T>(addresses[i], ct);

                    if (disposed)
                    {
                        handle.Dispose();
                        throw new ObjectDisposedException(nameof(LHAssetScope));
                    }

                    result[i] = handle.Asset;
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

            return result;
        }

        public async UniTask<ParallelLoadResult> TryLoadAssetsAsync(ParallelLoadData data, CancellationToken ct = default)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(LHAssetScope));
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
                throw new ObjectDisposedException(nameof(LHAssetScope));
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
