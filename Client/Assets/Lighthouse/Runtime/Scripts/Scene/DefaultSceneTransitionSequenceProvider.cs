using Lighthouse.Scene.SceneTransitionPhase;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Default implementation of ISceneTransitionSequenceProvider.
    /// Returns the standard Exclusive and Cross phase sequences used by the built-in transition pipeline.
    /// </summary>
    public sealed class DefaultSceneTransitionSequenceProvider : ISceneTransitionSequenceProvider
    {
        ISceneTransitionPhase[] ISceneTransitionSequenceProvider.CrossSequence { get; } =
        {
            new SaveCurrentSceneStatePhase(),
            new LoadNextSceneStatePhase(),
            new LoadSceneGroupPhase(),
            new EnterScenePhase(),
            new CrossAnimationPhase(),
            new LeaveScenePhase(),
            new UnloadSceneGroupPhase(),
            new FinishTransitionPhase(),
        };

        ISceneTransitionPhase[] ISceneTransitionSequenceProvider.ExclusiveSequence { get; } =
        {
            new SaveCurrentSceneStatePhase(),
            new LoadNextSceneStatePhase(),
            new OutAnimationPhase(),
            new LeaveScenePhase(),
            new LoadSceneGroupPhase(),
            new UnloadSceneGroupPhase(),
            new EnterScenePhase(),
            new InAnimationPhase(),
            new FinishTransitionPhase(),
            new CleanupPhase(),
        };
    }
}