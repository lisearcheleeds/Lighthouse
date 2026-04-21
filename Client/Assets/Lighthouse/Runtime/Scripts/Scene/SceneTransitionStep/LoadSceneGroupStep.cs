using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Step that loads the main scene and module scene group, then resets their in-animations.
    /// </summary>
    public sealed class LoadSceneGroupStep : ISceneTransitionStep
    {
        async UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            await context.MainSceneManager.Load(context);
            context.MainSceneManager.ResetInAnimation(context);

            await context.ModuleSceneManager.Load(context);
            context.ModuleSceneManager.ResetAnimation(context);
        }
    }
}
