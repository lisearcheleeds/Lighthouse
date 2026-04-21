using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Input;
using Lighthouse.Scene;
using Lighthouse.Scene.SceneCamera;
using Lighthouse.Scene.SceneTransitionPhase;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Lighthouse.Tests.EditMode
{
    public class SceneTransitionControllerTest
    {
        static readonly MainSceneId IdA = new MainSceneId(1, "A");
        static readonly MainSceneId IdB = new MainSceneId(2, "B");

        static readonly SceneGroup GroupContainingBoth = new SceneGroup(
            new System.Collections.Generic.Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { IdA, Array.Empty<ModuleSceneId>() },
                { IdB, Array.Empty<ModuleSceneId>() },
            });

        static readonly SceneGroup GroupA = new SceneGroup(
            new System.Collections.Generic.Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { IdA, Array.Empty<ModuleSceneId>() },
            });

        static readonly SceneGroup GroupB = new SceneGroup(
            new System.Collections.Generic.Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { IdB, Array.Empty<ModuleSceneId>() },
            });

        CapturingContextFactory contextFactory;
        SpyInputBlocker inputBlocker;
        ISceneTransitionController controller;

        [SetUp]
        public void SetUp()
        {
            contextFactory = new CapturingContextFactory();
            inputBlocker = new SpyInputBlocker();
            controller = new SceneTransitionController(
                new StubMainSceneManager(),
                new StubModuleSceneManager(),
                new StubSceneCameraManager(),
                new EmptySequenceProvider(),
                contextFactory,
                inputBlocker);
        }

        [UnityTest]
        public IEnumerator Auto_NextSceneInCurrentGroup_ResolvesToCross() => UniTask.ToCoroutine(async () =>
        {
            var diff = new SceneTransitionDiff(GroupContainingBoth, IdA, GroupContainingBoth, IdB);

            await controller.StartTransitionSequence(
                new TestTransitionData(IdB), diff,
                TransitionDirectionType.Forward, TransitionType.Auto, CancellationToken.None);

            Assert.AreEqual(TransitionType.Cross, contextFactory.CapturedType);
        });

        [UnityTest]
        public IEnumerator Auto_NextSceneNotInCurrentGroup_ResolvesToExclusive() => UniTask.ToCoroutine(async () =>
        {
            var diff = new SceneTransitionDiff(GroupA, IdA, GroupB, IdB);

            await controller.StartTransitionSequence(
                new TestTransitionData(IdB), diff,
                TransitionDirectionType.Forward, TransitionType.Auto, CancellationToken.None);

            Assert.AreEqual(TransitionType.Exclusive, contextFactory.CapturedType);
        });

        [UnityTest]
        public IEnumerator Auto_NullCurrentGroup_ResolvesToExclusive() => UniTask.ToCoroutine(async () =>
        {
            var diff = new SceneTransitionDiff(null, null, GroupB, IdB);

            await controller.StartTransitionSequence(
                new TestTransitionData(IdB), diff,
                TransitionDirectionType.Forward, TransitionType.Auto, CancellationToken.None);

            Assert.AreEqual(TransitionType.Exclusive, contextFactory.CapturedType);
        });

        [UnityTest]
        public IEnumerator ExplicitCross_UsesCrossEvenWhenGroupsDiffer() => UniTask.ToCoroutine(async () =>
        {
            var diff = new SceneTransitionDiff(GroupA, IdA, GroupB, IdB);

            await controller.StartTransitionSequence(
                new TestTransitionData(IdB), diff,
                TransitionDirectionType.Forward, TransitionType.Cross, CancellationToken.None);

            Assert.AreEqual(TransitionType.Cross, contextFactory.CapturedType);
        });

        [UnityTest]
        public IEnumerator ExplicitExclusive_UsesExclusiveEvenWhenSameGroup() => UniTask.ToCoroutine(async () =>
        {
            var diff = new SceneTransitionDiff(GroupContainingBoth, IdA, GroupContainingBoth, IdB);

            await controller.StartTransitionSequence(
                new TestTransitionData(IdB), diff,
                TransitionDirectionType.Forward, TransitionType.Exclusive, CancellationToken.None);

            Assert.AreEqual(TransitionType.Exclusive, contextFactory.CapturedType);
        });

        [UnityTest]
        public IEnumerator InputIsBlockedDuringTransition_UnblockedAfterCompletion() => UniTask.ToCoroutine(async () =>
        {
            var diff = new SceneTransitionDiff(null, null, GroupB, IdB);

            await controller.StartTransitionSequence(
                new TestTransitionData(IdB), diff,
                TransitionDirectionType.Forward, TransitionType.Exclusive, CancellationToken.None);

            Assert.IsTrue(inputBlocker.WasBlocked, "Block should have been called");
            Assert.IsTrue(inputBlocker.WasUnblocked, "UnBlock should have been called");
            Assert.AreEqual(0, inputBlocker.CurrentBlockCount, "Block and UnBlock calls should be balanced");
        });

        // ---- stubs ----

        class CapturingContextFactory : ISceneTransitionContextFactory
        {
            public TransitionType CapturedType { get; private set; }

            public ISceneTransitionContext Create(
                TransitionDataBase transitionData,
                TransitionDirectionType transitionDirectionType,
                TransitionType transitionType,
                SceneTransitionDiff sceneTransitionDiff,
                IMainSceneManager mainSceneManager,
                IModuleSceneManager moduleSceneManager,
                ISceneCameraManager sceneCameraManager)
            {
                CapturedType = transitionType;
                return new SceneTransitionContext(
                    transitionData, transitionDirectionType, transitionType,
                    sceneTransitionDiff, mainSceneManager, moduleSceneManager, sceneCameraManager);
            }
        }

        class SpyInputBlocker : IInputBlocker
        {
            public bool WasBlocked { get; private set; }
            public bool WasUnblocked { get; private set; }
            public int CurrentBlockCount { get; private set; }

            public IDisposable Block<T>(bool isSystemLayer = false)
            {
                WasBlocked = true;
                CurrentBlockCount++;
                return new NoopDisposable();
            }

            public void UnBlock<T>(bool isSystemLayer = false)
            {
                WasUnblocked = true;
                CurrentBlockCount--;
            }

            class NoopDisposable : IDisposable
            {
                public void Dispose() { }
            }
        }

        class EmptySequenceProvider : ISceneTransitionSequenceProvider
        {
            static readonly ISceneTransitionPhase[] Empty = Array.Empty<ISceneTransitionPhase>();
            ISceneTransitionPhase[] ISceneTransitionSequenceProvider.CrossSequence => Empty;
            ISceneTransitionPhase[] ISceneTransitionSequenceProvider.ExclusiveSequence => Empty;
        }

        class StubSceneCameraManager : ISceneCameraManager
        {
            public ISceneCamera BaseCamera => null;
            public ISceneCamera UICamera => null;
            public ISceneCamera[] OverlayCameraList => Array.Empty<ISceneCamera>();
            public void UpdateCameraStack(IMainSceneManager m, SceneTransitionDiff d) { }
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
            public TestTransitionData(MainSceneId id) { MainSceneId = id; }
        }
    }
}
