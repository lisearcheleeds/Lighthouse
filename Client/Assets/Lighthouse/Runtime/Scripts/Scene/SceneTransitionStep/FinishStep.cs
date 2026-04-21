using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// The final step of a transition. Notifies the main scene and module scenes
    /// that the transition has completed via OnSceneTransitionFinished.
    /// </summary>
    public sealed class FinishStep : ISceneTransitionStep
    {
        UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            context.MainSceneManager.OnSceneTransitionFinished(context);
            context.ModuleSceneManager.OnSceneTransitionFinished(context);
            return UniTask.CompletedTask;
        }
    }
}
