using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Step that frees memory by calling GC and UnloadUnusedAssets.
    /// GC is invoked twice, before and after the unload.
    /// </summary>
    public sealed class CleanupStep : ISceneTransitionStep
    {
        async UniTask ISceneTransitionStep.Run(
            ISceneTransitionContext context,
            CancellationToken cancelToken)
        {
            // Before GC
            GC.Collect();

            await Resources.UnloadUnusedAssets();

            // After GC
            GC.Collect();
        }
    }
}
