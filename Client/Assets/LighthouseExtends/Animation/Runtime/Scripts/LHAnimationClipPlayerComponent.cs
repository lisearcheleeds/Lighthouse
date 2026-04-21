using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LighthouseExtends.Animation
{
    /// <summary>
    /// MonoBehaviour wrapper for LHAnimationClipPlayer.
    /// Configures Animator and AnimationClips in the Inspector; the internal player is lazily initialized on first use.
    /// </summary>
    public class LHAnimationClipPlayerComponent : MonoBehaviour, ILHAnimationClipPlayer
    {
        [SerializeField] Animator animator;
        [SerializeField] AnimationClip[] animationClips;

        LHAnimationClipPlayer animationClipPlayer;

        public void PlayAnimation(bool isRestart, bool isEndEvaluate, Action onComplete)
        {
            InitializeIfNeeded();
            animationClipPlayer.PlayAnimation(isRestart, isEndEvaluate, onComplete);
        }

        public UniTask PlayAnimationAsync(bool isRestart, bool isEndEvaluate)
        {
            InitializeIfNeeded();
            return animationClipPlayer.PlayAnimationAsync(isRestart, isEndEvaluate);
        }

        public void ResetAnimation()
        {
            InitializeIfNeeded();
            animationClipPlayer.ResetAnimation();
        }

        public void Stop()
        {
            InitializeIfNeeded();
            animationClipPlayer.Stop();
        }

        public void Skip()
        {
            InitializeIfNeeded();
            animationClipPlayer.Skip();
        }

        void InitializeIfNeeded()
        {
            if (animationClipPlayer == null)
            {
                animationClipPlayer = new LHAnimationClipPlayer(animator, animationClips);
            }
        }

        void OnDestroy()
        {
            animationClipPlayer?.Dispose();
        }
    }
}