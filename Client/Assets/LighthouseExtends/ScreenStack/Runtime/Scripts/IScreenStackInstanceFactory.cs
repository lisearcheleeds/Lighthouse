using System.Threading;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStackInstanceFactory
    {
        UniTask<TScreenStack> CreateScreenStackInstance<TScreenStack>(string screenStackAddress, CancellationToken ct);
    }
}