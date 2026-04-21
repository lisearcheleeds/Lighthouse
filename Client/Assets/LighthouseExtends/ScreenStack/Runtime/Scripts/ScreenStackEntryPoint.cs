using System;
using VContainer;
using VContainer.Unity;

namespace LighthouseExtends.ScreenStack
{
    public class ScreenStackEntryPoint : IStartable, IDisposable
    {
        readonly IScreenStackModuleProxy screenStackModuleProxy;
        readonly IScreenStackModuleImpl screenStackModuleImpl;

        [Inject]
        public ScreenStackEntryPoint(IScreenStackModuleProxy screenStackModuleProxy, IScreenStackModuleImpl screenStackModuleImpl)
        {
            this.screenStackModuleProxy = screenStackModuleProxy;
            this.screenStackModuleImpl = screenStackModuleImpl;
        }

        void IStartable.Start()
        {
            screenStackModuleProxy.RegisterModule(screenStackModuleImpl);
        }

        void IDisposable.Dispose()
        {
            screenStackModuleProxy.UnregisterModule(screenStackModuleImpl);
        }
    }
}