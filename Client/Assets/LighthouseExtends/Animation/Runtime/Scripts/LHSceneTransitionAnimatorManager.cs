using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LighthouseExtends.Animation
{
    /// <summary>
    /// Aggregates ILHSceneTransitionAnimator components on child objects and drives them as a unit.
    /// In/OutAnimation are executed in parallel via WhenAll.
    /// When IsAutoCollect is true the animator list is refreshed automatically in OnValidate.
    /// </summary>
    public class LHSceneTransitionAnimatorManager : MonoBehaviour
    {
        [SerializeField] bool isAutoCollect = true;

        [SerializeField] MonoBehaviour[] sceneTransitionAnimatorList;

        public void ResetInAnimation()
        {
            foreach (var sceneTransitionAnimator in sceneTransitionAnimatorList)
            {
                ((ILHSceneTransitionAnimator)sceneTransitionAnimator).ResetInAnimation();
            }
        }

        public async UniTask InAnimation()
        {
            await UniTask.WhenAll(sceneTransitionAnimatorList.Select(x => ((ILHSceneTransitionAnimator)x).InAnimation()));
        }

        public async UniTask OutAnimation()
        {
            await UniTask.WhenAll(sceneTransitionAnimatorList.Select(x => ((ILHSceneTransitionAnimator)x).OutAnimation()));
        }

        public void CollectAnimators()
        {
            var beforeCount = sceneTransitionAnimatorList?.Length ?? 0;
            sceneTransitionAnimatorList = GetComponentsInChildren<MonoBehaviour>()
                .Where(x => x is ILHSceneTransitionAnimator)
                .ToArray();

            if (beforeCount != sceneTransitionAnimatorList.Length)
            {
                Debug.Log($"[LHSceneTransitionAnimatorManager] Collected ILHSceneTransitionAnimator: {sceneTransitionAnimatorList.Length}");
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (isAutoCollect)
            {
                CollectAnimators();
            }
        }
#endif
    }
}
