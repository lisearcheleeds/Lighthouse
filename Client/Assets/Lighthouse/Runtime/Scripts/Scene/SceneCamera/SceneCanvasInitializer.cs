using UnityEngine;

namespace Lighthouse.Scene.SceneCamera
{
    /// <summary>
    /// Injects the UI camera into scene Canvases and enables them.
    /// All Canvases are disabled on Awake and activated only when Initialize is called.
    /// </summary>
    public class SceneCanvasInitializer : MonoBehaviour
    {
        [SerializeField] Canvas[] sceneCanvasList;

        void Awake()
        {
            foreach (var canvas in sceneCanvasList)
            {
                canvas.enabled = false;
            }
        }

        public void Initialize(ISceneCamera canvasCamera)
        {
            foreach (var canvas in sceneCanvasList)
            {
                canvas.enabled = true;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = canvasCamera.GetCamera();
            }
        }
    }
}