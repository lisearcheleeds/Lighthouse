using Lighthouse.Scene.SceneCamera;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Factory interface for creating an ISceneTransitionContext at the start of each transition.
    /// Implement this to inject a custom context with additional data or services.
    /// </summary>
    public interface ISceneTransitionContextFactory
    {
        ISceneTransitionContext Create(
            TransitionDataBase transitionData,
            TransitionDirectionType transitionDirectionType,
            TransitionType transitionType,
            SceneTransitionDiff sceneTransitionDiff,
            IMainSceneManager mainSceneManager,
            IModuleSceneManager moduleSceneManager,
            ISceneCameraManager sceneCameraManager);
    }
}
