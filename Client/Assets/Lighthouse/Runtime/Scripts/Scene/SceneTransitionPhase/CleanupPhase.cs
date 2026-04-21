using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// Phase that performs cleanup after the transition completes. Cannot be intercepted.
    /// </summary>
    public sealed class CleanupPhase : ISceneTransitionPhase
    {
        ISceneTransitionStep[] ISceneTransitionPhase.Steps { get; } =
        {
            new CleanupStep(),
        };

        bool ISceneTransitionPhase.CanTransitionIntercept => false;
    }
}