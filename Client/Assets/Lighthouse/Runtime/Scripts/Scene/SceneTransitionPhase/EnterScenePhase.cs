using Lighthouse.Scene.SceneTransitionStep;

namespace Lighthouse.Scene.SceneTransitionPhase
{
    /// <summary>
    /// Phase that executes the Enter process on the next scene and resolves the camera setup.
    /// Cannot be intercepted.
    /// </summary>
    public sealed class EnterScenePhase : ISceneTransitionPhase
    {
        ISceneTransitionStep[] ISceneTransitionPhase.Steps { get; } =
        {
            new EnterSceneStep(), new ResolveCameraStep(),
        };

        bool ISceneTransitionPhase.CanTransitionIntercept => false;
    }
}