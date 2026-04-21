using Lighthouse.Scene.SceneCamera;

namespace Lighthouse.Scene.SceneBase
{
    /// <summary>
    /// Interface for scene types that own a Canvas.
    /// Declares InitializeCanvas so the scene system can inject the correct camera.
    /// </summary>
    public interface ICanvasSceneBase
    {
        ISceneCamera[] GetSceneCameraList();
        void InitializeCanvas(ISceneCamera canvasCamera);
    }
}