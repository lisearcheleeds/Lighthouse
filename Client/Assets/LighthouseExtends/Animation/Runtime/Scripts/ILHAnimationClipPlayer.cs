using System;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.Animation
{
    /// <summary>
    /// Interface for an AnimationClip player. Supports synchronous and asynchronous playback
    /// as well as reset, stop, and skip operations.
    /// </summary>
    public interface ILHAnimationClipPlayer
    {
        public void PlayAnimation(bool isRestart, bool isEndEvaluate, Action onComplete);
        public UniTask PlayAnimationAsync(bool isRestart, bool isEndEvaluate);
        public void ResetAnimation();
        public void Stop();
        public void Skip();
    }
}