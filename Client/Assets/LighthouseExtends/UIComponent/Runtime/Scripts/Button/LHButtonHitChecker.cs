using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LighthouseExtends.UIComponent.Button
{
    public static class LHButtonHitChecker
    {
        static readonly List<RaycastResult> raycastResults = new();

        public static bool IsHittable(LHButton button)
        {
            if (!button.IsInteractable())
            {
                return false;
            }

            var canvas = button.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return false;
            }

            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            var screenPos = RectTransformUtility.WorldToScreenPoint(camera, button.transform.position);

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPos,
            };

            raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            if (raycastResults.Count == 0)
            {
                return false;
            }

            var topHit = raycastResults[0];
            return topHit.gameObject == button.gameObject
                || topHit.gameObject.transform.IsChildOf(button.transform);
        }

        public static bool TryClick(LHButton button)
        {
            if (!IsHittable(button))
            {
                return false;
            }

            button.onClick.Invoke();
            return true;
        }
    }
}
