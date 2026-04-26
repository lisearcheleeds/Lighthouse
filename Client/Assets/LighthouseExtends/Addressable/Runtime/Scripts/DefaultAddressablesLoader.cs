using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LighthouseExtends.Addressable
{
    internal sealed class DefaultAddressablesLoader : IAddressablesLoader
    {
        public AsyncOperationHandle LoadAssetAsync<T>(string address) where T : UnityEngine.Object
            => Addressables.LoadAssetAsync<T>(address);

        public AsyncOperationHandle LoadAssetsAsync<T>(string label, Action<T> callback) where T : UnityEngine.Object
            => Addressables.LoadAssetsAsync<T>(label, callback);

        public void Release(AsyncOperationHandle handle)
            => Addressables.Release(handle);
    }
}
