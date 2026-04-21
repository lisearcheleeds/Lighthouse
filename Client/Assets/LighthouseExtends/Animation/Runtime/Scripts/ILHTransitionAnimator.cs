using Cysharp.Threading.Tasks;

namespace LighthouseExtends.Animation
{
    /// <summary>
    /// Interface for a transition animator that defines reset, play, and skip for In and Out animations.
    /// Implementations are responsible for ensuring In and Out playback are mutually exclusive.
    /// </summary>
    public interface ILHTransitionAnimator
    {
        void ResetInAnimation();
        UniTask InAnimation();
        void EndInAnimation();

        void ResetOutAnimation();
        UniTask OutAnimation();
        void EndOutAnimation();
    }
}