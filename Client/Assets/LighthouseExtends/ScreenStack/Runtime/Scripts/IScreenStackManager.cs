using Cysharp.Threading.Tasks;
using Lighthouse.Scene;

namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStackManager
    {
        void Setup();

        UniTask Enqueue(IScreenStackData screenStackData);
        UniTask Open();
        UniTask Open(IScreenStackData screenStackData);

        UniTask Close();
        UniTask Close(IScreenStackData screenStackData);

        UniTask ClearAll();
        UniTask ClearCurrentAll();

        UniTask ResumeFromSceneId(MainSceneId mainSceneId, bool isPlayInAnimation);
        UniTask SuspendFromSceneId(MainSceneId mainSceneId);
    }
}