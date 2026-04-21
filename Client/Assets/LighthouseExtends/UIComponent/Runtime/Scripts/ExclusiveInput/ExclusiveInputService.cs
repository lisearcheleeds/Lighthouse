using System;

namespace LighthouseExtends.UIComponent.ExclusiveInput
{
    public sealed class ExclusiveInputService : IExclusiveInputService, IDisposable
    {
        public static IExclusiveInputService Instance { get; private set; }

        int? activePointerId;

        public ExclusiveInputService()
        {
            Instance = this;
        }

        bool IExclusiveInputService.TryUsePointerId(int pointerId)
        {
            if (activePointerId.HasValue)
            {
                return false;
            }

            activePointerId = pointerId;
            return true;
        }

        void IExclusiveInputService.ReleasePointerId(int pointerId)
        {
            if (activePointerId == pointerId)
            {
                activePointerId = null;
            }
        }

        public void Dispose()
        {
            Instance = null;
        }
    }
}
