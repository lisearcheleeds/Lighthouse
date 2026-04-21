using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LighthouseExtends.Animation.Tests.PlayMode
{
    public class LHTransitionAnimatorTest
    {
        GameObject go;
        LHTransitionAnimator animator;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            go = new GameObject("TestAnimator");
            animator = go.AddComponent<LHTransitionAnimator>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InAnimation_NoClipsAssigned_CompletesImmediately() => UniTask.ToCoroutine(async () =>
        {
            await animator.InAnimation();
        });

        [UnityTest]
        public IEnumerator OutAnimation_NoClipsAssigned_CompletesImmediately() => UniTask.ToCoroutine(async () =>
        {
            await animator.OutAnimation();
        });

        [Test]
        public void ResetInAnimation_NoClipsAssigned_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => animator.ResetInAnimation());
        }

        [Test]
        public void ResetOutAnimation_NoClipsAssigned_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => animator.ResetOutAnimation());
        }

        [Test]
        public void EndInAnimation_NoClipsAssigned_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => animator.EndInAnimation());
        }

        [Test]
        public void EndOutAnimation_NoClipsAssigned_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => animator.EndOutAnimation());
        }
    }
}
