using System;

namespace Lighthouse.Input
{
    /// <summary>
    /// This class is for blocking full-screen input.
    /// This functionality needs to be implemented on the production side.
    /// If necessary, it is recommended to implement the Sample directly.
    /// https://github.com/lisearcheleeds/LighthouseSample/blob/master/Client/Assets/LighthouseExtends/UIComponent/Runtime/Scripts/InputBlocker/LHInputBlocker.cs
    /// </summary>
    public interface IInputBlocker
    {
        IDisposable Block<T>(bool isSystemLayer = false);
        void UnBlock<T>(bool isSystemLayer = false);
    }
}
