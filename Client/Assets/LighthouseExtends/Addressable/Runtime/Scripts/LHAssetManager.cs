using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LighthouseExtends.Addressable
{
    /// <summary>
    /// Implements ILHAssetManager. Shares a single Addressables handle per address with ref-counting;
    /// Addressables.Release is called only when all holders have released.
    /// </summary>
    public sealed class LHAssetManager : ILHAssetManager, IDisposable
    {
        class Entry
        {
            public AsyncOperationHandle Handle { get; set; }
            public int RefCount { get; set; }
        }

        readonly IAddressablesLoader loader;
        readonly Dictionary<string, Entry> entries = new();

        bool disposed;

        public LHAssetManager() : this(new DefaultAddressablesLoader()) { }

        internal LHAssetManager(IAddressablesLoader loader)
        {
            this.loader = loader;
        }

        public ILHAssetScope CreateScope()
        {
            return new LHAssetScope(this);
        }

        internal async UniTask<LHAssetHandle<T>> LoadInternalAsync<T>(string address, CancellationToken ct)
            where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(LHAssetManager));
            }

            if (!entries.TryGetValue(address, out var entry))
            {
                entry = new Entry
                {
                    Handle = loader.LoadAssetAsync<T>(address),
                    RefCount = 0,
                };
                entries[address] = entry;
            }

            // Increment before await so concurrent calls to the same address accumulate RefCount correctly.
            entry.RefCount++;

            try
            {
                // ct cancels this await but not the underlying Addressables load; other callers may share the same handle.
                await entry.Handle.ToUniTask(cancellationToken: ct);
                return new LHAssetHandle<T>((T)entry.Handle.Result, () => Release(address));
            }
            catch
            {
                Release(address);
                throw;
            }
        }

        internal async UniTask<LHAssetListHandle<T>> LoadAssetsInternalAsync<T>(string label, CancellationToken ct)
            where T : UnityEngine.Object
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(LHAssetManager));
            }

            if (!entries.TryGetValue(label, out var entry))
            {
                entry = new Entry
                {
                    Handle = loader.LoadAssetsAsync<T>(label, null),
                    RefCount = 0,
                };
                entries[label] = entry;
            }

            entry.RefCount++;

            try
            {
                await entry.Handle.ToUniTask(cancellationToken: ct);
                return new LHAssetListHandle<T>((IList<T>)entry.Handle.Result, () => Release(label));
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
                loader.Release(entry.Handle);
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
                loader.Release(entry.Handle);
            }

            entries.Clear();
        }
    }
}
