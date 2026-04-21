namespace LighthouseExtends.Animation
{
    /// <summary>
    /// Extends ILHTransitionAnimator with per-scene flags that control whether
    /// ResetInAnimation, InAnimation, and OutAnimation are invoked during a scene transition.
    /// </summary>
    public interface ILHSceneTransitionAnimator : ILHTransitionAnimator
    {
        bool InvokeResetInAnimation { get; }
        bool InvokeInAnimation { get; }
        bool InvokeOutAnimation { get; }
    }
}