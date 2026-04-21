using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Step that updates the camera stack and injects the camera into the Canvases
    /// of both the main scene and module scenes.
    /// </summary>
    public sealed class ResolveCameraStep : ISceneTransitionStep
    {
        UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            context.SceneCameraManager.UpdateCameraStack(context.MainSceneManager, context.SceneTransitionDiff);

            context.MainSceneManager.InitializeCanvas(context);
            context.ModuleSceneManager.InitializeCanvas(context);

            return UniTask.CompletedTask;
        }
    }
}
