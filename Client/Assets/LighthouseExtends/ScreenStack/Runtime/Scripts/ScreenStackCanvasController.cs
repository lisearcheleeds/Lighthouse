using UnityEngine;

namespace LighthouseExtends.ScreenStack
{
    public class ScreenStackCanvasController : MonoBehaviour, IScreenStackCanvasController
    {
        [SerializeField] Canvas defaultLayerCanvas;
        [SerializeField] Canvas systemLayerCanvas;

        void IScreenStackCanvasController.AddChild(IScreenStack screenStack, bool isSystemLayer)
        {
            screenStack.SetParent(
                isSystemLayer
                    ? systemLayerCanvas.transform
                    : defaultLayerCanvas.transform);
        }

        void IScreenStackCanvasController.AddChild(IScreenStackBackgroundInputBlocker screenStackBackgroundInputBlocker, bool isSystemLayer)
        {
            screenStackBackgroundInputBlocker.SetParent(
                isSystemLayer
                    ? systemLayerCanvas.transform
                    : defaultLayerCanvas.transform);
        }
    }
}