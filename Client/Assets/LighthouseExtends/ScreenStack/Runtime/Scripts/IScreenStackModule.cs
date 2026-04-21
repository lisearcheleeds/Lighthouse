using Cysharp.Threading.Tasks;

namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStackModule
    {
        UniTask Enqueue(IScreenStackData screenStackData);
        UniTask Open();
        UniTask Open(IScreenStackData screenStackData);

        UniTask Close();
        UniTask Close(IScreenStackData screenStackData);

        UniTask ClearAll();
        UniTask ClearCurrentAll();
    }
}