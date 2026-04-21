using System.IO;
using Lighthouse.Editor.PropertyDrawer;
using LighthouseExtends.ScreenStack.Editor.ScriptGenerator;
using UnityEditor;
using UnityEngine;

namespace LighthouseExtends.ScreenStack.Editor.ScriptableObject
{
    public class ScreenStackGenerateSettings : UnityEngine.ScriptableObject
    {
        [SerializeField, FolderOnly] DefaultAsset screenStackEntityFactoryDirectoryAsset;
        [SerializeField] string screenStackEntityFactoryClassName = "ScreenStackEntityFactory";
        [SerializeField] string screenStackEntityFactoryNamespace = "YourProduct";
        [SerializeField] TextAsset screenStackEntityFactoryTemplate;

        [Header("ScreenStack Dialog Script Generation")]
        [SerializeField, FolderOnly] DefaultAsset screenStackDialogScriptOutputDirectoryAsset;
        [SerializeField] string screenStackDialogScriptNamespace = "YourProduct";

        [Header("ScreenStack Dialog Script Templates")]
        [SerializeField] ScreenStackScriptTemplate[] screenStackScriptTemplates = System.Array.Empty<ScreenStackScriptTemplate>();

        public static string DefaultDialogTemplatePath =>
            $"{ScreenStackEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultDialogTemplate/DefaultDialogTemplate.asset";
        public static string DefaultDialogTemplateDir =>
            $"{ScreenStackEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/DefaultTemplates/DefaultDialogTemplate";
        static string DefaultEntityFactoryTemplatePath =>
            $"{ScreenStackEditorPaths.PackageBasePath}/Editor/Scripts/ScriptGenerator/ScreenStackEntityFactoryTemplate.txt";

        string ScreenStackEntityFactoryDirectory
        {
            get
            {
                if (screenStackEntityFactoryDirectoryAsset == null)
                {
                    return string.Empty;
                }

                var assetPath = AssetDatabase.GetAssetPath(screenStackEntityFactoryDirectoryAsset);
                return assetPath.StartsWith("Assets/") ? assetPath.Substring("Assets/".Length) : assetPath;
            }
        }

        public string ScreenStackEntityFactoryFilePath =>
            string.IsNullOrEmpty(ScreenStackEntityFactoryDirectory)
                ? string.Empty
                : Path.Combine(Application.dataPath, ScreenStackEntityFactoryDirectory,
                    $"{screenStackEntityFactoryClassName}.g.cs");

        public string ScreenStackEntityFactoryClassName => screenStackEntityFactoryClassName;
        public string ScreenStackEntityFactoryNamespace => screenStackEntityFactoryNamespace;
        public TextAsset ScreenStackEntityFactoryTemplate => screenStackEntityFactoryTemplate;

        public string ScreenStackDialogScriptOutputDirectory
        {
            get
            {
                if (screenStackDialogScriptOutputDirectoryAsset == null)
                {
                    return string.Empty;
                }

                return AssetDatabase.GetAssetPath(screenStackDialogScriptOutputDirectoryAsset);
            }
        }

        public string ScreenStackDialogScriptNamespace => screenStackDialogScriptNamespace;
        public ScreenStackScriptTemplate[] ScreenStackScriptTemplates => screenStackScriptTemplates;

        void Reset()
        {
            screenStackEntityFactoryTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultEntityFactoryTemplatePath);

            var dialogTemplate = AssetDatabase.LoadAssetAtPath<ScreenStackScriptTemplate>(DefaultDialogTemplatePath);
            screenStackScriptTemplates = dialogTemplate != null
                ? new[] { dialogTemplate }
                : System.Array.Empty<ScreenStackScriptTemplate>();
        }

        public void InitializeEntityFactoryTemplateDefaults()
        {
            screenStackEntityFactoryTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultEntityFactoryTemplatePath);
        }

        public void InitializeDefaults(ScreenStackScriptTemplate[] templates)
        {
            InitializeEntityFactoryTemplateDefaults();
            screenStackScriptTemplates = templates;
        }
    }
}
