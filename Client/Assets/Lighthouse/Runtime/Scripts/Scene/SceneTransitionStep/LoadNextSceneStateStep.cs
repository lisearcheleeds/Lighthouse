using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Step that loads the next scene state via TransitionData.
    /// Retrieves the state according to the transition direction.
    /// </summary>
    public sealed class LoadNextSceneStateStep : ISceneTransitionStep
    {
        async UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            await context.TransitionData.LoadSceneState(context.TransitionDirectionType, cancelToken);
        }
    }
}
