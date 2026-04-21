using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Abstract base class for scene transition data, defining the target MainSceneId and transition permission flags.
    /// Override LoadSceneState to pass runtime state into the next scene before it enters.
    /// </summary>
    public abstract class TransitionDataBase
    {
        public abstract MainSceneId MainSceneId { get; }

        public bool CanTransition { get; protected set; } = true;

        public bool CanBackTransition { get; protected set; } = true;

        public virtual UniTask LoadSceneState(TransitionDirectionType transitionDirectionType, CancellationToken cancelToken)
        {
            // If for any reason you want to cancel the scene transition and change the transition to a different screen,
            // throw an LHSceneInterceptException.
            return UniTask.CompletedTask;
        }
    }
}
