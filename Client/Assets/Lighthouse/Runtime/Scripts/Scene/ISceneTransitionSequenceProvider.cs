using Lighthouse.Scene.SceneTransitionPhase;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Provides the ordered phase sequences for each transition type.
    /// Replace this to customize the transition pipeline without modifying individual steps.
    /// </summary>
    public interface ISceneTransitionSequenceProvider
    {
        ISceneTransitionPhase[] ExclusiveSequence { get; }
        ISceneTransitionPhase[] CrossSequence { get; }
    }
}