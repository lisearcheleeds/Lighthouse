using System;

namespace LighthouseExtends.Addressable
{
    internal sealed class LHAssetHandle<T> : IAssetHandle<T> where T : UnityEngine.Object
    {
        readonly Action onDispose;

        bool disposed;

        public T Asset { get; }

        UnityEngine.Object IAssetHandle.Asset => Asset;

        internal LHAssetHandle(T asset, Action onDispose)
        {
            Asset = asset;
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            onDispose();
        }
    }
}
