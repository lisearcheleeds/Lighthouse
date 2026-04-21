using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Step that plays the out animations of the main scene and module scenes in parallel via WhenAll.
    /// </summary>
    public sealed class OutAnimationStep : ISceneTransitionStep
    {
        async UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            await UniTask.WhenAll(
                context.MainSceneManager.PlayOutAnimation(context),
                context.ModuleSceneManager.PlayOutAnimation(context));
        }
    }
}
