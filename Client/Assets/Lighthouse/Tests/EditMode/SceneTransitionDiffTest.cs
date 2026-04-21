using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Lighthouse.Scene;

namespace Lighthouse.Tests.EditMode
{
    public class SceneTransitionDiffTest
    {
        static readonly MainSceneId MainA = new MainSceneId(1, "A");
        static readonly MainSceneId MainB = new MainSceneId(2, "B");
        static readonly ModuleSceneId ModuleX = new ModuleSceneId(10, "X");
        static readonly ModuleSceneId ModuleY = new ModuleSceneId(11, "Y");
        static readonly ModuleSceneId ModuleZ = new ModuleSceneId(12, "Z");

        static SceneGroup MakeGroup(Dictionary<MainSceneId, ModuleSceneId[]> map)
            => new SceneGroup(map);

        [Test]
        public void IsInnerGroupTransition_SameReference_ReturnsTrue()
        {
            var group = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, new[] { ModuleX } },
                { MainB, new[] { ModuleY } }
            });

            var diff = new SceneTransitionDiff(group, MainA, group, MainB);

            Assert.IsTrue(diff.IsInnerGroupTransition);
        }

        [Test]
        public void IsInnerGroupTransition_DifferentReference_ReturnsFalse()
        {
            var groupA = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, System.Array.Empty<ModuleSceneId>() }
            });
            var groupB = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainB, System.Array.Empty<ModuleSceneId>() }
            });

            var diff = new SceneTransitionDiff(groupA, MainA, groupB, MainB);

            Assert.IsFalse(diff.IsInnerGroupTransition);
        }

        [Test]
        public void FirstTransition_NullCurrent_LoadsAllNextScenes()
        {
            var nextGroup = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, new[] { ModuleX } }
            });

            var diff = new SceneTransitionDiff(null, null, nextGroup, MainA);

            Assert.AreEqual(1, diff.LoadMainSceneIds.Length);
            Assert.AreEqual(MainA, diff.LoadMainSceneIds[0]);
            Assert.AreEqual(0, diff.UnloadMainSceneIds.Length);
        }

        [Test]
        public void FirstTransition_NullCurrent_LoadsAllNextModules()
        {
            var nextGroup = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, new[] { ModuleX, ModuleY } }
            });

            var diff = new SceneTransitionDiff(null, null, nextGroup, MainA);

            Assert.AreEqual(2, diff.LoadSceneModuleIds.Length);
            Assert.AreEqual(0, diff.UnloadSceneModuleIds.Length);
        }

        [Test]
        public void GroupSwitch_LoadsNextUnloadsCurrent()
        {
            var groupA = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, System.Array.Empty<ModuleSceneId>() }
            });
            var groupB = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainB, System.Array.Empty<ModuleSceneId>() }
            });

            var diff = new SceneTransitionDiff(groupA, MainA, groupB, MainB);

            Assert.IsTrue(diff.LoadMainSceneIds.Contains(MainB));
            Assert.IsTrue(diff.UnloadMainSceneIds.Contains(MainA));
        }

        [Test]
        public void GroupSwitch_SharedModule_NotLoadedOrUnloaded()
        {
            var groupA = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, new[] { ModuleX, ModuleY } }
            });
            var groupB = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainB, new[] { ModuleX, ModuleZ } }
            });

            var diff = new SceneTransitionDiff(groupA, MainA, groupB, MainB);

            Assert.IsFalse(diff.LoadSceneModuleIds.Contains(ModuleX));
            Assert.IsFalse(diff.UnloadSceneModuleIds.Contains(ModuleX));
            Assert.IsTrue(diff.LoadSceneModuleIds.Contains(ModuleZ));
            Assert.IsTrue(diff.UnloadSceneModuleIds.Contains(ModuleY));
        }

        [Test]
        public void InnerGroupTransition_ActivatesAndDeactivatesModules()
        {
            var group = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, new[] { ModuleX, ModuleY } },
                { MainB, new[] { ModuleX, ModuleZ } }
            });

            var diff = new SceneTransitionDiff(group, MainA, group, MainB);

            Assert.IsTrue(diff.ActivateSceneModuleIds.Contains(ModuleZ));
            Assert.IsTrue(diff.DeactivateSceneModuleIds.Contains(ModuleY));
            Assert.IsFalse(diff.ActivateSceneModuleIds.Contains(ModuleX));
            Assert.IsFalse(diff.DeactivateSceneModuleIds.Contains(ModuleX));
        }

        [Test]
        public void InnerGroupTransition_NoMainSceneLoadOrUnload()
        {
            var group = MakeGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, System.Array.Empty<ModuleSceneId>() },
                { MainB, System.Array.Empty<ModuleSceneId>() }
            });

            var diff = new SceneTransitionDiff(group, MainA, group, MainB);

            Assert.AreEqual(0, diff.LoadMainSceneIds.Length);
            Assert.AreEqual(0, diff.UnloadMainSceneIds.Length);
        }
    }
}
