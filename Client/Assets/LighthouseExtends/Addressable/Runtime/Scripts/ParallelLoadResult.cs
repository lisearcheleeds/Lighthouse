namespace LighthouseExtends.Addressable
{
    public sealed class ParallelLoadResult
    {
        readonly IAssetHandle[] handles;
        readonly bool[] succeeded;

        internal ParallelLoadResult(IAssetHandle[] handles, bool[] succeeded)
        {
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
