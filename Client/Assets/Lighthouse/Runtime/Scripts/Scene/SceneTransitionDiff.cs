using System;
using System.Collections.Generic;
using System.Linq;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Computes the difference between the current and next SceneGroup on transition.
    /// Produces Load/Unload/Activate/Deactivate ID lists consumed by transition steps throughout the pipeline.
    /// </summary>
    public sealed class SceneTransitionDiff
    {
        public bool IsInnerGroupTransition { get; }

        public SceneGroup CurrentSceneGroup { get; }
        public MainSceneId CurrentMainSceneId { get; }
        public SceneGroup NextSceneGroup { get; }
        public MainSceneId NextMainSceneId { get; }

        public MainSceneId[] LoadMainSceneIds { get; }
        public MainSceneId[] UnloadMainSceneIds { get; }
        public ModuleSceneId[] LoadSceneModuleIds { get; }
        public ModuleSceneId[] UnloadSceneModuleIds { get; }

        public ModuleSceneId[] ActivateSceneModuleIds { get; }
        public ModuleSceneId[] DeactivateSceneModuleIds { get; }

        public SceneTransitionDiff(SceneGroup currentSceneGroup, MainSceneId currentMainSceneId, SceneGroup nextSceneGroup, MainSceneId nextMainSceneId)
        {
            IsInnerGroupTransition = ReferenceEquals(currentSceneGroup, nextSceneGroup);

            CurrentSceneGroup = currentSceneGroup;
            CurrentMainSceneId = currentMainSceneId;
            NextSceneGroup = nextSceneGroup;
            NextMainSceneId = nextMainSceneId;

            LoadMainSceneIds = GetMainSceneIdsOnlyInNext(currentSceneGroup, nextSceneGroup);
            UnloadMainSceneIds = GetMainSceneIdsOnlyInCurrent(currentSceneGroup, nextSceneGroup);
            LoadSceneModuleIds = GetSceneModuleIdsOnlyInNext(currentSceneGroup, currentMainSceneId, nextSceneGroup, nextMainSceneId);
            UnloadSceneModuleIds = GetSceneModuleIdsOnlyInCurrent(currentSceneGroup, currentMainSceneId, nextSceneGroup, nextMainSceneId);

            var currentSceneModuleIds = currentSceneGroup?.SceneModuleMap[currentMainSceneId] ?? Array.Empty<ModuleSceneId>();
            var nextSceneModuleIds = nextSceneGroup.SceneModuleMap[nextMainSceneId];
            ActivateSceneModuleIds = nextSceneModuleIds.Except(currentSceneModuleIds).ToArray();
            DeactivateSceneModuleIds = currentSceneModuleIds.Except(nextSceneModuleIds).ToArray();
        }

        static MainSceneId[] GetMainSceneIdsOnlyInNext(SceneGroup currentSceneGroup, SceneGroup nextSceneGroup)
        {
            var currentMainSceneIds = currentSceneGroup?.MainSceneIds ?? Array.Empty<MainSceneId>();
            var nextMainSceneIds = nextSceneGroup.MainSceneIds;

            var currentSet = new HashSet<MainSceneId>(currentMainSceneIds);

            return nextMainSceneIds
                .Where(m => !currentSet.Contains(m))
                .Distinct()
                .ToArray();
        }

        static MainSceneId[] GetMainSceneIdsOnlyInCurrent(SceneGroup currentSceneGroup, SceneGroup nextSceneGroup)
        {
            var currentMainSceneIds = currentSceneGroup?.MainSceneIds ?? Array.Empty<MainSceneId>();
            var nextMainSceneIds = nextSceneGroup.MainSceneIds;

            var nextSet = new HashSet<MainSceneId>(nextMainSceneIds);

            return currentMainSceneIds
                .Where(m => !nextSet.Contains(m))
                .Distinct()
                .ToArray();
        }

        static ModuleSceneId[] GetSceneModuleIdsOnlyInNext(SceneGroup currentSceneGroup, MainSceneId currentSceneId, SceneGroup nextSceneGroup, MainSceneId nextSceneId)
        {
            var currentSceneModuleIds = currentSceneGroup?.SceneModuleMap[currentSceneId] ?? Array.Empty<ModuleSceneId>();
            var nextSceneModuleIds = nextSceneGroup.SceneModuleMap[nextSceneId];

            var currentSet = new HashSet<ModuleSceneId>(currentSceneModuleIds);

            return nextSceneModuleIds?.Where(m => !currentSet.Contains(m)).Distinct().ToArray() ?? Array.Empty<ModuleSceneId>();
        }

        static ModuleSceneId[] GetSceneModuleIdsOnlyInCurrent(SceneGroup currentSceneGroup, MainSceneId currentSceneId, SceneGroup nextSceneGroup, MainSceneId nextSceneId)
        {
            var currentSceneModuleIds = currentSceneGroup?.SceneModuleMap[currentSceneId] ?? Array.Empty<ModuleSceneId>();
            var nextSceneModuleIds = nextSceneGroup.SceneModuleMap[nextSceneId] ?? Array.Empty<ModuleSceneId>();

            var nextSet = new HashSet<ModuleSceneId>(nextSceneModuleIds);

            return currentSceneModuleIds.Where(m => !nextSet.Contains(m)).Distinct().ToArray();
        }
    }
}