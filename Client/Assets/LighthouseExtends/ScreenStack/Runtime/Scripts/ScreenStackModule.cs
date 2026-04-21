using Cysharp.Threading.Tasks;
using VContainer;

namespace LighthouseExtends.ScreenStack
{
    public sealed class ScreenStackModule : IScreenStackModuleImpl
    {
        readonly IScreenStackManager screenStackManager;

        [Inject]
        public ScreenStackModule(IScreenStackManager screenStackManager)
        {
            this.screenStackManager = screenStackManager;
        }

        UniTask IScreenStackModule.Enqueue(IScreenStackData screenStackData)
        {
            return screenStackManager.Enqueue(screenStackData);
        }

        UniTask IScreenStackModule.Open()
        {
            return screenStackManager.Open();
        }

        UniTask IScreenStackModule.Open(IScreenStackData screenStackData)
        {
            return screenStackManager.Open(screenStackData);
        }

        UniTask IScreenStackModule.Close()
        {
            return screenStackManager.Close();
        }

        UniTask IScreenStackModule.Close(IScreenStackData screenStackData)
        {
            return screenStackManager.Close(screenStackData);
        }

        UniTask IScreenStackModule.ClearAll()
        {
            return screenStackManager.ClearAll();
        }

        UniTask IScreenStackModule.ClearCurrentAll()
        {
            return screenStackManager.ClearCurrentAll();
        }
    }
}