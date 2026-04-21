using System.Threading;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStackEntityFactory
    {
        UniTask<ScreenStackEntity> CreateAsync(IScreenStackData data, CancellationToken ct);
    }
}