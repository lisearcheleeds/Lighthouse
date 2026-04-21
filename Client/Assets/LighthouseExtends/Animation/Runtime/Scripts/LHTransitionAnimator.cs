using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LighthouseExtends.Animation
{
    /// <summary>
    /// MonoBehaviour that manages In and Out animations via separate LHAnimationClipPlayers.
    /// Supports per-direction start delay and stops the opposing player before each playback to ensure mutual exclusivity.
    /// </summary>
    public class LHTransitionAnimator : MonoBehaviour, ILHTransitionAnimator, IAnimationClipSource
    {
        [SerializeField] Animator animator;
        [SerializeField] AnimationClip[] inAnimationClips;
        [SerializeField] int inDelayMilliSec;
        [SerializeField] AnimationClip[] outAnimationClips;
        [SerializeField] int outDelayMilliSec;

        LHAnimationClipPlayer inAnimationClipPlayer;
        LHAnimationClipPlayer outAnimationClipPlayer;

        void InitializeIfNeeded()
        {
            if (inAnimationClipPlayer == null && inAnimationClips != null)
            {
                inAnimationClipPlayer = new LHAnimationClipPlayer(animator, inAnimationClips);
            }

            if (outAnimationClipPlayer == null && outAnimationClips != null)
            {
                outAnimationClipPlayer = new LHAnimationClipPlayer(animator, outAnimationClips);
            }
        }

        public void ResetInAnimation()
        {
            InitializeIfNeeded();

            if (inAnimationClipPlayer == null)
            {
                return;
            }

            if (outAnimationClipPlayer != null)
            {
                outAnimationClipPlayer.Stop();
            }

            inAnimationClipPlayer.ResetAnimation();
        }

        public async UniTask InAnimation()
        {
            InitializeIfNeeded();

            if (inAnimationClipPlayer == null)
            {
                return;
            }

            if (outAnimationClipPlayer != null)
            {
                outAnimationClipPlayer.Stop();
            }

            if (inDelayMilliSec != 0)
            {
                await UniTask.Delay(inDelayMilliSec);
            }

            await inAnimationClipPlayer.PlayAnimationAsync(true, true);
        }

        public void EndInAnimation()
        {
            InitializeIfNeeded();

            if (inAnimationClipPlayer == null)
            {
                return;
            }

            if (outAnimationClipPlayer != null)
            {
                outAnimationClipPlayer.Stop();
            }

            inAnimationClipPlayer.Skip();
        }

        public void ResetOutAnimation()
        {
            InitializeIfNeeded();

            if (outAnimationClipPlayer == null)
            {
                return;
            }

            if (inAnimationClipPlayer != null)
            {
                inAnimationClipPlayer.Stop();
            }

            outAnimationClipPlayer.ResetAnimation();
        }

        public async UniTask OutAnimation()
        {
            InitializeIfNeeded();

            if (outAnimationClipPlayer == null)
            {
                return;
            }

            if (inAnimationClipPlayer != null)
            {
                inAnimationClipPlayer.Stop();
            }

            if (outDelayMilliSec != 0)
            {
                await UniTask.Delay(outDelayMilliSec);
            }

            await outAnimationClipPlayer.PlayAnimationAsync(true, true);
        }

        public void EndOutAnimation()
        {
            InitializeIfNeeded();

            if (outAnimationClipPlayer == null)
            {
                return;
            }

            if (inAnimationClipPlayer != null)
            {
                inAnimationClipPlayer.Stop();
            }

            outAnimationClipPlayer.Skip();
        }

        void IAnimationClipSource.GetAnimationClips(List<AnimationClip> results)
        {
            if (inAnimationClips != null)
            {
                results.AddRange(inAnimationClips);
            }

            if (outAnimationClips != null)
            {
                results.AddRange(outAnimationClips);
            }
        }

        void OnDestroy()
        {
            inAnimationClipPlayer?.Dispose();
            outAnimationClipPlayer?.Dispose();
        }
    }
}
