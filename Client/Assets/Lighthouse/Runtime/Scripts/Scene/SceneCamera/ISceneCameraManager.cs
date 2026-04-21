namespace Lighthouse.Scene.SceneCamera
{
    /// <summary>
    /// Interface for the manager that rebuilds the camera stack on scene transitions.
    /// Holds references to the BaseCamera, UICamera, and the list of overlay cameras.
    /// </summary>
    public interface ISceneCameraManager
    {
        ISceneCamera BaseCamera { get; }
        ISceneCamera UICamera { get; }

        ISceneCamera[] OverlayCameraList { get; }

        void UpdateCameraStack(IMainSceneManager mainSceneManager, SceneTransitionDiff sceneTransitionDiff);
    }
}