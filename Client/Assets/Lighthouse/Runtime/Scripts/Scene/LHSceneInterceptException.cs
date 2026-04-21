using System;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Throw this exception inside TransitionDataBase.LoadSceneState to redirect the transition to a different scene.
    /// Only valid during phases where CanTransitionIntercept is true (e.g. LoadNextSceneStatePhase).
    /// Throwing this in a non-interceptable phase is a developer error and will be reported as an InvalidOperationException.
    /// </summary>
    public sealed class LHSceneInterceptException : Exception
    {
        public TransitionDataBase RedirectTo { get; }

        public LHSceneInterceptException(TransitionDataBase redirectTo)
        {
            RedirectTo = redirectTo;
        }
    }
}
