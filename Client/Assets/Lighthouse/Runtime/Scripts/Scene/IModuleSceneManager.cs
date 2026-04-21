using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene.SceneCamera;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Manages the lifecycle of module scenes: loading, unloading, enter/leave, animations, and canvas initialization.
    /// Called by transition steps via ISceneTransitionContext; not intended for direct use from game code.
    /// </summary>
    public interface IModuleSceneManager
    {
        void SetEnqueueParentLifetimeScope(Func<IDisposable> enqueueParentLifetimeScope);

        public UniTask Load(ISceneTransitionContext context);
        public UniTask Unload(ISceneTransitionContext context);

        public UniTask Enter(ISceneTransitionContext context, CancellationToken cancellationToken);
        public UniTask Leave(ISceneTransitionContext context, CancellationToken cancellationToken);

        public void ResetAnimation(ISceneTransitionContext context);
        public UniTask PlayInAnimation(ISceneTransitionContext context);
        public UniTask PlayOutAnimation(ISceneTransitionContext context);

        public ISceneCamera[] GetSceneCameraList(ModuleSceneId[] requestSceneModuleIds);
        public void InitializeCanvas(ISceneTransitionContext context);

        public void OnSceneTransitionFinished(ISceneTransitionContext context);

        UniTask PreReboot();
    }
}
