using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Step that sequentially executes the Enter process on the main scene and module scenes,
    /// then resets their in-animations.
    /// </summary>
    public sealed class EnterSceneStep : ISceneTransitionStep
    {
        async UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            await context.MainSceneManager.Enter(context, cancelToken);
            context.MainSceneManager.ResetInAnimation(context);

            await context.ModuleSceneManager.Enter(context, cancelToken);
            context.ModuleSceneManager.ResetAnimation(context);
        }
    }
}
