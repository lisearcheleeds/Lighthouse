using UnityEngine.EventSystems;

namespace Lighthouse.Scene.SceneCamera
{
    /// <summary>
    /// Interface for objects that provide a UI camera and EventSystem.
    /// Acts as the access point through which SceneCameraManager retrieves the UICamera.
    /// </summary>
    public interface ICanvasSceneObject
    {
        ISceneCamera UICamera { get; }
        EventSystem UIEventSystem { get; }
    }
}