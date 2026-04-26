using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LighthouseExtends.Addressable.Tests
{
    [TestFixture]
    public class LHAssetScopeTests
    {
        FakeAddressablesLoader fakeLoader;
        LHAssetManager manager;
        Texture2D asset;

        [SetUp]
        public void SetUp()
        {
            fakeLoader = new FakeAddressablesLoader();
            manager = new LHAssetManager(fakeLoader);
            asset = new Texture2D(1, 1);
            fakeLoader.RegisterAsset("test/asset", asset);
        }

        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
            Object.DestroyImmediate(asset);
        }

        [Test]
        public async Task Dispose_ReleasesAllHandles()
        {
            var scope = manager.CreateScope();
            await scope.LoadAsync<Texture2D>("test/asset");

            scope.Dispose();

            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var scope = manager.CreateScope();

            scope.Dispose();
            Assert.DoesNotThrow(() => scope.Dispose());
        }

        [Test]
        public async Task Dispose_CalledTwice_ReleasesOnlyOnce()
        {
            var scope = manager.CreateScope();
            await scope.LoadAsync<Texture2D>("test/asset");

            scope.Dispose();
            scope.Dispose();

            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(1));
        }

        [Test]
        public void LoadAsync_AfterDispose_ThrowsObjectDisposedException()
        {
            var scope = manager.CreateScope();
            scope.Dispose();

            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await scope.LoadAsync<Texture2D>("test/asset"));
        }

        [Test]
        public async Task LoadAsync_EarlyHandleDispose_ReleasesBeforeScope()
        {
            var scope = manager.CreateScope();
            var handle = await scope.LoadAsync<Texture2D>("test/asset");

            handle.Dispose();
            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(1), "handle disposed early");

            scope.Dispose();
            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(1), "scope dispose is no-op for already-released handle");
        }

        [Test]
        public async Task LoadAsync_MultipleAssets_DisposeReleasesAll()
        {
            var asset2 = new Texture2D(1, 1);
            fakeLoader.RegisterAsset("test/asset2", asset2);

            var scope = manager.CreateScope();
            await scope.LoadAsync<Texture2D>("test/asset");
            await scope.LoadAsync<Texture2D>("test/asset2");

            scope.Dispose();

            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(2));

            Object.DestroyImmediate(asset2);
        }

        [Test]
        public async Task TryLoadAssetsAsync_PartialFailure_SuccessfulHandlesTracked()
        {
            var data = new ParallelLoadData();
            var successReq = data.Add<Texture2D>("test/asset");
            var failReq = data.Add<Texture2D>("test/missing"); // not registered → will throw

            var scope = manager.CreateScope();
            var result = await scope.TryLoadAssetsAsync(data);

            Assert.That(result.IsSuccess(successReq), Is.True);
            Assert.That(result.IsSuccess(failReq), Is.False);
            Assert.That(result.Get(successReq), Is.Not.Null);
            Assert.That(result.Get(failReq), Is.Null);

            scope.Dispose();
            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(1), "only the successful handle is released");
        }

        [Test]
        public async Task TryLoadAssetsAsync_AllSuccess_AllHandlesTracked()
        {
            var asset2 = new Texture2D(1, 1);
            fakeLoader.RegisterAsset("test/asset2", asset2);

            var data = new ParallelLoadData();
            var req1 = data.Add<Texture2D>("test/asset");
            var req2 = data.Add<Texture2D>("test/asset2");

            var scope = manager.CreateScope();
            var result = await scope.TryLoadAssetsAsync(data);

            Assert.That(result.IsSuccess(req1), Is.True);
            Assert.That(result.IsSuccess(req2), Is.True);

            scope.Dispose();
            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(2));

            Object.DestroyImmediate(asset2);
        }

        [Test]
        public async Task LoadAssetsAsync_Label_ReturnsRegisteredAssets()
        {
            fakeLoader.RegisterLabel<Texture2D>("test/label", new List<Texture2D> { asset });

            var scope = manager.CreateScope();
            var result = await scope.LoadAssetsAsync<Texture2D>("test/label");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(asset));

            scope.Dispose();
        }

        [Test]
        public async Task LoadAssetsAsync_Label_Dispose_ReleasesHandle()
        {
            fakeLoader.RegisterLabel<Texture2D>("test/label", new List<Texture2D> { asset });

            var scope = manager.CreateScope();
            await scope.LoadAssetsAsync<Texture2D>("test/label");

            scope.Dispose();

            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(1));
        }

        [Test]
        public async Task LoadAssetsAsync_Addresses_ReturnsAllAssets()
        {
            var asset2 = new Texture2D(1, 1);
            fakeLoader.RegisterAsset("test/asset2", asset2);

            var scope = manager.CreateScope();
            var result = await scope.LoadAssetsAsync<Texture2D>(new[] { "test/asset", "test/asset2" });

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(asset));
            Assert.That(result[1], Is.EqualTo(asset2));

            scope.Dispose();
            Object.DestroyImmediate(asset2);
        }

        [Test]
        public async Task LoadAssetsAsync_Addresses_Dispose_ReleasesAllHandles()
        {
            var asset2 = new Texture2D(1, 1);
            fakeLoader.RegisterAsset("test/asset2", asset2);

            var scope = manager.CreateScope();
            await scope.LoadAssetsAsync<Texture2D>(new[] { "test/asset", "test/asset2" });

            scope.Dispose();

            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(2));
            Object.DestroyImmediate(asset2);
        }

        [Test]
        public async Task LoadAssetsAsync_Addresses_MissingAddress_ReleasesAcquiredHandles()
        {
            var scope = manager.CreateScope();

            try
            {
                await scope.LoadAssetsAsync<Texture2D>(new[] { "test/asset", "test/missing" });
            }
            catch { }

            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(1), "first address acquired then released due to failure");
        }

        [Test]
        public async Task Load_SameAddress_TwoScopes_LoaderCalledOnlyOnce()
        {
            var scope1 = manager.CreateScope();
            var scope2 = manager.CreateScope();

            await scope1.LoadAsync<Texture2D>("test/asset");
            await scope2.LoadAsync<Texture2D>("test/asset");

            Assert.That(fakeLoader.LoadCount, Is.EqualTo(1));

            scope1.Dispose();
            scope2.Dispose();
        }

        [Test]
        public async Task Load_SameAddress_TwoScopes_ReleasesOnlyWhenLastScopeDisposed()
        {
            var scope1 = manager.CreateScope();
            var scope2 = manager.CreateScope();

            await scope1.LoadAsync<Texture2D>("test/asset");
            await scope2.LoadAsync<Texture2D>("test/asset");

            scope1.Dispose();
            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(0), "scope2 still holds the handle");

            scope2.Dispose();
            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(1), "last holder released");
        }

        [Test]
        public async Task Manager_Dispose_ReleasesAllActiveHandles()
        {
            var asset2 = new Texture2D(1, 1);
            fakeLoader.RegisterAsset("test/asset2", asset2);

            var scope = manager.CreateScope();
            await scope.LoadAsync<Texture2D>("test/asset");
            await scope.LoadAsync<Texture2D>("test/asset2");

            manager.Dispose();

            Assert.That(fakeLoader.ReleaseCount, Is.EqualTo(2));
            Object.DestroyImmediate(asset2);
        }

        [Test]
        public void Manager_Dispose_CalledTwice_DoesNotThrow()
        {
            manager.Dispose();
            Assert.DoesNotThrow(() => manager.Dispose());
        }
    }
}
