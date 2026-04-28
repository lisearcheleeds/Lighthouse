using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene;
using Lighthouse.Scene.SceneBase;
using VContainer;

namespace LighthouseExtends.ScreenStack
{
    public abstract class ScreenStackModuleSceneBase : CanvasModuleSceneBase
    {
        IScreenStackManager screenStackManager;

        [Inject]
        public void Construct(IScreenStackManager screenStackManager)
        {
            this.screenStackManager = screenStackManager;
        }

        protected override UniTask OnSetup()
        {
            screenStackManager.Setup();
            return UniTask.CompletedTask;
        }

        protected override UniTask OnEnter(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            if (context.TransitionDirectionType == TransitionDirectionType.Back && context.TransitionType == TransitionType.Exclusive)
            {
                return UniTask.WhenAll(
                    screenStackManager.ResumeFromSceneId(context.SceneTransitionDiff.NextMainSceneId, false),
                    base.OnEnter(context, cancelToken));
            }

            return base.OnEnter(context, cancelToken);
        }

        protected override UniTask OnLeave(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            if (context.TransitionDirectionType == TransitionDirectionType.Forward && context.TransitionType == TransitionType.Exclusive)
            {
                return UniTask.WhenAll(
                    screenStackManager.SuspendFromSceneId(context.SceneTransitionDiff.CurrentMainSceneId),
                    base.OnLeave(context, cancelToken));
            }

            return base.OnLeave(context, cancelToken);
        }

        protected override UniTask InAnimation(ISceneTransitionContext context)
        {
            if (context.TransitionDirectionType == TransitionDirectionType.Back && context.TransitionType == TransitionType.Cross)
            {
                return screenStackManager.ResumeFromSceneId(context.SceneTransitionDiff.NextMainSceneId, true);
            }

            return UniTask.CompletedTask;
        }

        protected override UniTask OutAnimation(ISceneTransitionContext context)
        {
            if (context.TransitionDirectionType == TransitionDirectionType.Forward && context.TransitionType == TransitionType.Cross)
            {
                return screenStackManager.SuspendFromSceneId(context.SceneTransitionDiff.CurrentMainSceneId);
            }

            return UniTask.CompletedTask;
        }
    }
}
