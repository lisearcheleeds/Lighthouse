using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse.Scene;
using Lighthouse.Scene.SceneBase;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lighthouse.Tests.PlayMode
{
    public class SceneBaseLifecycleTest
    {
        GameObject go;
        TestSceneBase scene;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            go = new GameObject("TestScene");
            scene = go.AddComponent<TestSceneBase>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Enter_FirstCall_InvokesOnSetupOnce() => UniTask.ToCoroutine(async () =>
        {
            await scene.Enter(null, CancellationToken.None);

            Assert.AreEqual(1, scene.SetupCallCount);
        });

        [UnityTest]
        public IEnumerator Enter_SecondCall_DoesNotInvokeOnSetupAgain() => UniTask.ToCoroutine(async () =>
        {
            await scene.Enter(null, CancellationToken.None);
            await scene.Enter(null, CancellationToken.None);

            Assert.AreEqual(1, scene.SetupCallCount);
        });

        [UnityTest]
        public IEnumerator Enter_AlwaysInvokesOnEnter() => UniTask.ToCoroutine(async () =>
        {
            await scene.Enter(null, CancellationToken.None);
            await scene.Enter(null, CancellationToken.None);

            Assert.AreEqual(2, scene.EnterCallCount);
        });

        [UnityTest]
        public IEnumerator Leave_InvokesOnLeave() => UniTask.ToCoroutine(async () =>
        {
            await scene.Leave(null, CancellationToken.None);

            Assert.AreEqual(1, scene.LeaveCallCount);
        });

        [UnityTest]
        public IEnumerator PlayInAnimation_CallsCallbacksInOrder() => UniTask.ToCoroutine(async () =>
        {
            await scene.PlayInAnimation(null);

            Assert.AreEqual(
                new[] { "BeginIn", "InAnimation", "CompleteIn" },
                scene.AnimationLog.ToArray());
        });

        [UnityTest]
        public IEnumerator PlayOutAnimation_CallsCallbacksInOrder() => UniTask.ToCoroutine(async () =>
        {
            await scene.PlayOutAnimation(null);

            Assert.AreEqual(
                new[] { "BeginOut", "OutAnimation", "CompleteOut" },
                scene.AnimationLog.ToArray());
        });

        // ---- test double ----

        class TestSceneBase : SceneBase
        {
            public int SetupCallCount;
            public int EnterCallCount;
            public int LeaveCallCount;
            public List<string> AnimationLog { get; } = new List<string>();

            protected override UniTask OnSetup()
            {
                SetupCallCount++;
                return UniTask.CompletedTask;
            }

            protected override UniTask OnEnter(ISceneTransitionContext context, CancellationToken ct)
            {
                EnterCallCount++;
                return UniTask.CompletedTask;
            }

            protected override UniTask OnLeave(ISceneTransitionContext context, CancellationToken ct)
            {
                LeaveCallCount++;
                return UniTask.CompletedTask;
            }

            protected override void OnBeginInAnimation(ISceneTransitionContext context) => AnimationLog.Add("BeginIn");
            protected override UniTask InAnimation(ISceneTransitionContext context) { AnimationLog.Add("InAnimation"); return UniTask.CompletedTask; }
            protected override void OnCompleteInAnimation(ISceneTransitionContext context) => AnimationLog.Add("CompleteIn");

            protected override void OnBeginOutAnimation(ISceneTransitionContext context) => AnimationLog.Add("BeginOut");
            protected override UniTask OutAnimation(ISceneTransitionContext context) { AnimationLog.Add("OutAnimation"); return UniTask.CompletedTask; }
            protected override void OnCompleteOutAnimation(ISceneTransitionContext context) => AnimationLog.Add("CompleteOut");
        }
    }
}
