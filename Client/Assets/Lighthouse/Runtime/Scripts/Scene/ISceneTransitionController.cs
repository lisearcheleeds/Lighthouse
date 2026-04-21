using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Executes the ordered phase sequence for a scene transition.
    /// Responsible for input blocking and CanTransitionIntercept validation during execution.
    /// </summary>
    public interface ISceneTransitionController
    {
        UniTask StartTransitionSequence(
            TransitionDataBase transitionData,
            SceneTransitionDiff sceneTransitionDiff,
            TransitionDirectionType transitionDirectionType,
            TransitionType transitionType,
            CancellationToken cancelToken);
    }
}