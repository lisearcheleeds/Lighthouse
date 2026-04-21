using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene.SceneBase;
using Lighthouse.Scene.SceneCamera;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Implements IModuleSceneManager, tracking loaded module scenes in a list and applying Diff-based load/unload.
    /// Scene activation is deferred until progress reaches 0.9f, matching the same pattern as MainSceneManager.
    /// </summary>
    public class ModuleSceneManager : IModuleSceneManager
    {
        Func<IDisposable> enqueueParentLifetimeScope;

        readonly List<ModuleSceneBase> loadedSceneModules = new();

        void IModuleSceneManager.SetEnqueueParentLifetimeScope(Func<IDisposable> enqueueParentLifetimeScope)
        {
            this.enqueueParentLifetimeScope = enqueueParentLifetimeScope;
        }

        ISceneCamera[] IModuleSceneManager.GetSceneCameraList(ModuleSceneId[] requestSceneModuleIds)
        {
            return loadedSceneModules
                .Where(s => requestSceneModuleIds.Contains(s.ModuleSceneId))
                .SelectMany(x => x.GetSceneCameraList() ?? Array.Empty<ISceneCamera>())
                .ToArray();
        }

        void IModuleSceneManager.ResetAnimation(ISceneTransitionContext context)
        {
            var targetAnimations = loadedSceneModules
                .Where(x => context.SceneTransitionDiff.NextSceneGroup.SceneModuleIds.Contains(x.ModuleSceneId))
                .ToArray();

            foreach (var targetAnimation in targetAnimations)
            {
                targetAnimation.ResetInAnimation(context);
            }
        }

        async UniTask IModuleSceneManager.PlayInAnimation(ISceneTransitionContext context)
        {
            var target = loadedSceneModules
                .Where(x => context.SceneTransitionDiff.NextSceneGroup.SceneModuleIds.Contains(x.ModuleSceneId))
                .Select(x => x.PlayInAnimation(context))
                .ToArray();

            await UniTask.WhenAll(target);
        }

        async UniTask IModuleSceneManager.PlayOutAnimation(ISceneTransitionContext context)
        {
            if (context.SceneTransitionDiff.CurrentSceneGroup == null)
            {
                return;
            }

            var target = loadedSceneModules
                .Where(x => context.SceneTransitionDiff.CurrentSceneGroup.SceneModuleIds.Contains(x.ModuleSceneId))
                .Select(x => x.PlayOutAnimation(context))
                .ToArray();

            await UniTask.WhenAll(target);
        }

        async UniTask IModuleSceneManager.Load(ISceneTransitionContext context)
        {
            if (!context.SceneTransitionDiff.LoadSceneModuleIds.Any())
            {
                return;
            }

            using (enqueueParentLifetimeScope.Invoke())
            {
                // The scene loading progress is not linear, and the loading itself is completed in 0.9f.
                // Call OnLoad before the scene's Active state to initialize the display.
                var loadOperations = context.SceneTransitionDiff.LoadSceneModuleIds
                    .Select(sceneId => (sceneId: sceneId, ops: UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneId.Name, LoadSceneMode.Additive)))
                    .ToArray();

                await UniTask.WhenAll(loadOperations.Select(x => UniTask.WaitUntil(() => x.ops.progress >= 0.9f)));

                foreach (var loadOperation in loadOperations)
                {
                    loadOperation.ops.allowSceneActivation = true;
                }

                await UniTask.WhenAll(
                        loadOperations.Select(async x =>
                        {
                            await UniTask.WaitUntil(() =>
                            {
                                var s = UnityEngine.SceneManagement.SceneManager.GetSceneByName(x.sceneId.Name);
                                return s.IsValid() && s.isLoaded;
                            });

                            var scene = FindSceneBase(x.sceneId);
                            await scene.OnLoad();
                            await x.ops;
                            return scene;
                        }))
                    .ContinueWith(scenes =>
                    {
                        loadedSceneModules.AddRange(scenes);
                    });
            }
        }

        async UniTask IModuleSceneManager.Unload(ISceneTransitionContext context)
        {
            if (!context.SceneTransitionDiff.UnloadSceneModuleIds.Any())
            {
                return;
            }

            var unloadSceneModules = loadedSceneModules.Where(x => context.SceneTransitionDiff.UnloadSceneModuleIds.Contains(x.ModuleSceneId)).ToArray();
            await UniTask.WhenAll(unloadSceneModules.Select(s => s.OnUnload()));

            await UniTask.WhenAll(unloadSceneModules.Select(s => UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(s.ModuleSceneId.Name).ToUniTask()));

            foreach (var unloadSceneModule in unloadSceneModules)
            {
                loadedSceneModules.Remove(unloadSceneModule);
            }
        }

        async UniTask IModuleSceneManager.Enter(ISceneTransitionContext context, CancellationToken cancellationToken)
        {
            var target = loadedSceneModules
                .Where(x => context.SceneTransitionDiff.NextSceneGroup.SceneModuleIds.Contains(x.ModuleSceneId))
                .Select(x => x.Enter(context, cancellationToken))
                .ToArray();

            await UniTask.WhenAll(target);
        }

        async UniTask IModuleSceneManager.Leave(ISceneTransitionContext context, CancellationToken cancellationToken)
        {
            if (context.SceneTransitionDiff.CurrentSceneGroup == null)
            {
                return;
            }

            var target = loadedSceneModules
                .Where(x => context.SceneTransitionDiff.CurrentSceneGroup.SceneModuleIds.Contains(x.ModuleSceneId))
                .Select(x => x.Leave(context, cancellationToken))
                .ToArray();

            await UniTask.WhenAll(target);
        }

        void IModuleSceneManager.InitializeCanvas(ISceneTransitionContext context)
        {
            foreach (var sceneModule in loadedSceneModules)
            {
                if (context.SceneTransitionDiff.LoadSceneModuleIds.Contains(sceneModule.ModuleSceneId) && sceneModule is ICanvasSceneBase canvasSceneModule)
                {
                    canvasSceneModule.InitializeCanvas(context.SceneCameraManager.UICamera);
                }
            }
        }

        void IModuleSceneManager.OnSceneTransitionFinished(ISceneTransitionContext context)
        {
            foreach (var sceneModule in loadedSceneModules)
            {
                if (context.SceneTransitionDiff.NextSceneGroup.SceneModuleIds.Contains(sceneModule.ModuleSceneId))
                {
                    sceneModule.OnSceneTransitionFinished(context.SceneTransitionDiff);
                }
            }
        }

        UniTask IModuleSceneManager.PreReboot()
        {
            loadedSceneModules.Clear();
            return UniTask.CompletedTask;
        }

        ModuleSceneBase FindSceneBase(ModuleSceneId moduleSceneId)
        {
            try
            {
                return UnityEngine.SceneManagement.SceneManager
                    .GetSceneByName(moduleSceneId.Name)
                    .GetRootGameObjects()
                    .Select(x => x.GetComponent<ModuleSceneBase>())
                    .First(x => x != null);
            }
            catch (InvalidOperationException)
            {
                LHLogger.LogError($"[ModuleSceneManager] SceneModule NotFound {moduleSceneId}");
                throw;
            }
        }
    }
}
