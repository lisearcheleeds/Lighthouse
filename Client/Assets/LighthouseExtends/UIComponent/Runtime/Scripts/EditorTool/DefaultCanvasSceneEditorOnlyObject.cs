#if UNITY_EDITOR
using Lighthouse.EditorTool;
using Lighthouse.Scene.SceneBase;
using LighthouseExtends.UIComponent.CanvasSceneObject;
using UnityEngine;

namespace LighthouseExtends.UIComponent.EditorTool
{
    public class DefaultCanvasSceneEditorOnlyObject : MonoBehaviour, IEditorOnlyObjectCanvasScene
    {
        [SerializeField] LHCanvasSceneObject lhCanvasSceneObject;

        public void Apply(ICanvasSceneBase[] canvasSceneBaseList)
        {
            foreach (var canvasSceneBase in canvasSceneBaseList)
            {
                canvasSceneBase.InitializeCanvas(lhCanvasSceneObject.UICamera);
            }
        }

        public void Revoke()
        {
            DestroyImmediate(this);
        }
    }
}
#endif