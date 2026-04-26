using NUnit.Framework;
using UnityEngine;

namespace LighthouseExtends.Addressable.Tests
{
    [TestFixture]
    public class ParallelLoadTests
    {
        [Test]
        public void Add_MultipleEntries_MappedInAddOrder()
        {
            var data = new ParallelLoadData();
            var req1 = data.Add<Texture2D>("a");
            var req2 = data.Add<Sprite>("b");
            var req3 = data.Add<Texture2D>("c");

            var handle1 = new FakeHandle<Texture2D>();
            var handle3 = new FakeHandle<Texture2D>();
            var result = new ParallelLoadResult(
                new IAssetHandle[] { handle1, null, handle3 },
                new bool[] { true, false, true }
            );

            Assert.That(result.IsSuccess(req1), Is.True);
            Assert.That(result.IsSuccess(req2), Is.False);
            Assert.That(result.IsSuccess(req3), Is.True);
        }

        [Test]
        public void IsSuccess_ReturnsTrue_WhenSucceeded()
        {
            var data = new ParallelLoadData();
            var req = data.Add<Texture2D>("a");

            var result = new ParallelLoadResult(
                new IAssetHandle[] { new FakeHandle<Texture2D>() },
                new bool[] { true }
            );

            Assert.That(result.IsSuccess(req), Is.True);
        }

        [Test]
        public void IsSuccess_ReturnsFalse_WhenFailed()
        {
            var data = new ParallelLoadData();
            var req = data.Add<Texture2D>("a");

            var result = new ParallelLoadResult(
                new IAssetHandle[] { null },
                new bool[] { false }
            );

            Assert.That(result.IsSuccess(req), Is.False);
        }

        [Test]
        public void Get_ReturnsHandle_WhenSucceeded()
        {
            var data = new ParallelLoadData();
            var req = data.Add<Texture2D>("a");
            var handle = new FakeHandle<Texture2D>();

            var result = new ParallelLoadResult(
                new IAssetHandle[] { handle },
                new bool[] { true }
            );

            Assert.That(result.Get(req), Is.SameAs(handle));
        }

        [Test]
        public void Get_ReturnsNull_WhenFailed()
        {
            var data = new ParallelLoadData();
            var req = data.Add<Texture2D>("a");

            var result = new ParallelLoadResult(
                new IAssetHandle[] { null },
                new bool[] { false }
            );

            Assert.That(result.Get(req), Is.Null);
        }

        [Test]
        public void Get_DifferentTypes_ReturnCorrectHandles()
        {
            var data = new ParallelLoadData();
            var texReq = data.Add<Texture2D>("tex");
            var spriteReq = data.Add<Sprite>("sprite");

            var texHandle = new FakeHandle<Texture2D>();
            var spriteHandle = new FakeHandle<Sprite>();
            var result = new ParallelLoadResult(
                new IAssetHandle[] { texHandle, spriteHandle },
                new bool[] { true, true }
            );

            Assert.That(result.Get(texReq), Is.SameAs(texHandle));
            Assert.That(result.Get(spriteReq), Is.SameAs(spriteHandle));
        }

        sealed class FakeHandle<T> : IAssetHandle<T> where T : Object
        {
            public T Asset => null;
            Object IAssetHandle.Asset => null;
            public void Dispose() { }
        }
    }
}
