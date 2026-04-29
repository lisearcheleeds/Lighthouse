using System;

namespace LighthouseExtends.Addressable
{
    public sealed class ParallelLoadResult
    {
        readonly IAssetHandle[] handles;
        readonly bool[] succeeded;

        public ParallelLoadResult(IAssetHandle[] handles, bool[] succeeded)
        {
            if (handles.Length != succeeded.Length)
            {
                throw new ArgumentException("handles and succeeded must have the same length.");
            }

            this.handles = handles;
            this.succeeded = succeeded;
        }

        public bool IsSuccess<T>(AssetRequest<T> request) where T : UnityEngine.Object
        {
            return succeeded[request.Index];
        }

        public IAssetHandle<T> Get<T>(AssetRequest<T> request) where T : UnityEngine.Object
        {
            return succeeded[request.Index] ? (IAssetHandle<T>)handles[request.Index] : null;
        }
    }
}
