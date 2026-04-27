using System;

namespace LighthouseExtends.Addressable
{
    public interface IAssetManager : IDisposable
    {
        IAssetScope CreateScope();
    }
}
