using System;

namespace LighthouseExtends.Addressable
{
    public interface IAssetHandle : IDisposable
    {
        UnityEngine.Object Asset { get; }
    }

    public interface IAssetHandle<out T> : IAssetHandle where T : UnityEngine.Object
    {
        new T Asset { get; }
    }
}
