using LighthouseExtends.UIComponent.RaycastTargetObject;
using UnityEngine;
using VContainer;

namespace LighthouseExtends.ScreenStack
{
    public sealed class ScreenStackBackgroundInputBlocker : MonoBehaviour, IScreenStackBackgroundInputBlocker
    {
        [SerializeField] LHRaycastTargetObject raycastTargetObject;

        IScreenStackCanvasController screenStackCanvasController;

        RectTransform rectTransform;

        [Inject]
        public void Construct(IScreenStackCanvasController screenStackCanvasController)
        {
            this.screenStackCanvasController = screenStackCanvasController;
            rectTransform = (RectTransform)transform;
        }

        void IScreenStackBackgroundInputBlocker.Setup()
        {
            raycastTargetObject.gameObject.SetActive(false);
            screenStackCanvasController.AddChild(this, false);

        }

        void IScreenStackBackgroundInputBlocker.SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        void IScreenStackBackgroundInputBlocker.BlockScreenStackBackground(bool isSystem)
        {
            raycastTargetObject.gameObject.SetActive(true);
            screenStackCanvasController.AddChild(this, isSystem);
            raycastTargetObject.transform.SetSiblingIndex(Mathf.Max(0, raycastTargetObject.transform.parent.childCount - 2));

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        void IScreenStackBackgroundInputBlocker.UnBlock()
        {
            raycastTargetObject.gameObject.SetActive(false);
        }
    }
}