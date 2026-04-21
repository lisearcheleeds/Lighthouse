using System;
using System.IO;
using System.Linq;
using LighthouseExtends.ScreenStack.Editor.ScriptableObject;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace LighthouseExtends.ScreenStack.Editor.ScriptGenerator
{
    public class ScreenStackDialogScriptGeneratorWindow : EditorWindow
    {
        ScreenStackDialogBaseClassInfo[] baseClasses = Array.Empty<ScreenStackDialogBaseClassInfo>();
        string[] baseClassLabels = Array.Empty<string>();
        string screenStackName = string.Empty;
        bool hasCompileErrors;
        int selectedBaseClassIndex;
        int selectedTemplateIndex;
        ScreenStackGenerateSettings settings;
        string templateError;
        string selectedTemplateError;

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

        [MenuItem("Lighthouse/Generate/Open ScreenStack scripts generator")]
        static void Open()
        {
            var window = GetWindow<ScreenStackDialogScriptGeneratorWindow>("ScreenStack Script Generator");
            window.minSize = new Vector2(440, 270);
            window.Show();
        }

        void RefreshState()
        {
            if (ScreenStackDialogScriptGenerator.IsCompiling)
            {
                hasCompileErrors = false;
                templateError = null;
                selectedTemplateError = null;
                return;
            }

            hasCompileErrors = ScreenStackDialogScriptGenerator.HasCompileErrors();
            if (hasCompileErrors)
            {
                templateError = null;
                selectedTemplateError = null;
                return;
            }

            settings = GetOrCreateSettings();
            templateError = settings != null
                ? ScreenStackDialogScriptGenerator.GetTemplateValidationError(settings)
                : "Failed to load ScreenStackGenerateSettings.";

            if (templateError == null && settings != null)
            {
                selectedTemplateIndex = Mathf.Clamp(selectedTemplateIndex, 0, settings.ScreenStackScriptTemplates.Length - 1);
                RefreshSelectedTemplateError();
            }

            RefreshBaseClasses();
        }

        void RefreshBaseClasses()
        {
            baseClasses = ScreenStackDialogScriptGenerator.CollectBaseClasses();
            baseClassLabels = baseClasses.Select(b => b.DropdownLabel).ToArray();
            selectedBaseClassIndex = Mathf.Clamp(selectedBaseClassIndex, 0, Mathf.Max(0, baseClasses.Length - 1));
        }

        void RefreshSelectedTemplateError()
        {
            if (settings == null || settings.ScreenStackScriptTemplates.Length == 0)
            {
                selectedTemplateError = null;
                return;
            }

            var template = settings.ScreenStackScriptTemplates[Mathf.Clamp(selectedTemplateIndex, 0, settings.ScreenStackScriptTemplates.Length - 1)];
            selectedTemplateError = ScreenStackDialogScriptGenerator.GetTemplateValidationError(template);
        }

        void OnGUI()
        {
            GUILayout.Label("ScreenStack Script Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (ScreenStackDialogScriptGenerator.IsCompiling)
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
                if (GUILayout.Button("Open ScreenStackGenerateSettings"))
                {
                    ShowSettings();
                }

                return;
            }

            // Template Preset
            var templates = settings.ScreenStackScriptTemplates;
            var templateLabels = Array.ConvertAll(templates, t => t != null ? t.TemplateName : "(null)");
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

            // Name
            EditorGUILayout.LabelField("Name");
            screenStackName = EditorGUILayout.TextField(screenStackName);
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

            EditorGUILayout.Space(12);

            if (string.IsNullOrWhiteSpace(screenStackName))
            {
                EditorGUILayout.HelpBox("Please enter a Name.", MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(
                       string.IsNullOrWhiteSpace(screenStackName) || baseClasses.Length == 0 || selectedTemplateError != null))
            {
                if (GUILayout.Button("Generate", GUILayout.Height(32)))
                {
                    Generate();
                }
            }
        }

        void Generate()
        {
            var template = settings.ScreenStackScriptTemplates[Mathf.Clamp(selectedTemplateIndex, 0, settings.ScreenStackScriptTemplates.Length - 1)];
            ScreenStackDialogScriptGenerator.GenerateFiles(
                screenStackName.Trim(),
                baseClasses[selectedBaseClassIndex],
                settings,
                template);
        }

        static ScreenStackGenerateSettings GetOrCreateSettings()
        {
            var existing = LoadSettings();
            if (existing != null)
            {
                return existing;
            }

            const string SettingsDir = "Assets/Settings/Lighthouse";
            const string SettingsPath = SettingsDir + "/ScreenStackGenerateSettings.asset";

            var fsDirPath = Path.Combine(Application.dataPath, "..", SettingsDir);
            if (!Directory.Exists(fsDirPath))
            {
                Directory.CreateDirectory(fsDirPath);
                AssetDatabase.Refresh();
            }

            var settings = CreateInstance<ScreenStackGenerateSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);

            var templateDir = ScreenStackGenerateSettings.DefaultDialogTemplateDir;
            var templateFsDir = ScreenStackEditorPaths.AssetPathToFsPath(templateDir);
            if (!Directory.Exists(templateFsDir))
            {
                Directory.CreateDirectory(templateFsDir);
                AssetDatabase.Refresh();
            }

            var defaultTemplate = AssetDatabase.LoadAssetAtPath<ScreenStackScriptTemplate>(ScreenStackGenerateSettings.DefaultDialogTemplatePath);
            if (defaultTemplate == null)
            {
                defaultTemplate = CreateInstance<ScreenStackScriptTemplate>();
                AssetDatabase.CreateAsset(defaultTemplate, ScreenStackGenerateSettings.DefaultDialogTemplatePath);
                defaultTemplate.InitializeDefaults("Default Dialog", templateDir);
                EditorUtility.SetDirty(defaultTemplate);
            }

            settings.InitializeDefaults(new[] { defaultTemplate });
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        static void ShowSettings()
        {
            var s = LoadSettings();
            if (s != null)
            {
                Selection.activeObject = s;
            }
        }

        static ScreenStackGenerateSettings LoadSettings()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(ScreenStackGenerateSettings)}");
            if (guids.Length == 0)
            {
                return null;
            }

            if (guids.Length > 1)
            {
                Debug.LogWarning(
                    $"[ScreenStackDialogScriptGeneratorWindow] Multiple {nameof(ScreenStackGenerateSettings)} found. Using the first one.");
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return (ScreenStackGenerateSettings)AssetDatabase.LoadAssetAtPath(path, typeof(ScreenStackGenerateSettings));
        }
    }
}
