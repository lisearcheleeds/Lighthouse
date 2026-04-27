using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LighthouseExtends.Addressable
{
    public sealed class AssetManager : IAssetManager
    {
        class Entry
        {
            public AsyncOperationHandle Handle { get; set; }
            public int RefCount { get; set; }
        }

        readonly Dictionary<string, Entry> entries = new();

        bool disposed;

        public IAssetScope CreateScope()
        {
            return new AssetScope(this);
        }

        internal async UniTask<AssetHandle<T>> LoadInternalAsync<T>(string address, CancellationToken ct)
            where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AssetManager));
            }

            if (!entries.TryGetValue(address, out var entry))
            {
                entry = new Entry
                {
                    Handle = Addressables.LoadAssetAsync<T>(address),
                    RefCount = 0,
                };
                entries[address] = entry;
            }

            entry.RefCount++;

            try
            {
                // ct cancels this await but not the underlying Addressables load;
                // other callers may share the same handle.
                await entry.Handle.ToUniTask(cancellationToken: ct);
                return new AssetHandle<T>((T)entry.Handle.Result, () => Release(address));
            }
            catch
            {
                Release(address);
                throw;
            }
        }

        internal async UniTask<AssetListHandle<T>> LoadAssetsInternalAsync<T>(string label, CancellationToken ct)
            where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AssetManager));
            }

            if (!entries.TryGetValue(label, out var entry))
            {
                entry = new Entry
                {
                    Handle = Addressables.LoadAssetsAsync<T>(label, null),
                    RefCount = 0,
                };
                entries[label] = entry;
            }

            entry.RefCount++;

            try
            {
                // ct cancels this await but not the underlying Addressables load;
                // other callers may share the same handle.
                await entry.Handle.ToUniTask(cancellationToken: ct);
                return new AssetListHandle<T>((IList<T>)entry.Handle.Result, () => Release(label));
            }
            catch
            {
                Release(label);
                throw;
            }
        }

        void Release(string address)
        {
            if (!entries.TryGetValue(address, out var entry))
            {
                return;
            }

            entry.RefCount--;

            if (entry.RefCount <= 0)
            {
                Addressables.Release(entry.Handle);
                entries.Remove(address);
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            foreach (var entry in entries.Values)
            {
                Addressables.Release(entry.Handle);
            }

            entries.Clear();
        }
    }
}
