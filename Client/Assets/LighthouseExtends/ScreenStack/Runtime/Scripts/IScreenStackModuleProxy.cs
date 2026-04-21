namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStackModuleProxy
    {
        void RegisterModule(IScreenStackModuleImpl module);
        void UnregisterModule(IScreenStackModuleImpl module);
    }
}