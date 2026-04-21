using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene.SceneTransitionStep
{
    /// <summary>
    /// Represents an individual step within a transition phase.
    /// Implement Run to define the concrete processing executed inside a phase.
    /// </summary>
    public interface ISceneTransitionStep
    {
        UniTask Run(ISceneTransitionContext context, CancellationToken cancelToken);
    }
}
