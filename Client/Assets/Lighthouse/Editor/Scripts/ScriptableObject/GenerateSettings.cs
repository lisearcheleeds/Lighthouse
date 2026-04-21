using System.IO;
using Lighthouse.Editor.PropertyDrawer;
using UnityEditor;
using UnityEngine;

namespace Lighthouse.Editor.ScriptableObject
{
    public class GenerateSettings : UnityEngine.ScriptableObject
    {
        [SerializeField] string productNameSpace = "YourProduct";
        [SerializeField, FolderOnly] DefaultAsset generatedFileDirectoryAsset;
        [SerializeField] string mainSceneIdPrefix = "YourProduct";
        [SerializeField] string moduleSceneIdPrefix = "YourProduct";
        [SerializeField] TextAsset mainSceneIdTemplate;
        [SerializeField] TextAsset moduleSceneIdTemplate;

        [Header("Scene Script Generation")]
        [SerializeField, FolderOnly] DefaultAsset sceneScriptOutputDirectoryAsset;

        [Header("Scene Script Templates")]
        [SerializeField] SceneScriptTemplate[] sceneScriptTemplates = System.Array.Empty<SceneScriptTemplate>();

        public static string DefaultMainSceneTemplatePath =>
            $"{Lighthouse.Editor.Menu.LighthouseEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultMainSceneTemplate/DefaultMainSceneTemplate.asset";
        public static string DefaultModuleSceneTemplatePath =>
            $"{Lighthouse.Editor.Menu.LighthouseEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultModuleSceneTemplate/DefaultModuleSceneTemplate.asset";
        public static string DefaultMainSceneIdTemplatePath =>
            $"{Lighthouse.Editor.Menu.LighthouseEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultSceneIdTemplate.txt";
        public static string DefaultModuleSceneIdTemplatePath =>
            $"{Lighthouse.Editor.Menu.LighthouseEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultSceneIdTemplate.txt";

        string MainSceneIdFileName => $"{mainSceneIdPrefix}MainSceneId.g.cs";
        string ModuleSceneIdFileName => $"{mainSceneIdPrefix}ModuleSceneId.g.cs";

        public string ProductNameSpace => productNameSpace;

        public string GeneratedFileDirectory
        {
            get
            {
                if (generatedFileDirectoryAsset == null)
                {
                    return string.Empty;
                }

                var assetPath = AssetDatabase.GetAssetPath(generatedFileDirectoryAsset);
                return assetPath.StartsWith("Assets/") ? assetPath.Substring("Assets/".Length) : assetPath;
            }
        }

        public string MainSceneIdFilePath => Path.Combine(Application.dataPath, GeneratedFileDirectory, MainSceneIdFileName);
        public string SceneModuleIdIdFilePath => Path.Combine(Application.dataPath, GeneratedFileDirectory, ModuleSceneIdFileName);
        public string MainSceneIdPrefix => mainSceneIdPrefix;
        public string ModuleSceneIdPrefix => moduleSceneIdPrefix;
        public TextAsset MainSceneIdTemplate => mainSceneIdTemplate;
        public TextAsset ModuleSceneIdTemplate => moduleSceneIdTemplate;

        public string GeneratedMainSceneIdClassName => $"{mainSceneIdPrefix}MainSceneId";
        public string GeneratedModuleSceneIdClassName => $"{moduleSceneIdPrefix}ModuleSceneId";
        public string GeneratedSceneIdNamespace => $"{productNameSpace}.LighthouseGenerated";

        public string SceneScriptOutputDirectory
        {
            get
            {
                if (sceneScriptOutputDirectoryAsset == null)
                {
                    return string.Empty;
                }

                return AssetDatabase.GetAssetPath(sceneScriptOutputDirectoryAsset);
            }
        }

        public SceneScriptTemplate[] SceneScriptTemplates => sceneScriptTemplates;

        void Reset()
        {
            mainSceneIdTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultMainSceneIdTemplatePath);
            moduleSceneIdTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultModuleSceneIdTemplatePath);

            var mainTemplate = AssetDatabase.LoadAssetAtPath<SceneScriptTemplate>(DefaultMainSceneTemplatePath);
            var moduleTemplate = AssetDatabase.LoadAssetAtPath<SceneScriptTemplate>(DefaultModuleSceneTemplatePath);

            var templates = new System.Collections.Generic.List<SceneScriptTemplate>();
            if (mainTemplate != null)
            {
                templates.Add(mainTemplate);
            }

            if (moduleTemplate != null)
            {
                templates.Add(moduleTemplate);
            }
            sceneScriptTemplates = templates.ToArray();
        }

        public void InitializeDefaults(TextAsset mainSceneId, TextAsset moduleSceneId, SceneScriptTemplate[] templates)
        {
            mainSceneIdTemplate = mainSceneId;
            moduleSceneIdTemplate = moduleSceneId;
            sceneScriptTemplates = templates;
        }
    }
}
