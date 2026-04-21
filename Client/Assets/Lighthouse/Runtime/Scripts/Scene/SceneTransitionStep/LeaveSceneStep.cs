using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Step that sequentially executes the Leave process on the main scene and module scenes.
    /// </summary>
    public sealed class LeaveSceneStep : ISceneTransitionStep
    {
        async UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            await context.MainSceneManager.Leave(context, cancelToken);
            await context.ModuleSceneManager.Leave(context, cancelToken);
        }
    }
}
