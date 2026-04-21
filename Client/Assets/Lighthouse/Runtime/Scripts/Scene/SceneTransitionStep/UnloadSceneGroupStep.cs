using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Step that sequentially unloads the main scene and module scene group.
    /// </summary>
    public sealed class UnloadSceneGroupStep : ISceneTransitionStep
    {
        async UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            await context.MainSceneManager.Unload(context);
            await context.ModuleSceneManager.Unload(context);
        }
    }
}
