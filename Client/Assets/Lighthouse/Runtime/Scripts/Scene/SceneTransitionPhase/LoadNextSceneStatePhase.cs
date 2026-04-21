using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// Loads the next scene state after the current state has been saved.
    /// CanTransitionIntercept is true, allowing LHSceneInterceptException to redirect the transition here.
    /// </summary>
    public sealed class LoadNextSceneStatePhase : ISceneTransitionPhase
    {
        ISceneTransitionStep[] ISceneTransitionPhase.Steps { get; } =
        {
            new LoadNextSceneStateStep(),
        };

        bool ISceneTransitionPhase.CanTransitionIntercept => true;
    }
}
