namespace Lighthouse.Scene.SceneCamera
{
    /// <summary>
    /// Defines the role of a camera in the scene (3D, 2D, or UI).
    /// Used to determine camera sort order when building the URP camera stack.
    /// </summary>
    public enum SceneCameraType
    {
        Camera3D,
        Camera2D,
        CameraUI,
    }
}