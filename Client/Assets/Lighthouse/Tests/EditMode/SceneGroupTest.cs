using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Lighthouse.Scene;

namespace Lighthouse.Tests.EditMode
{
    public class SceneGroupTest
    {
        static readonly MainSceneId MainA = new MainSceneId(1, "A");
        static readonly MainSceneId MainB = new MainSceneId(2, "B");
        static readonly ModuleSceneId ModuleX = new ModuleSceneId(10, "X");
        static readonly ModuleSceneId ModuleY = new ModuleSceneId(11, "Y");

        [Test]
        public void MainSceneIds_ContainsAllKeys()
        {
            var group = new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, new[] { ModuleX } },
                { MainB, new[] { ModuleY } }
            });

            Assert.AreEqual(2, group.MainSceneIds.Length);
            Assert.IsTrue(group.MainSceneIds.Contains(MainA));
            Assert.IsTrue(group.MainSceneIds.Contains(MainB));
        }

        [Test]
        public void SceneModuleIds_DeduplicatesSharedModules()
        {
            var group = new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, new[] { ModuleX, ModuleY } },
                { MainB, new[] { ModuleX } }
            });

            Assert.AreEqual(2, group.SceneModuleIds.Length);
            Assert.IsTrue(group.SceneModuleIds.Contains(ModuleX));
            Assert.IsTrue(group.SceneModuleIds.Contains(ModuleY));
        }

        [Test]
        public void SceneModuleMap_ReflectsInputDictionary()
        {
            var group = new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, new[] { ModuleX } }
            });

            Assert.IsTrue(group.SceneModuleMap.ContainsKey(MainA));
            Assert.AreEqual(1, group.SceneModuleMap[MainA].Length);
            Assert.AreEqual(ModuleX, group.SceneModuleMap[MainA][0]);
        }

        [Test]
        public void SceneModuleIds_EmptyModules_ReturnsEmpty()
        {
            var group = new SceneGroup(new Dictionary<MainSceneId, ModuleSceneId[]>
            {
                { MainA, System.Array.Empty<ModuleSceneId>() }
            });

            Assert.AreEqual(0, group.SceneModuleIds.Length);
        }
    }
}
