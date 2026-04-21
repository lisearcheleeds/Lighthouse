using System.Linq;
using Lighthouse.Editor.Menu;
using Lighthouse.Editor.ScriptableObject;
using Lighthouse.EditorTool;
using Lighthouse.Scene.SceneBase;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lighthouse.Editor.PostProcess.SceneEditor
{
    /// <summary>
    /// This creates a temporary GameObject for use while editing in the Unity Editor.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneEditProcessor
    {
        static readonly SceneEditSettings sceneEditSettings;

        static SceneEditProcessor()
        {
            if (Application.isBatchMode || BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            sceneEditSettings = LighthouseEditor.GetOrCreateSettings<SceneEditSettings>();
            if (sceneEditSettings.EnableSceneEditProcess)
            {
                SetupSceneEditProcessor();
            }
        }

        static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            DestroyEditorOnlyObject();
            InitEditorOnlyObject();
        }

        static void OnSceneClosed(UnityEngine.SceneManagement.Scene scene)
        {
            DestroyEditorOnlyObject();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode:
                    DestroyEditorOnlyObject();
                    InitEditorOnlyObject();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    DestroyEditorOnlyObject();
                    break;
            }
        }

        static void SetupSceneEditProcessor()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneClosed += OnSceneClosed;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += () =>
                {
                    DestroyEditorOnlyObject();
                    InitEditorOnlyObject();
                };
            }
        }

        static void InitEditorOnlyObject()
        {
            if (!sceneEditSettings.EnableSceneEditProcess || sceneEditSettings.CanvasSceneEditorOnlyObject == null)
            {
                return;
            }

            if (GetSceneRootComponentList<IEditorOnlyObjectCanvasScene>().Any())
            {
                return;
            }

            var canvasSceneBaseList = GetSceneRootComponentList<ICanvasSceneBase>();
            if (!canvasSceneBaseList.Any())
            {
                return;
            }

            var editorOnlyObject = Object.Instantiate(sceneEditSettings.CanvasSceneEditorOnlyObject);
            editorOnlyObject.name = sceneEditSettings.EditorOnlyObjectName;

            SetHideFlags(editorOnlyObject.transform, HideFlags.DontSave);

            var editorOnlyComponents = editorOnlyObject.GetComponents<MonoBehaviour>().OfType<IEditorOnlyObjectCanvasScene>().ToArray();
            foreach (var editorOnlyComponent in editorOnlyComponents)
            {
                editorOnlyComponent.Apply(canvasSceneBaseList);
            }
        }

        static void DestroyEditorOnlyObject()
        {
            var editorOnlyObjectList = GetSceneRootComponentList<IEditorOnlyObjectCanvasScene>();
            foreach (var editorOnlyObject in editorOnlyObjectList)
            {
                var gameObject = editorOnlyObject is MonoBehaviour mono ? mono.gameObject : null;
                editorOnlyObject.Revoke();
                if (gameObject != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        static T[] GetSceneRootComponentList<T>()
        {
            return Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Where(scene => scene.IsValid() && scene.isLoaded)
                .SelectMany(scene => scene.GetRootGameObjects().SelectMany(x => x.GetComponents<MonoBehaviour>().OfType<T>()))
                .ToArray();
        }

        static void SetHideFlags(Transform obj, HideFlags flags)
        {
            obj.gameObject.hideFlags = flags;
            foreach (var child in obj.Cast<Transform>())
            {
                SetHideFlags(child, flags);
            }
        }
    }
}
