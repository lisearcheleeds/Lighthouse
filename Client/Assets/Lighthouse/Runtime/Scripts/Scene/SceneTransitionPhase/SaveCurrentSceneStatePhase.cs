using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// Saves the current scene state before any redirect check occurs.
    /// CanTransitionIntercept is false to ensure save completes before LoadNextSceneStatePhase runs.
    /// </summary>
    public sealed class SaveCurrentSceneStatePhase : ISceneTransitionPhase
    {
        ISceneTransitionStep[] ISceneTransitionPhase.Steps { get; } =
        {
            new SaveCurrentSceneStateStep(),
        };

        bool ISceneTransitionPhase.CanTransitionIntercept => false;
    }
}
