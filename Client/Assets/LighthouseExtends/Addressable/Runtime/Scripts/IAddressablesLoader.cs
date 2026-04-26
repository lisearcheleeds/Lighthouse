using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LighthouseExtends.Addressable
{
    internal interface IAddressablesLoader
    {
        AsyncOperationHandle LoadAssetAsync<T>(string address) where T : UnityEngine.Object;
        AsyncOperationHandle LoadAssetsAsync<T>(string label, Action<T> callback) where T : UnityEngine.Object;
        void Release(AsyncOperationHandle handle);
    }
}
