using System.Linq;
using Lighthouse.Editor.Menu;
using Lighthouse.Editor.ScriptableObject;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Lighthouse.Editor.ScriptGenerator
{
    /// <summary>
    /// This is the window for manipulating EditorScript, which generates the scene's script.
    /// </summary>
    public class SceneScriptGeneratorWindow : EditorWindow
    {
        string sceneName = string.Empty;
        SceneType sceneType = SceneType.MainScene;
        int selectedBaseClassIndex;
        int selectedTemplateIndex;
        GenerateBaseClassInfo[] baseClasses = System.Array.Empty<GenerateBaseClassInfo>();
        string[] baseClassLabels = System.Array.Empty<string>();
        bool hasCompileErrors;
        GenerateSettings settings;
        string templateError;
        string selectedTemplateError;

        [MenuItem("Lighthouse/Generate/Open Scene scripts generator")]
        static void Open()
        {
            var window = GetWindow<SceneScriptGeneratorWindow>("Scene Script Generator");
            window.minSize = new Vector2(440, 300);
            window.Show();
        }

        void OnEnable()
        {
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            RefreshState();
        }

        void OnDisable()
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
        }

        void OnFocus()
        {
            RefreshState();
        }

        void OnCompilationFinished(object _)
        {
            RefreshState();
            Repaint();
        }

        void RefreshState()
        {
            if (SceneScriptGenerator.IsCompiling)
            {
                hasCompileErrors = false;
                templateError = null;
                selectedTemplateError = null;
                return;
            }

            hasCompileErrors = SceneScriptGenerator.HasCompileErrors();
            if (hasCompileErrors)
            {
                templateError = null;
                selectedTemplateError = null;
                return;
            }

            settings = LighthouseEditor.GetOrCreateSettings<GenerateSettings>();
            templateError = settings != null
                ? SceneScriptGenerator.GetTemplateValidationError(settings)
                : "Failed to load GenerateSettings.";

            if (templateError == null && settings != null)
            {
                selectedTemplateIndex = Mathf.Clamp(selectedTemplateIndex, 0, settings.SceneScriptTemplates.Length - 1);
                RefreshSelectedTemplateError();
            }

            RefreshBaseClasses();
        }

        void RefreshBaseClasses()
        {
            baseClasses = SceneScriptGenerator.CollectBaseClasses(sceneType);
            baseClassLabels = baseClasses.Select(b => b.DropdownLabel).ToArray();
            selectedBaseClassIndex = FindDefaultIndex();
        }

        void RefreshSelectedTemplateError()
        {
            if (settings == null || settings.SceneScriptTemplates.Length == 0)
            {
                selectedTemplateError = null;
                return;
            }

            var template = settings.SceneScriptTemplates[Mathf.Clamp(selectedTemplateIndex, 0, settings.SceneScriptTemplates.Length - 1)];
            selectedTemplateError = SceneScriptGenerator.GetTemplateValidationError(template);
        }

        int FindDefaultIndex()
        {
            var defaultName = sceneType == SceneType.MainScene ? "CanvasMainSceneBase" : "CanvasModuleSceneBase";
            var idx = System.Array.FindIndex(baseClasses, b => b.TypeName == defaultName);
            return idx >= 0 ? idx : 0;
        }

        void OnGUI()
        {
            GUILayout.Label("Scene Script Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (SceneScriptGenerator.IsCompiling)
            {
                EditorGUILayout.HelpBox("Compiling. Please wait until compilation is finished.", MessageType.Info);
                return;
            }

            if (hasCompileErrors)
            {
                EditorGUILayout.HelpBox(
                    "Compile errors detected.\nPlease fix all errors and reopen this window.",
                    MessageType.Error);
                if (GUILayout.Button("Refresh"))
                {
                    RefreshState();
                }

                return;
            }

            if (templateError != null)
            {
                EditorGUILayout.HelpBox(templateError, MessageType.Error);
                if (GUILayout.Button("Open GenerateSettings"))
                {
                    LighthouseEditor.ShowGenerateSettings();
                }

                return;
            }

            // Scene Type
            EditorGUI.BeginChangeCheck();
            sceneType = (SceneType)EditorGUILayout.EnumPopup("Scene Type", sceneType);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshBaseClasses();
            }

            EditorGUILayout.Space();

            // Template Preset
            var templates = settings.SceneScriptTemplates;
            var templateLabels = System.Array.ConvertAll(templates, t => t != null ? t.TemplateName : "(null)");
            EditorGUILayout.LabelField("Template Preset");
            EditorGUI.BeginChangeCheck();
            selectedTemplateIndex = EditorGUILayout.Popup(
                Mathf.Clamp(selectedTemplateIndex, 0, templates.Length - 1),
                templateLabels);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshSelectedTemplateError();
            }

            if (selectedTemplateError != null)
            {
                EditorGUILayout.HelpBox(selectedTemplateError, MessageType.Error);
            }

            EditorGUILayout.Space();

            // Base Class
            if (baseClasses.Length > 0)
            {
                EditorGUILayout.LabelField("Base Class");
                selectedBaseClassIndex = EditorGUILayout.Popup(
                    Mathf.Clamp(selectedBaseClassIndex, 0, baseClasses.Length - 1),
                    baseClassLabels);
            }
            else
            {
                EditorGUILayout.HelpBox("No base classes found.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // Scene Name
            EditorGUILayout.LabelField("Scene Name");
            sceneName = EditorGUILayout.TextField(sceneName);
            EditorGUILayout.Space(12);

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                EditorGUILayout.HelpBox("Please enter a Scene Name.", MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(
                       string.IsNullOrWhiteSpace(sceneName) || baseClasses.Length == 0 || selectedTemplateError != null))
            {
                if (GUILayout.Button("Generate", GUILayout.Height(32)))
                {
                    Generate();
                }
            }
        }

        void Generate()
        {
            var template = settings.SceneScriptTemplates[Mathf.Clamp(selectedTemplateIndex, 0, settings.SceneScriptTemplates.Length - 1)];
            SceneScriptGenerator.GenerateFiles(
                sceneName.Trim(),
                sceneType,
                baseClasses[selectedBaseClassIndex],
                settings,
                template);
        }
    }
}
