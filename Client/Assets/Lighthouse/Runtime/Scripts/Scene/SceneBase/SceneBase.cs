using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene.SceneCamera;
using UnityEngine;

namespace Lighthouse.Scene.SceneBase
{
    /// <summary>
    /// Abstract base MonoBehaviour that defines the scene lifecycle.
    /// Manages the Enter/Leave flow including OnSetup (called once on first enter),
    /// OnEnter, OnLeave, and in/out animation hooks.
    /// </summary>
    public abstract class SceneBase : MonoBehaviour
    {
        bool initialized;

        public virtual ISceneCamera[] GetSceneCameraList()
        {
            return Array.Empty<ISceneCamera>();
        }

        public virtual UniTask OnLoad()
        {
            // This method may be called immediately after the scene assets are loaded, before the Inject process runs.
            // If Injection is required, consider using OnSetup instead.
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnUnload()
        {
            return UniTask.CompletedTask;
        }

        public async UniTask Enter(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            if (!initialized)
            {
                initialized = true;
                await OnSetup();
            }

            await OnEnter(context, cancelToken);
        }

        public async UniTask Leave(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            await OnLeave(context, cancelToken);
        }

        public virtual void ResetInAnimation(ISceneTransitionContext context)
        {
        }

        public async UniTask PlayInAnimation(ISceneTransitionContext context)
        {
            // NOTE: InAnimation events are independent because they are expected to wait in WhenAll().
            OnBeginInAnimation(context);
            await InAnimation(context);
            OnCompleteInAnimation(context);
        }

        public async UniTask PlayOutAnimation(ISceneTransitionContext context)
        {
            // NOTE: OutAnimation events are independent because they are expected to wait in WhenAll().
            OnBeginOutAnimation(context);
            await OutAnimation(context);
            OnCompleteOutAnimation(context);
        }

        public virtual void OnSceneTransitionFinished(SceneTransitionDiff sceneTransitionDiff)
        {
        }

        protected virtual UniTask OnSetup()
        {
            // NOTE: If your setup requires TransitionData then that's not the right idea.
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnEnter(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnLeave(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnBeginInAnimation(ISceneTransitionContext context)
        {
        }

        protected virtual UniTask InAnimation(ISceneTransitionContext context)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnCompleteInAnimation(ISceneTransitionContext context)
        {
        }

        protected virtual void OnBeginOutAnimation(ISceneTransitionContext context)
        {
        }

        protected virtual UniTask OutAnimation(ISceneTransitionContext context)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnCompleteOutAnimation(ISceneTransitionContext context)
        {
        }
    }
}
