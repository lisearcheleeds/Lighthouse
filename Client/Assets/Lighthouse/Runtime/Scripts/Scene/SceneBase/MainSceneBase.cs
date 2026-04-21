using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneBase
{
    /// <summary>
    /// Abstract base for main scenes. Must be placed on a root GameObject in the Unity scene
    /// so MainSceneManager can locate it after additive load.
    /// </summary>
    public abstract class MainSceneBase : SceneBase
    {
        public abstract MainSceneId MainSceneId { get; }

        /// <summary>
        /// Called before leaving this scene. Override to persist domain state or communicate with a server.
        /// To redirect the transition to a different scene, throw an LHSceneInterceptException.
        /// </summary>
        public virtual UniTask SaveSceneState(CancellationToken cancelToken)
        {
            return UniTask.CompletedTask;
        }
    }

    /// <summary>
    /// Typed variant of MainSceneBase that extracts TTransitionData from the transition context.
    /// Activates the GameObject on enter and deactivates it on leave.
    /// </summary>
    public abstract class MainSceneBase<TTransitionData> : MainSceneBase where TTransitionData : TransitionDataBase
    {
        protected TTransitionData TransitionData { get; private set; }

        protected override UniTask OnEnter(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            // If necessary, you can override OnEnter to control the gameObject.
            gameObject.SetActive(true);

            TransitionData = (TTransitionData)context.TransitionData;

            return OnEnter(TransitionData, context, cancelToken);
        }

        protected virtual UniTask OnEnter(TTransitionData transitionData, ISceneTransitionContext context, CancellationToken cancelToken)
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask OnLeave(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            gameObject.SetActive(false);
            return base.OnLeave(context, cancelToken);
        }
    }
}
