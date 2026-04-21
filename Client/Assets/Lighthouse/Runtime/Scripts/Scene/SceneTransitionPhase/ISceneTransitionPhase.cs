using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// Represents a single phase in a scene transition.
    /// Defines the Steps to execute and whether an incoming transition is allowed to intercept during this phase.
    /// </summary>
    public interface ISceneTransitionPhase
    {
        ISceneTransitionStep[] Steps { get; }

        bool CanTransitionIntercept { get; }
    }
}