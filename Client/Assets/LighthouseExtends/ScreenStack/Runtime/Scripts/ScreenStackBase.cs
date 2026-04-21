using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LighthouseExtends.ScreenStack
{
    public abstract class ScreenStackBase : MonoBehaviour, IScreenStack
    {
        public virtual UniTask OnInitialize()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnEnter(bool isResume)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnLeave()
        {
            return UniTask.CompletedTask;
        }

        public void SetParent(Transform parentTransform)
        {
            transform.SetParent(parentTransform, false);
        }

        public virtual void ResetInAnimation()
        {
        }

        public virtual UniTask PlayInAnimation()
        {
            return UniTask.CompletedTask;
        }

        public virtual void EndInAnimation()
        {
        }

        public virtual void ResetOutAnimation()
        {
        }

        public virtual UniTask PlayOutAnimation()
        {
            return UniTask.CompletedTask;
        }

        public virtual void EndOutAnimation()
        {
        }

        public virtual void Dispose()
        {
            Destroy(gameObject);
        }
    }
}