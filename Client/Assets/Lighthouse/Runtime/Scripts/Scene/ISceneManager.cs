using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Top-level facade for scene navigation. Exposes TransitionScene, BackScene, and PreReboot.
    /// IsTransition indicates whether a transition is currently in progress.
    /// </summary>
    public interface ISceneManager
    {
        bool IsTransition { get; }

        UniTask TransitionScene(
            TransitionDataBase nextTransitionData,
            TransitionType transitionType = TransitionType.Auto,
            MainSceneId backMainSceneId = null);

        UniTask BackScene(TransitionType transitionType = TransitionType.Auto);

        UniTask PreReboot();
    }
}