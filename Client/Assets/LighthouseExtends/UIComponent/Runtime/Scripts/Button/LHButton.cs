using LighthouseExtends.UIComponent.ExclusiveInput;
using UnityEngine.EventSystems;

namespace LighthouseExtends.UIComponent.Button
{
    public class LHButton : UnityEngine.UI.Button
    {
        bool isUsing;
        int currentPointerId;

        public override void OnPointerDown(PointerEventData eventData)
        {
            var service = ExclusiveInputService.Instance;
            if (service != null && !service.TryUsePointerId(eventData.pointerId))
            {
                return;
            }

            isUsing = true;
            currentPointerId = eventData.pointerId;
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!isUsing)
            {
                return;
            }

            base.OnPointerUp(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!isUsing)
            {
                return;
            }

            base.OnPointerClick(eventData);
            Release();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (isUsing)
            {
                Release();
            }
        }

        void OnApplicationPause(bool pause)
        {
            if (!pause || !isUsing)
            {
                return;
            }

            Release();
            InstantClearState();
        }

        void Release()
        {
            ExclusiveInputService.Instance?.ReleasePointerId(currentPointerId);
            isUsing = false;
        }
    }
}
