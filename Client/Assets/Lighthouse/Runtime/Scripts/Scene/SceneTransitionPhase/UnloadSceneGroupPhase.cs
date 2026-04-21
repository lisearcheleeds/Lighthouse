using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// Phase that unloads the previous scene group. Cannot be intercepted.
    /// </summary>
    public sealed class UnloadSceneGroupPhase : ISceneTransitionPhase
    {
        ISceneTransitionStep[] ISceneTransitionPhase.Steps { get; } =
        {
            new UnloadSceneGroupStep(),
        };

        bool ISceneTransitionPhase.CanTransitionIntercept => false;
    }
}