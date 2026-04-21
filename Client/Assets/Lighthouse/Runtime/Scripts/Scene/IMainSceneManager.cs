using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene.SceneCamera;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Manages the lifecycle of main scenes: loading, unloading, enter/leave, animations, and canvas initialization.
    /// Called by transition steps via ISceneTransitionContext; not intended for direct use from game code.
    /// </summary>
    public interface IMainSceneManager
    {
        void SetEnqueueParentLifetimeScope(Func<IDisposable> enqueueParentLifetimeScope);

        UniTask Load(ISceneTransitionContext context);
        UniTask Unload(ISceneTransitionContext context);

        UniTask Enter(ISceneTransitionContext context, CancellationToken cancelToken);
        UniTask Leave(ISceneTransitionContext context, CancellationToken cancelToken);

        void ResetInAnimation(ISceneTransitionContext context);
        UniTask PlayInAnimation(ISceneTransitionContext context);
        UniTask PlayOutAnimation(ISceneTransitionContext context);

        UniTask SaveSceneState(ISceneTransitionContext context, CancellationToken cancelToken);

        ISceneCamera[] GetSceneCameraList(SceneTransitionDiff sceneTransitionDiff);
        void InitializeCanvas(ISceneTransitionContext context);

        void OnSceneTransitionFinished(ISceneTransitionContext context);

        UniTask PreReboot();
    }
}
