using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene.SceneCamera;
using UnityEngine;

namespace Lighthouse.Scene.SceneBase
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(SceneCanvasInitializer))]
    /// <summary>
    /// ModuleSceneBase with Canvas integration. Alpha-controls the CanvasGroup based on activation state in the transition diff.
    /// Also handles animation hooks to correctly set alpha during in/out transitions.
    /// </summary>
    public abstract class CanvasModuleSceneBase : ModuleSceneBase, ICanvasSceneBase
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] SceneCanvasInitializer canvasInitializer;

        public override UniTask OnLoad()
        {
            canvasGroup.alpha = 0;
            return base.OnLoad();
        }

        protected override UniTask OnEnter(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            if (context.SceneTransitionDiff.ActivateSceneModuleIds.Contains(ModuleSceneId))
            {
                canvasGroup.alpha = 1;
            }

            return base.OnEnter(context, cancelToken);
        }

        protected override UniTask OnLeave(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            if (context.SceneTransitionDiff.DeactivateSceneModuleIds.Contains(ModuleSceneId))
            {
                canvasGroup.alpha = 0;
            }

            return base.OnLeave(context, cancelToken);
        }

        public virtual void InitializeCanvas(ISceneCamera canvasCamera)
        {
            canvasInitializer.Initialize(canvasCamera);
        }

        protected override void OnBeginInAnimation(ISceneTransitionContext context)
        {
            base.OnBeginInAnimation(context);

            var isActivateScene = context.SceneTransitionDiff.ActivateSceneModuleIds.Contains(ModuleSceneId);
            canvasGroup.alpha = isActivateScene ? 1.0f : canvasGroup.alpha;
        }

        protected override void OnCompleteOutAnimation(ISceneTransitionContext context)
        {
            base.OnCompleteOutAnimation(context);

            var isDeactivateScene = context.SceneTransitionDiff.DeactivateSceneModuleIds.Contains(ModuleSceneId);
            canvasGroup.alpha = isDeactivateScene ? 0.0f : canvasGroup.alpha;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            canvasInitializer ??= GetComponent<SceneCanvasInitializer>();
            canvasGroup ??= GetComponent<CanvasGroup>();
        }
#endif
    }
}
