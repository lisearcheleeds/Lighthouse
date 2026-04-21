using System.Collections.Generic;
using System.Linq;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Represents a group of one or more main scenes paired with their associated module scenes.
    /// Provides a SceneModuleMap dictionary for fast lookup of module scenes by main scene.
    /// </summary>
    public sealed class SceneGroup
    {
        public MainSceneId[] MainSceneIds { get; }
        public ModuleSceneId[] SceneModuleIds { get; }

        public IReadOnlyDictionary<MainSceneId, ModuleSceneId[]> SceneModuleMap { get; }

        public SceneGroup(Dictionary<MainSceneId, ModuleSceneId[]> sceneModuleMap)
        {
            SceneModuleMap = sceneModuleMap;

            MainSceneIds = sceneModuleMap.Keys
                .Distinct()
                .ToArray();

            SceneModuleIds =
                sceneModuleMap.Values.SelectMany(x => x)
                .Distinct()
                .ToArray();
        }
    }
}