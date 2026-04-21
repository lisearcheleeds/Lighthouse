using System.IO;
using Lighthouse.Editor.ScriptableObject;
using UnityEditor;
using UnityEngine;
using Lighthouse;

namespace Lighthouse.Editor.Menu
{
    /// <summary>
    /// Lighthouse Editor Script
    /// </summary>
    public class LighthouseEditor : UnityEditor.Editor
    {
        const string SettingsDefaultDirectory = "Assets/Settings/Lighthouse";
        const string SettingsFileExtension = ".asset";
        static string DefaultMainSceneTemplatesDir => $"{LighthouseEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultSceneScriptTemplates/MainScene";
        static string DefaultModuleSceneTemplatesDir => $"{LighthouseEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultSceneScriptTemplates/ModuleScene";
        static string DefaultMainSceneTemplateDir => $"{LighthouseEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultMainSceneTemplate";
        static string DefaultModuleSceneTemplateDir => $"{LighthouseEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultModuleSceneTemplate";

        [MenuItem("Lighthouse/Settings/GenerateSettings")]
        public static void ShowGenerateSettings()
        {
            ShowSettings<GenerateSettings>();
        }

        [MenuItem("Lighthouse/Settings/SceneEditSettings")]
        public static void ShowSceneEditSettings()
        {
            ShowSettings<SceneEditSettings>();
        }

        public static T GetOrCreateSettings<T>() where T : UnityEngine.ScriptableObject
        {
            return LoadSettings<T>() ?? CreateSettings<T>();
        }

        static void ShowSettings<T>() where T : UnityEngine.ScriptableObject
        {
            var instance = LoadSettings<T>();
            if (instance == null)
            {
                instance = CreateSettings<T>();
            }

            if (instance == null)
            {
                return;
            }

            Selection.activeObject = instance;
        }

        static T CreateSettings<T>() where T : UnityEngine.ScriptableObject
        {
            var settings = (T)CreateInstance(typeof(T));
            if (settings == null)
            {
                return null;
            }

            var fsDirPath = Path.Combine(Application.dataPath, "..", SettingsDefaultDirectory);
            if (!Directory.Exists(fsDirPath))
            {
                Directory.CreateDirectory(fsDirPath);
                AssetDatabase.Refresh();
            }

            var assetPath = $"{SettingsDefaultDirectory}/{typeof(T).Name}{SettingsFileExtension}";
            AssetDatabase.CreateAsset(settings, assetPath);

            if (settings is GenerateSettings generateSettings)
            {
                EnsureDefaultTemplateDirectoryExists(DefaultMainSceneTemplateDir);
                EnsureDefaultTemplateDirectoryExists(DefaultModuleSceneTemplateDir);

                var mainSceneTemplate = GetOrCreateSceneScriptTemplate(
                    GenerateSettings.DefaultMainSceneTemplatePath,
                    "Default MainScene",
                    DefaultMainSceneTemplatesDir);

                var moduleSceneTemplate = GetOrCreateSceneScriptTemplate(
                    GenerateSettings.DefaultModuleSceneTemplatePath,
                    "Default ModuleScene",
                    DefaultModuleSceneTemplatesDir);

                generateSettings.InitializeDefaults(
                    AssetDatabase.LoadAssetAtPath<TextAsset>(GenerateSettings.DefaultMainSceneIdTemplatePath),
                    AssetDatabase.LoadAssetAtPath<TextAsset>(GenerateSettings.DefaultModuleSceneIdTemplatePath),
                    new[] { mainSceneTemplate, moduleSceneTemplate });
                EditorUtility.SetDirty(settings);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        static void EnsureDefaultTemplateDirectoryExists(string assetDirPath)
        {
            var fsDirPath = Path.Combine(Application.dataPath, "..", assetDirPath);
            if (!Directory.Exists(fsDirPath))
            {
                Directory.CreateDirectory(fsDirPath);
                AssetDatabase.Refresh();
            }
        }

        static SceneScriptTemplate GetOrCreateSceneScriptTemplate(string assetPath, string templateName, string templateDirAssetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<SceneScriptTemplate>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            var template = (SceneScriptTemplate)CreateInstance(typeof(SceneScriptTemplate));
            AssetDatabase.CreateAsset(template, assetPath);
            template.InitializeDefaults(templateName, templateDirAssetPath);
            EditorUtility.SetDirty(template);
            return template;
        }

        static T LoadSettings<T>() where T : UnityEngine.ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0)
            {
                return null;
            }

            if (1 < guids.Length)
            {
                LHLogger.LogWarning($"[LighthouseEditor] Multiple {typeof(T).Name} found. Using the first one.");
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
        }
    }
}
