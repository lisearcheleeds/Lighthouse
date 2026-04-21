using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace Lighthouse.Scene.SceneCamera
{
    /// <summary>
    /// Implementation of ISceneCameraManager. Collects active cameras from the transition diff,
    /// assigns Base/Overlay roles and depths, and rebuilds the URP camera stack.
    /// Camera order is determined by SceneCameraType and default depth.
    /// </summary>
    public sealed class SceneCameraManager : ISceneCameraManager
    {
        readonly ICanvasSceneObject canvasSceneObject;

        public ISceneCamera BaseCamera { get; private set; }
        public ISceneCamera UICamera => canvasSceneObject.UICamera;

        public ISceneCamera[] OverlayCameraList { get; private set; }

        [Inject]
        public SceneCameraManager(ICanvasSceneObject canvasSceneObject)
        {
            this.canvasSceneObject = canvasSceneObject;
        }

        void ISceneCameraManager.UpdateCameraStack(
            IMainSceneManager mainSceneManager,
            SceneTransitionDiff sceneTransitionDiff)
        {
            var sceneCameras = mainSceneManager
                .GetSceneCameraList(sceneTransitionDiff)
                .Concat(new[] { UICamera })
                .Distinct()
                .OrderBy(x => (x.SceneCameraType, x.CameraDefaultDepth))
                .ToArray();

            if (!sceneCameras.Any())
            {
                LHLogger.LogError("[SceneCameraManager] No cameras found. At least UICamera must exist.");
                return;
            }

            var baseCamera = sceneCameras.First();
            var overlayCameraList = sceneCameras.Skip(1).ToArray();

            if (ReferenceEquals(BaseCamera, baseCamera) && OverlayCameraList.SequenceEqual(overlayCameraList))
            {
                return;
            }

            if (BaseCamera != null)
            {
                BaseCamera.ClearStackCamera();
            }

            if (OverlayCameraList != null)
            {
                foreach (var overlayCamera in OverlayCameraList)
                {
                    overlayCamera.ClearStackCamera();
                }
            }

            BaseCamera = baseCamera;
            OverlayCameraList = overlayCameraList;

            var depth = 0.0f;
            BaseCamera.SetupCamera(CameraRenderType.Base, depth);

            foreach (var overlayCamera in overlayCameraList)
            {
                depth++;

                overlayCamera.SetupCamera(CameraRenderType.Overlay, depth);
                BaseCamera.AddStackCamera(overlayCamera);
            }
        }
    }
}
