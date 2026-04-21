using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene.SceneBase;
using Lighthouse.Scene.SceneCamera;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Implements IMainSceneManager, loading main scenes additively and tracking them in a dictionary keyed by MainSceneId.
    /// Scene activation is deferred until progress reaches 0.9f to allow pre-activation initialization via OnLoad.
    /// </summary>
    public class MainSceneManager : IMainSceneManager
    {
        readonly Dictionary<MainSceneId, MainSceneBase> loadedScenes = new();

        Func<IDisposable> enqueueParentLifetimeScope;

        void IMainSceneManager.SetEnqueueParentLifetimeScope(Func<IDisposable> enqueueParentLifetimeScope)
        {
            this.enqueueParentLifetimeScope = enqueueParentLifetimeScope;
        }

        ISceneCamera[] IMainSceneManager.GetSceneCameraList(SceneTransitionDiff sceneTransitionDiff)
        {
            if (loadedScenes.ContainsKey(sceneTransitionDiff.NextMainSceneId))
            {
                return loadedScenes[sceneTransitionDiff.NextMainSceneId].GetSceneCameraList();
            }

            return Array.Empty<ISceneCamera>();
        }

        void IMainSceneManager.InitializeCanvas(ISceneTransitionContext context)
        {
            if (!loadedScenes.TryGetValue(context.SceneTransitionDiff.NextMainSceneId, out var scene))
            {
                return;
            }

            if (scene is ICanvasSceneBase canvasSceneBase)
            {
                canvasSceneBase.InitializeCanvas(context.SceneCameraManager.UICamera);
            }
        }

        async UniTask IMainSceneManager.Enter(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            if (!loadedScenes.TryGetValue(context.SceneTransitionDiff.NextMainSceneId, out var scene))
            {
                return;
            }

            await scene.Enter(context, cancelToken);
        }

        async UniTask IMainSceneManager.Leave(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            if (context.SceneTransitionDiff.CurrentMainSceneId == null
                || !loadedScenes.TryGetValue(context.SceneTransitionDiff.CurrentMainSceneId, out var scene))
            {
                return;
            }

            await scene.Leave(context, cancelToken);
        }

        void IMainSceneManager.ResetInAnimation(ISceneTransitionContext context)
        {
            if (!loadedScenes.TryGetValue(context.SceneTransitionDiff.NextMainSceneId, out var scene))
            {
                return;
            }

            scene.ResetInAnimation(context);
        }

        async UniTask IMainSceneManager.PlayInAnimation(ISceneTransitionContext context)
        {
            if (!loadedScenes.TryGetValue(context.SceneTransitionDiff.NextMainSceneId, out var scene))
            {
                return;
            }

            await scene.PlayInAnimation(context);
        }

        async UniTask IMainSceneManager.PlayOutAnimation(ISceneTransitionContext context)
        {
            if (context.SceneTransitionDiff.CurrentMainSceneId == null
                || !loadedScenes.TryGetValue(context.SceneTransitionDiff.CurrentMainSceneId, out var scene))
            {
                return;
            }

            await scene.PlayOutAnimation(context);
        }

        async UniTask IMainSceneManager.SaveSceneState(ISceneTransitionContext context, CancellationToken cancelToken)
        {
            if (context.SceneTransitionDiff.CurrentMainSceneId == null
                || !loadedScenes.TryGetValue(context.SceneTransitionDiff.CurrentMainSceneId, out var scene))
            {
                return;
            }

            await scene.SaveSceneState(cancelToken);
        }

        async UniTask IMainSceneManager.Load(ISceneTransitionContext context)
        {
            if (context.SceneTransitionDiff.IsInnerGroupTransition)
            {
                return;
            }

            using (enqueueParentLifetimeScope.Invoke())
            {
                // The scene loading progress is not linear, and the loading itself is completed in 0.9f.
                // Call OnLoad before the scene's Active state to initialize the display.
                var loadOperations = context.SceneTransitionDiff.LoadMainSceneIds
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
                        foreach (var scene in scenes)
                        {
                            loadedScenes.Add(scene.MainSceneId, scene);
                        }
                    });
            }
        }

        async UniTask IMainSceneManager.Unload(ISceneTransitionContext context)
        {
            if (context.SceneTransitionDiff.IsInnerGroupTransition)
            {
                return;
            }

            var unloadTargetMainScenes = loadedScenes
                .Where(loadedScene => context.SceneTransitionDiff.UnloadMainSceneIds.Contains(loadedScene.Key))
                .ToArray();

            await UniTask.WhenAll(unloadTargetMainScenes.Select(s => s.Value.OnUnload()));

            await UniTask.WhenAll(unloadTargetMainScenes.Select(s => UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(s.Key.Name).ToUniTask()));

            foreach (var unloadTargetMainScene in unloadTargetMainScenes)
            {
                loadedScenes.Remove(unloadTargetMainScene.Key);
            }
        }

        void IMainSceneManager.OnSceneTransitionFinished(ISceneTransitionContext context)
        {
            if (!loadedScenes.TryGetValue(context.SceneTransitionDiff.NextMainSceneId, out var scene))
            {
                return;
            }

            scene.OnSceneTransitionFinished(context.SceneTransitionDiff);
        }

        async UniTask IMainSceneManager.PreReboot()
        {
            foreach (var loadedScene in loadedScenes)
            {
                if (loadedScene.Value.isActiveAndEnabled)
                {
                    await loadedScene.Value.SaveSceneState(CancellationToken.None);
                }
            }

            loadedScenes.Clear();
        }

        MainSceneBase FindSceneBase(MainSceneId mainSceneId)
        {
            try
            {
                return UnityEngine.SceneManagement.SceneManager
                    .GetSceneByName(mainSceneId.Name)
                    .GetRootGameObjects()
                    .Select(x => x.GetComponent<MainSceneBase>())
                    .First(x => x != null);
            }
            catch (InvalidOperationException)
            {
                LHLogger.LogError($"[SceneGroup] MainSceneBase NotFound\n" +
                               $"To add a scene, you need to add the scene to UnityEditor and place a GameObject that inherits MainSceneBase at the root of the added scene.");
                throw;
            }
        }
    }
}
