using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Saves the current scene's state before leaving.
    /// Override SaveSceneState in the scene to persist domain data or trigger server communication as needed.
    /// </summary>
    public sealed class SaveCurrentSceneStateStep : ISceneTransitionStep
    {
        async UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            await context.MainSceneManager.SaveSceneState(context, cancelToken);
        }
    }
}
