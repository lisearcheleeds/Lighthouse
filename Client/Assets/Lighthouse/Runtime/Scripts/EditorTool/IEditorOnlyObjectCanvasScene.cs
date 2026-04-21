#if UNITY_EDITOR
using Lighthouse.Scene.SceneBase;

namespace Lighthouse.EditorTool
{
    /// <summary>
    /// This class temporarily generates and manages components such as Canvas and Camera that are necessary for editing a scene.
    /// These are automatically generated when you open a scene, but they are not saved as assets.
    /// </summary>
    public interface IEditorOnlyObjectCanvasScene
    {
        void Apply(ICanvasSceneBase[] canvasSceneBaseList);
        void Revoke();
    }
}
#endif
