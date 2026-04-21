using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Input;
using Lighthouse.Scene;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LighthouseExtends.ScreenStack.Tests.EditMode
{
    public class ScreenStackManagerTest
    {
        static readonly MainSceneId SceneA = new MainSceneId(1, "A");
        static readonly MainSceneId SceneB = new MainSceneId(2, "B");

        IScreenStackManager CreateManager(out StubScreenStackEntityFactory factory)
        {
            factory = new StubScreenStackEntityFactory();
            return (IScreenStackManager)new ScreenStackManager(
                new StubScreenStackCanvasController(),
                factory,
                new StubScreenStackBackgroundInputBlocker(),
                new StubInputBlocker());
        }

        // ---- open / close ----

        [UnityTest]
        public IEnumerator Open_SingleScreen_CallsOnInitializeAndOnEnter() => UniTask.ToCoroutine(async () =>
        {
            var manager = CreateManager(out var factory);
            var data = new TestScreenStackData();

            await manager.Open(data);

            Assert.AreEqual(1, factory.Created.Count);
            var screen = factory.Created[0].Screen;
            Assert.AreEqual(1, screen.OnInitializeCallCount);
            Assert.AreEqual(1, screen.OnEnterCallCount);
            Assert.IsFalse(screen.LastOnEnterIsResume);
        });

        [UnityTest]
        public IEnumerator Open_SecondScreen_PreviousReceivesOnLeave() => UniTask.ToCoroutine(async () =>
        {
            var manager = CreateManager(out var factory);

            await manager.Open(new TestScreenStackData());
            await manager.Open(new TestScreenStackData());

            var first = factory.Created[0].Screen;
            Assert.AreEqual(1, first.OnLeaveCallCount);
        });

        [UnityTest]
        public IEnumerator Close_AfterTwoOpens_TopScreenDisposedAndPreviousResumes() => UniTask.ToCoroutine(async () =>
        {
            var manager = CreateManager(out var factory);

            await manager.Open(new TestScreenStackData());
            await manager.Open(new TestScreenStackData());
            await manager.Close();

            var first = factory.Created[0].Screen;
            var second = factory.Created[1].Screen;

            Assert.AreEqual(1, second.DisposeCallCount);
            Assert.AreEqual(2, first.OnEnterCallCount); // initial + resume
            Assert.IsTrue(first.LastOnEnterIsResume);
        });

        [UnityTest]
        public IEnumerator Close_EmptyStack_DoesNotThrow() => UniTask.ToCoroutine(async () =>
        {
            var manager = CreateManager(out _);
            Assert.DoesNotThrow(() => manager.Close().Forget());
            await UniTask.DelayFrame(1);
        });

        [UnityTest]
        public IEnumerator ClearAll_AfterTwoOpens_AllScreensDisposed() => UniTask.ToCoroutine(async () =>
        {
            var manager = CreateManager(out var factory);

            await manager.Open(new TestScreenStackData());
            await manager.Open(new TestScreenStackData());
            await manager.ClearAll();

            foreach (var (_, screen) in factory.Created)
            {
                Assert.AreEqual(1, screen.DisposeCallCount);
            }
        });

        // ---- overlay open ----

        [UnityTest]
        public IEnumerator Open_OverlayScreen_PreviousDoesNotPlayOutAnimation() => UniTask.ToCoroutine(async () =>
        {
            var manager = CreateManager(out var factory);

            await manager.Open(new TestScreenStackData());
            await manager.Open(new TestScreenStackData { IsOverlayOpen = true });

            var first = factory.Created[0].Screen;
            Assert.AreEqual(0, first.PlayOutAnimationCallCount);
            Assert.AreEqual(1, first.OnLeaveCallCount);
        });

        // ---- suspend / resume ----

        [UnityTest]
        public IEnumerator SuspendAndResume_RecreatesEntitiesForSavedData() => UniTask.ToCoroutine(async () =>
        {
            var manager = CreateManager(out var factory);
            var data = new TestScreenStackData();

            await manager.Enqueue(data);
            await manager.SuspendFromSceneId(SceneA);
            Assert.AreEqual(0, factory.Created.Count);

            await manager.ResumeFromSceneId(SceneA, false);
            Assert.AreEqual(1, factory.Created.Count);
        });

        [UnityTest]
        public IEnumerator ClearAll_AfterSuspend_SuspendedDataIsAlsoCleared() => UniTask.ToCoroutine(async () =>
        {
            var manager = CreateManager(out var factory);

            await manager.Enqueue(new TestScreenStackData());
            await manager.SuspendFromSceneId(SceneA);
            await manager.ClearAll();
            await manager.ResumeFromSceneId(SceneA, false);

            Assert.AreEqual(0, factory.Created.Count);
        });

        // ---- stubs ----

        class TestScreenStackData : IScreenStackData
        {
            public bool IsSystem { get; set; }
            public bool IsOverlayOpen { get; set; }
        }

        class StubScreenStack : IScreenStack
        {
            public int OnInitializeCallCount;
            public int OnEnterCallCount;
            public int OnLeaveCallCount;
            public int DisposeCallCount;
            public int PlayOutAnimationCallCount;
            public bool LastOnEnterIsResume;

            public void SetParent(Transform t) { }
            public UniTask OnInitialize() { OnInitializeCallCount++; return UniTask.CompletedTask; }
            public UniTask OnEnter(bool isResume) { OnEnterCallCount++; LastOnEnterIsResume = isResume; return UniTask.CompletedTask; }
            public UniTask OnLeave() { OnLeaveCallCount++; return UniTask.CompletedTask; }
            public void ResetInAnimation() { }
            public UniTask PlayInAnimation() => UniTask.CompletedTask;
            public void EndInAnimation() { }
            public void ResetOutAnimation() { }
            public UniTask PlayOutAnimation() { PlayOutAnimationCallCount++; return UniTask.CompletedTask; }
            public void EndOutAnimation() { }
            public void Dispose() { DisposeCallCount++; }
        }

        class StubScreenStackEntityFactory : IScreenStackEntityFactory
        {
            public List<(IScreenStackData Data, StubScreenStack Screen)> Created = new();

            public UniTask<ScreenStackEntity> CreateAsync(IScreenStackData data, CancellationToken ct)
            {
                var screen = new StubScreenStack();
                Created.Add((data, screen));
                return UniTask.FromResult(new ScreenStackEntity(screen, data));
            }
        }

        class StubScreenStackCanvasController : IScreenStackCanvasController
        {
            public void AddChild(IScreenStack s, bool isSystem) { }
            public void AddChild(IScreenStackBackgroundInputBlocker b, bool isSystem) { }
        }

        class StubScreenStackBackgroundInputBlocker : IScreenStackBackgroundInputBlocker
        {
            public void Setup() { }
            public void SetParent(Transform t) { }
            public void BlockScreenStackBackground(bool isSystem) { }
            public void UnBlock() { }
        }

        class StubInputBlocker : IInputBlocker
        {
            public IDisposable Block<T>(bool isSystemLayer = false) => new NullDisposable();
            public void UnBlock<T>(bool isSystemLayer = false) { }

            class NullDisposable : IDisposable
            {
                public void Dispose() { }
            }
        }
    }
}
