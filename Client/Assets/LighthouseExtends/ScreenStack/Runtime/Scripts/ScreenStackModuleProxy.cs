using System;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.ScreenStack
{
    public sealed class ScreenStackModuleProxy : IScreenStackModule, IScreenStackModuleProxy
    {
        IScreenStackModuleImpl module;

        UniTask IScreenStackModule.Enqueue(IScreenStackData screenStackData)
        {
            return module?.Enqueue(screenStackData) ?? UniTask.CompletedTask;
        }

        UniTask IScreenStackModule.Open()
        {
            return module?.Open() ?? UniTask.CompletedTask;
        }

        UniTask IScreenStackModule.Open(IScreenStackData screenStackData)
        {
            return module?.Open(screenStackData) ?? UniTask.CompletedTask;
        }

        UniTask IScreenStackModule.Close()
        {
            return module?.Close() ?? UniTask.CompletedTask;
        }

        UniTask IScreenStackModule.Close(IScreenStackData screenStackData)
        {
            return module?.Close(screenStackData) ?? UniTask.CompletedTask;
        }

        UniTask IScreenStackModule.ClearAll()
        {
            return module?.ClearAll() ?? UniTask.CompletedTask;
        }

        UniTask IScreenStackModule.ClearCurrentAll()
        {
            return module?.ClearCurrentAll() ?? UniTask.CompletedTask;
        }

        void IScreenStackModuleProxy.RegisterModule(IScreenStackModuleImpl module)
        {
            if (this.module != null)
            {
                throw new ArgumentException($"Duplicate register");
            }

            this.module = module;
        }

        void IScreenStackModuleProxy.UnregisterModule(IScreenStackModuleImpl module)
        {
            if (this.module != module)
            {
                throw new ArgumentException($"No match impl");
            }

            this.module = null;
        }
    }
}