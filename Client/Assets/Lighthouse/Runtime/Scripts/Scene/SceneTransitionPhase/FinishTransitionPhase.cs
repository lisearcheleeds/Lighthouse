using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// The final phase of a transition. Executes finish processing to complete the transition sequence.
    /// Cannot be intercepted.
    /// </summary>
    public sealed class FinishTransitionPhase : ISceneTransitionPhase
    {
        ISceneTransitionStep[] ISceneTransitionPhase.Steps { get; } =
        {
            new FinishStep(),
        };

        bool ISceneTransitionPhase.CanTransitionIntercept => false;
    }
}