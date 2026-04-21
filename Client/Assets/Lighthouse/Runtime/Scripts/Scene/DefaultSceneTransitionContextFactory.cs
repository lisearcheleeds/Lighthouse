using Lighthouse.Scene.SceneCamera;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Default implementation of ISceneTransitionContextFactory.
    /// Assembles a SceneTransitionContext from the provided transition data and manager references.
    /// </summary>
    public class DefaultSceneTransitionContextFactory : ISceneTransitionContextFactory
    {
        public ISceneTransitionContext Create(
            TransitionDataBase transitionData,
            TransitionDirectionType transitionDirectionType,
            TransitionType transitionType,
            SceneTransitionDiff sceneTransitionDiff,
            IMainSceneManager mainSceneManager,
            IModuleSceneManager moduleSceneManager,
            ISceneCameraManager sceneCameraManager)
        {
            return new SceneTransitionContext(
                transitionData,
                transitionDirectionType,
                transitionType,
                sceneTransitionDiff,
                mainSceneManager,
                moduleSceneManager,
                sceneCameraManager);
        }
    }
}
