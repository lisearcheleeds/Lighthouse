using Lighthouse.Scene.SceneCamera;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Concrete implementation of ISceneTransitionContext.
    /// Constructed once per transition by ISceneTransitionContextFactory and handed to every step in the pipeline.
    /// </summary>
    public class SceneTransitionContext : ISceneTransitionContext
    {
        public TransitionDataBase TransitionData { get; }
        public TransitionDirectionType TransitionDirectionType { get; }
        public TransitionType TransitionType { get; }
        public SceneTransitionDiff SceneTransitionDiff { get; }

        public IMainSceneManager MainSceneManager { get; }
        public IModuleSceneManager ModuleSceneManager { get; }
        public ISceneCameraManager SceneCameraManager { get; }

        public SceneTransitionContext(
            TransitionDataBase transitionData,
            TransitionDirectionType transitionDirectionType,
            TransitionType transitionType,
            SceneTransitionDiff sceneTransitionDiff,
            IMainSceneManager mainSceneManager,
            IModuleSceneManager moduleSceneManager,
            ISceneCameraManager sceneCameraManager)
        {
            TransitionData = transitionData;
            TransitionDirectionType = transitionDirectionType;
            TransitionType = transitionType;
            SceneTransitionDiff = sceneTransitionDiff;

            MainSceneManager = mainSceneManager;
            ModuleSceneManager = moduleSceneManager;
            SceneCameraManager = sceneCameraManager;
        }
    }
}
