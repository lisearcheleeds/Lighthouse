using System;
using System.Collections.Generic;
using LighthouseExtends.Addressable;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LighthouseExtends.Addressable.Tests
{
    internal sealed class FakeAddressablesLoader : IAddressablesLoader
    {
        readonly Dictionary<string, UnityEngine.Object> assets = new();
        readonly Dictionary<string, object> labelAssets = new();

        public int LoadCount { get; private set; }
        public int ReleaseCount { get; private set; }

        public void RegisterAsset(string address, UnityEngine.Object asset)
        {
            assets[address] = asset;
        }

        public void RegisterLabel<T>(string label, IList<T> list) where T : UnityEngine.Object
        {
            labelAssets[label] = list;
        }

        public AsyncOperationHandle LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            LoadCount++;
            var asset = (T)assets[address];
            return Addressables.ResourceManager.CreateCompletedOperation(asset, null);
        }

        public AsyncOperationHandle LoadAssetsAsync<T>(string label, Action<T> callback) where T : UnityEngine.Object
        {
            var result = new List<T>();
            if (labelAssets.TryGetValue(label, out var raw))
            {
                foreach (var item in (IList<T>)raw)
                {
                    result.Add(item);
                    callback?.Invoke(item);
                }
            }
            return Addressables.ResourceManager.CreateCompletedOperation<IList<T>>(result, null);
        }

        public void Release(AsyncOperationHandle handle)
        {
            ReleaseCount++;
        }
    }
}
