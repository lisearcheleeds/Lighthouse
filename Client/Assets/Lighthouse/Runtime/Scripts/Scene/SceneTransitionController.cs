using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Input;
using Lighthouse.Scene.SceneCamera;
using Lighthouse.Scene.SceneTransitionPhase;
using UnityEngine;
using VContainer;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Implements ISceneTransitionController, iterating the phase sequence and running each phase's steps via WhenAll.
    /// Blocks input via IInputBlocker for the duration of the transition and enforces CanTransitionIntercept before starting.
    /// </summary>
    public sealed class SceneTransitionController : ISceneTransitionController
    {
        readonly IMainSceneManager mainSceneManager;
        readonly IModuleSceneManager moduleSceneManager;
        readonly ISceneCameraManager sceneCameraManager;
        readonly ISceneTransitionSequenceProvider sceneTransitionSequenceProvider;
        readonly ISceneTransitionContextFactory contextFactory;
        readonly IInputBlocker inputBlocker;

        public ISceneTransitionPhase CurrentTransitionPhase { get; private set; }

        [Inject]
        public SceneTransitionController(
            IMainSceneManager mainSceneManager,
            IModuleSceneManager moduleSceneManager,
            ISceneCameraManager sceneCameraManager,
            ISceneTransitionSequenceProvider sceneTransitionSequenceProvider,
            ISceneTransitionContextFactory contextFactory,
            IInputBlocker inputBlocker)
        {
            this.mainSceneManager = mainSceneManager;
            this.moduleSceneManager = moduleSceneManager;
            this.sceneCameraManager = sceneCameraManager;
            this.sceneTransitionSequenceProvider = sceneTransitionSequenceProvider;
            this.contextFactory = contextFactory;
            this.inputBlocker = inputBlocker;
        }

        async UniTask ISceneTransitionController.StartTransitionSequence(
            TransitionDataBase transitionData,
            SceneTransitionDiff sceneTransitionDiff,
            TransitionDirectionType transitionDirectionType,
            TransitionType transitionType,
            CancellationToken cancelToken)
        {
            if (!(CurrentTransitionPhase?.CanTransitionIntercept ?? true))
            {
                throw new InvalidOperationException($"[SceneTransitionWorker] Scene transition is not possible in the current phase. {CurrentTransitionPhase}");
            }

            CurrentTransitionPhase = null;

            var prevPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;

            if (transitionType == TransitionType.Auto)
            {
                if (sceneTransitionDiff.CurrentSceneGroup?.MainSceneIds.Contains(transitionData.MainSceneId) ?? false)
                {
                    transitionType = TransitionType.Cross;
                }
                else
                {
                    transitionType = TransitionType.Exclusive;
                }
            }

            var transitionSequence = transitionType switch
            {
                TransitionType.Cross => sceneTransitionSequenceProvider.CrossSequence,
                _ => sceneTransitionSequenceProvider.ExclusiveSequence,
            };

            var context = contextFactory.Create(transitionData, transitionDirectionType, transitionType, sceneTransitionDiff, mainSceneManager, moduleSceneManager, sceneCameraManager);

            inputBlocker.Block<SceneTransitionController>();

            try
            {
                foreach (var transitionPhase in transitionSequence)
                {
                    CurrentTransitionPhase = transitionPhase;

                    var tasks = transitionPhase.Steps.Select(step => step.Run(context, cancelToken));

                    await UniTask.WhenAll(tasks);
                }
            }
            catch (LHSceneInterceptException) when (CurrentTransitionPhase?.CanTransitionIntercept == true)
            {
                throw;
            }
            catch (LHSceneInterceptException e)
            {
                throw new InvalidOperationException(
                    $"[SceneTransitionController] LHSceneInterceptException was thrown in phase '{CurrentTransitionPhase?.GetType().Name}' " +
                    $"where CanTransitionIntercept is false. " +
                    $"LHSceneInterceptException must only be thrown during interceptable phases (e.g. LoadNextSceneStatePhase).", e);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                inputBlocker.UnBlock<SceneTransitionController>();

                Application.backgroundLoadingPriority = prevPriority;
                CurrentTransitionPhase = null;
            }
        }
    }
}
