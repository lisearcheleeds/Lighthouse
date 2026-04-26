using System;

namespace LighthouseExtends.Addressable
{
    public interface ILHAssetManager : IDisposable
    {
        ILHAssetScope CreateScope();
    }
}
