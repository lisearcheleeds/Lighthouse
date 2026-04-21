using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene;
using Lighthouse.Scene.SceneCamera;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Lighthouse.Tests.EditMode
{
    public class SceneManagerTest
    {
        static readonly MainSceneId IdA = new MainSceneId(1, "A");
        static readonly MainSceneId IdB = new MainSceneId(2, "B");
        static readonly MainSceneId IdC = new MainSceneId(3, "C");

        static readonly SceneGroup GroupA = new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
        {
            { IdA, Array.Empty<ModuleSceneId>() }
        });
        static readonly SceneGroup GroupB = new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
        {
            { IdB, Array.Empty<ModuleSceneId>() }
        });
        static readonly SceneGroup GroupC = new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
        {
            { IdC, Array.Empty<ModuleSceneId>() }
        });

        static ISceneManager CreateManager(ISceneTransitionController controller)
        {
            var provider = new StubSceneGroupProvider(new Dictionary<MainSceneId, SceneGroup>
            {
                { IdA, GroupA },
                { IdB, GroupB },
                { IdC, GroupC }
            });
            return (ISceneManager)new SceneManager(controller, new StubMainSceneManager(), new StubModuleSceneManager(), provider);
        }

        [UnityTest]
        public IEnumerator BackScene_EmptyStack_DoesNothing() => UniTask.ToCoroutine(async () =>
        {
            var controller = new StubSceneTransitionController();
            var manager = CreateManager(controller);

            await manager.BackScene(TransitionType.Auto);

            Assert.AreEqual(0, controller.CallCount);
        });

        [UnityTest]
        public IEnumerator BackScene_SingleEntry_DoesNothing() => UniTask.ToCoroutine(async () =>
        {
            var controller = new StubSceneTransitionController();
            var manager = CreateManager(controller);

            await manager.TransitionScene(new TestTransitionData(IdA));
            controller.Reset();

            await manager.BackScene(TransitionType.Auto);

            Assert.AreEqual(0, controller.CallCount);
        });

        [UnityTest]
        public IEnumerator BackScene_TwoEntries_TransitionsWithBackDirection() => UniTask.ToCoroutine(async () =>
        {
            var controller = new StubSceneTransitionController();
            var manager = CreateManager(controller);

            await manager.TransitionScene(new TestTransitionData(IdA));
            await manager.TransitionScene(new TestTransitionData(IdB));
            controller.Reset();

            await manager.BackScene(TransitionType.Auto);

            Assert.AreEqual(1, controller.CallCount);
            Assert.AreEqual(TransitionDirectionType.Back, controller.LastDirection);
        });

        [UnityTest]
        public IEnumerator BackScene_SkipsNonBackableEntry_FindsValidTarget() => UniTask.ToCoroutine(async () =>
        {
            var controller = new StubSceneTransitionController();
            var manager = CreateManager(controller);

            await manager.TransitionScene(new TestTransitionData(IdA));
            await manager.TransitionScene(new TestTransitionData(IdB, canBackTransition: false));
            await manager.TransitionScene(new TestTransitionData(IdC));
            controller.Reset();

            await manager.BackScene(TransitionType.Auto);

            Assert.AreEqual(1, controller.CallCount);
            Assert.AreEqual(TransitionDirectionType.Back, controller.LastDirection);
            Assert.AreEqual(IdA, controller.LastNextTransitionData.MainSceneId);
        });

        [UnityTest]
        public IEnumerator PreReboot_ClearsStack_SubsequentBackDoesNothing() => UniTask.ToCoroutine(async () =>
        {
            var controller = new StubSceneTransitionController();
            var manager = CreateManager(controller);

            await manager.TransitionScene(new TestTransitionData(IdA));
            await manager.TransitionScene(new TestTransitionData(IdB));
            await manager.PreReboot();
            controller.Reset();

            await manager.BackScene(TransitionType.Auto);

            Assert.AreEqual(0, controller.CallCount);
        });

        [UnityTest]
        public IEnumerator TransitionScene_WhileAlreadyTransitioning_IsIgnored() => UniTask.ToCoroutine(async () =>
        {
            var controller = new HoldingStubController();
            var manager = CreateManager(controller);

            var firstTransition = manager.TransitionScene(new TestTransitionData(IdA));

            // IsTransition = true here; second call should return immediately
            await manager.TransitionScene(new TestTransitionData(IdB));

            controller.Complete();
            await firstTransition;

            Assert.AreEqual(1, controller.CallCount);
        });

        [UnityTest]
        public IEnumerator TransitionScene_CanTransitionFalse_DoesNotCallController() => UniTask.ToCoroutine(async () =>
        {
            var controller = new StubSceneTransitionController();
            var manager = CreateManager(controller);

            await manager.TransitionScene(new TestTransitionData(IdA, canTransition: false));

            Assert.AreEqual(0, controller.CallCount);
        });

        [UnityTest]
        public IEnumerator TransitionScene_LHSceneInterceptException_RedirectsToNewTarget() => UniTask.ToCoroutine(async () =>
        {
            var redirectTarget = new TestTransitionData(IdC);
            var controller = new ThrowOnceStubController(new LHSceneInterceptException(redirectTarget));
            var manager = CreateManager(controller);

            await manager.TransitionScene(new TestTransitionData(IdA));

            // First call threw; second call is the redirect to IdC
            Assert.AreEqual(2, controller.CallCount);
            Assert.AreEqual(IdC, controller.LastNextTransitionData.MainSceneId);
        });

        // ---- stubs ----

        class StubSceneTransitionController : ISceneTransitionController
        {
            public int CallCount { get; private set; }
            public TransitionDirectionType LastDirection { get; private set; }
            public TransitionDataBase LastNextTransitionData { get; private set; }

            public void Reset()
            {
                CallCount = 0;
            }

            public UniTask StartTransitionSequence(
                TransitionDataBase transitionData,
                SceneTransitionDiff sceneTransitionDiff,
                TransitionDirectionType transitionDirectionType,
                TransitionType transitionType,
                CancellationToken cancelToken)
            {
                CallCount++;
                LastDirection = transitionDirectionType;
                LastNextTransitionData = transitionData;
                return UniTask.CompletedTask;
            }
        }

        class StubSceneGroupProvider : ISceneGroupProvider
        {
            readonly Dictionary<MainSceneId, SceneGroup> map;

            public StubSceneGroupProvider(Dictionary<MainSceneId, SceneGroup> map)
            {
                this.map = map;
            }

            public SceneGroup GetSceneGroup(MainSceneId id) => map[id];
        }

        class StubMainSceneManager : IMainSceneManager
        {
            public void SetEnqueueParentLifetimeScope(Func<IDisposable> e) { }
            public UniTask Load(ISceneTransitionContext c) => UniTask.CompletedTask;
            public UniTask Unload(ISceneTransitionContext c) => UniTask.CompletedTask;
            public UniTask Enter(ISceneTransitionContext c, CancellationToken ct) => UniTask.CompletedTask;
            public UniTask Leave(ISceneTransitionContext c, CancellationToken ct) => UniTask.CompletedTask;
            public void ResetInAnimation(ISceneTransitionContext c) { }
            public UniTask PlayInAnimation(ISceneTransitionContext c) => UniTask.CompletedTask;
            public UniTask PlayOutAnimation(ISceneTransitionContext c) => UniTask.CompletedTask;
            public UniTask SaveSceneState(ISceneTransitionContext c, CancellationToken ct) => UniTask.CompletedTask;
            public ISceneCamera[] GetSceneCameraList(SceneTransitionDiff d) => Array.Empty<ISceneCamera>();
            public void InitializeCanvas(ISceneTransitionContext c) { }
            public void OnSceneTransitionFinished(ISceneTransitionContext c) { }
            public UniTask PreReboot() => UniTask.CompletedTask;
        }

        class StubModuleSceneManager : IModuleSceneManager
        {
            public void SetEnqueueParentLifetimeScope(Func<IDisposable> e) { }
            public UniTask Load(ISceneTransitionContext c) => UniTask.CompletedTask;
            public UniTask Unload(ISceneTransitionContext c) => UniTask.CompletedTask;
            public UniTask Enter(ISceneTransitionContext c, CancellationToken ct) => UniTask.CompletedTask;
            public UniTask Leave(ISceneTransitionContext c, CancellationToken ct) => UniTask.CompletedTask;
            public void ResetAnimation(ISceneTransitionContext c) { }
            public UniTask PlayInAnimation(ISceneTransitionContext c) => UniTask.CompletedTask;
            public UniTask PlayOutAnimation(ISceneTransitionContext c) => UniTask.CompletedTask;
            public ISceneCamera[] GetSceneCameraList(ModuleSceneId[] ids) => Array.Empty<ISceneCamera>();
            public void InitializeCanvas(ISceneTransitionContext c) { }
            public void OnSceneTransitionFinished(ISceneTransitionContext c) { }
            public UniTask PreReboot() => UniTask.CompletedTask;
        }

        class TestTransitionData : TransitionDataBase
        {
            public override MainSceneId MainSceneId { get; }

            public TestTransitionData(MainSceneId id, bool canTransition = true, bool canBackTransition = true)
            {
                MainSceneId = id;
                CanTransition = canTransition;
                CanBackTransition = canBackTransition;
            }
        }

        class HoldingStubController : ISceneTransitionController
        {
            readonly UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            public int CallCount { get; private set; }

            public void Complete() => tcs.TrySetResult();

            public async UniTask StartTransitionSequence(
                TransitionDataBase transitionData,
                SceneTransitionDiff sceneTransitionDiff,
                TransitionDirectionType transitionDirectionType,
                TransitionType transitionType,
                CancellationToken cancelToken)
            {
                CallCount++;
                await tcs.Task;
            }
        }

        class ThrowOnceStubController : ISceneTransitionController
        {
            LHSceneInterceptException toThrow;
            public int CallCount { get; private set; }
            public TransitionDataBase LastNextTransitionData { get; private set; }

            public ThrowOnceStubController(LHSceneInterceptException toThrow)
            {
                this.toThrow = toThrow;
            }

            public UniTask StartTransitionSequence(
                TransitionDataBase transitionData,
                SceneTransitionDiff sceneTransitionDiff,
                TransitionDirectionType transitionDirectionType,
                TransitionType transitionType,
                CancellationToken cancelToken)
            {
                CallCount++;
                LastNextTransitionData = transitionData;

                if (toThrow != null)
                {
                    var ex = toThrow;
                    toThrow = null;
                    throw ex;
                }

                return UniTask.CompletedTask;
            }
        }
    }
}
