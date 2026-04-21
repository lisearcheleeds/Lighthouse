using System.IO;
using System.Linq;
using System.Text;
using Lighthouse.Editor.Menu;
using Lighthouse.Editor.ScriptableObject;
using UnityEditor;
using UnityEngine;
using Lighthouse;

namespace Lighthouse.Editor.ScriptGenerator
{
    /// <summary>
    /// This is an EditorScript that generates MainSceneId and ModuleSceneId.
    /// The scene can be automatically called when it is added to Unity's scene list, or manually called from the menu.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneIdGenerator
    {
        const string DefaultTemplatePath = "Assets/Lighthouse/Editor/Scripts/ScriptGenerator/SceneIdTemplate.txt";

        static SceneIdGenerator()
        {
            EditorBuildSettings.sceneListChanged += OnSceneListChanged;
        }

        static void OnSceneListChanged()
        {
            GenerateMainSceneId();
            GenerateModuleSceneId();
        }

        [MenuItem("Lighthouse/Generate/Auto/Generate \"MainSceneId\" manually")]
        static void GenerateMainSceneId()
        {
            var generateSettings = LighthouseEditor.GetOrCreateSettings<GenerateSettings>();

            if (string.IsNullOrEmpty(generateSettings.MainSceneIdFilePath))
            {
                Debug.Log("GenerateSettings.MainSceneIdFilePath is Empty");
                return;
            }

            GenerateSceneId("MainScene", generateSettings.MainSceneIdPrefix, generateSettings.MainSceneIdFilePath,
                generateSettings.GeneratedSceneIdNamespace, generateSettings.MainSceneIdTemplate);
        }

        [MenuItem("Lighthouse/Generate/Auto/Generate \"ModuleSceneId\" manually")]
        static void GenerateModuleSceneId()
        {
            var generateSettings = LighthouseEditor.GetOrCreateSettings<GenerateSettings>();

            if (string.IsNullOrEmpty(generateSettings.SceneModuleIdIdFilePath))
            {
                Debug.Log("GenerateSettings.SceneModuleIdIdFilePath is Empty");
                return;
            }

            GenerateSceneId("ModuleScene", generateSettings.ModuleSceneIdPrefix,
                generateSettings.SceneModuleIdIdFilePath, generateSettings.GeneratedSceneIdNamespace,
                generateSettings.ModuleSceneIdTemplate);
        }

        static void GenerateSceneId(string sceneIdType, string prefix, string outputPath, string nameSpace,
            TextAsset templateOverride)
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .Where(p => !string.IsNullOrWhiteSpace(p) && p.Contains($"{sceneIdType}"))
                .Distinct()
                .ToArray();

            var template = LoadTemplate(templateOverride);
            if (template == null)
            {
                LHLogger.LogError("[SceneIdGenerator] Failed to load template.");
                return;
            }

            var content = BuildContent(sceneIdType, prefix, scenes, nameSpace, template);
            var result = WriteContent(content, outputPath);
            if (!result)
            {
                LHLogger.LogError("[SceneIdGenerator] Failed to generate SceneID");
                return;
            }

            var assetPath = FsPathToAssetPath(outputPath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            LHLogger.Log("[SceneIdGenerator] Generated.");
        }

        static string LoadTemplate(TextAsset templateOverride)
        {
            if (templateOverride != null)
            {
                return templateOverride.text;
            }

            var defaultTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultTemplatePath);
            if (defaultTemplate != null)
            {
                return defaultTemplate.text;
            }

            LHLogger.LogError($"[SceneIdGenerator] Default template not found at: {DefaultTemplatePath}");
            return null;
        }

        static string BuildContent(string sceneIdType, string prefix, string[] sceneNames, string nameSpace,
            string template)
        {
            if (sceneNames.Length == 0)
            {
                LHLogger.LogError("[SceneIdGenerator] No enabled scenes found in Build Settings.");
            }

            var sceneIdTypeName = $"{sceneIdType}Id";
            var className = $"{prefix}{sceneIdTypeName}";
            var membersBlock = BuildMembersBlock(sceneIdTypeName, sceneNames);
            var allBlock = BuildAllBlock(sceneNames);

            return template
                .Replace("{{NAMESPACE}}", nameSpace)
                .Replace("{{CLASS_NAME}}", className)
                .Replace("{{SCENE_ID_TYPE}}", sceneIdTypeName)
                .Replace("{{MEMBERS_BLOCK}}", membersBlock)
                .Replace("{{ALL_BLOCK}}", allBlock);
        }

        static string BuildMembersBlock(string sceneIdTypeName, string[] sceneNames)
        {
            var sb = new StringBuilder();
            byte id = 1;

            sb.AppendLine(
                $"        public static readonly {sceneIdTypeName} None = new {sceneIdTypeName}({id}, string.Empty);");
            id++;

            foreach (var sceneName in sceneNames)
            {
                var validSceneName = Path.GetFileNameWithoutExtension(sceneName);
                var identifier = SanitizeIdentifier(validSceneName);
                sb.AppendLine(
                    $"        public static readonly {sceneIdTypeName} {identifier} = new {sceneIdTypeName}({id}, \"{validSceneName}\");");
                id++;
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        static string BuildAllBlock(string[] sceneNames)
        {
            var sb = new StringBuilder();
            sb.AppendLine("                    None,");

            foreach (var sceneName in sceneNames)
            {
                var validSceneName = Path.GetFileNameWithoutExtension(sceneName);
                var identifier = SanitizeIdentifier(validSceneName);
                sb.AppendLine($"                    {identifier},");
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        static bool WriteContent(string content, string sceneIdFilePath)
        {
            var directoryPath = Path.GetDirectoryName(sceneIdFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (File.Exists(sceneIdFilePath))
            {
                var existing = File.ReadAllText(sceneIdFilePath, Encoding.UTF8);
                if (existing == content)
                {
                    return true;
                }
            }

            File.WriteAllText(sceneIdFilePath, content, new UTF8Encoding(false));
            return true;
        }

        static string FsPathToAssetPath(string fsPath)
        {
            var normalized = fsPath.Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');
            if (normalized.StartsWith(dataPath))
            {
                return "Assets" + normalized.Substring(dataPath.Length);
            }

            return fsPath;
        }

        static string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "_Scene";
            }

            var sb = new StringBuilder(name.Length);

            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];

                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }

            if (char.IsDigit(sb[0]))
            {
                sb.Insert(0, '_');
            }

            return sb.ToString();
        }
    }
}
