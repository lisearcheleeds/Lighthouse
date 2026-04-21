using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// Phase that executes the Leave process on the current scene. Cannot be intercepted.
    /// </summary>
    public sealed class LeaveScenePhase : ISceneTransitionPhase
    {
        ISceneTransitionStep[] ISceneTransitionPhase.Steps { get; } =
        {
            new LeaveSceneStep()
        };

        bool ISceneTransitionPhase.CanTransitionIntercept => false;
    }
}