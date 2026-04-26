using System;
using System.Collections.Generic;

namespace LighthouseExtends.Addressable
{
    internal sealed class LHAssetListHandle<T> : IDisposable where T : UnityEngine.Object
    {
        readonly Action onDispose;

        bool disposed;

        public IReadOnlyList<T> Assets { get; }

        // Addressables returns List<T> as IList<T>; List<T> implements IReadOnlyList<T>.
        internal LHAssetListHandle(IList<T> assets, Action onDispose)
        {
            Assets = (IReadOnlyList<T>)assets;
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            onDispose?.Invoke();
        }
    }
}
