using UnityEngine;

namespace LighthouseExtends.Animation
{
    /// <summary>
    /// Extends LHTransitionAnimator with ILHSceneTransitionAnimator.
    /// Inspector flags allow per-scene control over whether ResetInAnimation, InAnimation, and OutAnimation are invoked.
    /// </summary>
    public class LHSceneTransitionAnimator : LHTransitionAnimator, ILHSceneTransitionAnimator
    {
        [SerializeField] bool invokeResetInAnimation = true;
        [SerializeField] bool invokeInAnimation = true;
        [SerializeField] bool invokeOutAnimation = true;

        public bool InvokeResetInAnimation => invokeResetInAnimation;
        public bool InvokeInAnimation => invokeInAnimation;
        public bool InvokeOutAnimation => invokeOutAnimation;
    }
}