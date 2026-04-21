using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// Phase that loads the next scene group. Cannot be intercepted.
    /// </summary>
    public sealed class LoadSceneGroupPhase : ISceneTransitionPhase
    {
        ISceneTransitionStep[] ISceneTransitionPhase.Steps { get; } =
        {
            new LoadSceneGroupStep(),
        };

        bool ISceneTransitionPhase.CanTransitionIntercept => false;
    }
}