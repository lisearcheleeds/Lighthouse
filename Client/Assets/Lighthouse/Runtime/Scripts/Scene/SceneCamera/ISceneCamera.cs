using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lighthouse.Scene.SceneCamera
{
    /// <summary>
    /// Interface that abstracts a camera owned by a scene.
    /// Handles URP camera stack configuration (Base/Overlay switching, stack add/clear) and depth management.
    /// </summary>
    public interface ISceneCamera
    {
        SceneCameraType SceneCameraType { get; }
        float CameraDefaultDepth { get; }

        void SetupCamera(CameraRenderType cameraRenderType, float runtimeDepth);
        void AddStackCamera(ISceneCamera overlaySceneCamera);
        void ClearStackCamera();

        public Camera GetCamera();
    }
}