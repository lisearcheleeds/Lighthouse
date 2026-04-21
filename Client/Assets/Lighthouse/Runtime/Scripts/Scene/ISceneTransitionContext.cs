using Lighthouse.Scene.SceneCamera;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Shared context passed through all phases and steps of a scene transition.
    /// Exposes TransitionData, SceneTransitionDiff, and the manager interfaces needed by each step.
    /// </summary>
    public interface ISceneTransitionContext
    {
        TransitionDataBase TransitionData { get; }
        TransitionDirectionType TransitionDirectionType { get; }
        TransitionType TransitionType { get; }
        SceneTransitionDiff SceneTransitionDiff { get; }

        IMainSceneManager MainSceneManager { get; }
        IModuleSceneManager ModuleSceneManager { get; }
        ISceneCameraManager SceneCameraManager { get; }
    }
}
