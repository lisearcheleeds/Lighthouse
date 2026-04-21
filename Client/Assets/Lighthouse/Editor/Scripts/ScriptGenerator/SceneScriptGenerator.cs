using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lighthouse.Editor.ScriptableObject;
using Lighthouse.Scene.SceneBase;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEngine;
using Lighthouse;
using Assembly = System.Reflection.Assembly;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Lighthouse.Editor.ScriptGenerator
{
    /// <summary>
    /// This is an EditorScript that generates the scene's script.
    ///
    /// Placeholders available in template files and #OUTPUT directives:
    /// {{NAMESPACE}}                    - Namespace of the output file
    /// {{SCENE_NAME}}                   - Scene name (e.g. Sample)
    /// {{SCENE_NAME_CAMEL}}             - Scene name in camelCase (e.g. sample)
    /// {{SCENE_FILE_NAME}}              - Scene file name (e.g. SampleScene / SampleModuleScene)
    /// {{SCENE_FILE_NAME_CAMEL}}        - Scene file name in camelCase (e.g. sampleScene)
    /// {{BASE_CLASS}}                   - Base class name
    /// {{BASE_CLASS_NAMESPACE}}         - Namespace of the base class
    /// {{SCENE_ID_TYPE}}               - SceneId type name (MainSceneId / ModuleSceneId)
    /// {{SCENE_ID_CLASS}}               - Generated SceneId class name
    /// {{GENERATED_SCENE_ID_NAMESPACE}} - Namespace of the generated SceneId class
    /// </summary>
    public static class SceneScriptGenerator
    {
        const string OutputDirective = "#OUTPUT ";

        public static bool IsCompiling => EditorApplication.isCompiling;

        public static bool HasCompileErrors()
        {
            if (EditorApplication.isCompiling)
            {
                return false;
            }

            var compiledNames = new HashSet<string>(
                CompilationPipeline.GetAssemblies(AssembliesType.Editor)
                    .Concat(CompilationPipeline.GetAssemblies(AssembliesType.Player))
                    .Select(a => a.name));

            var loadedNames = new HashSet<string>(AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name));

            return compiledNames.Any(name => !loadedNames.Contains(name));
        }

        public static GenerateBaseClassInfo[] CollectBaseClasses(SceneType sceneType)
        {
            var rootType = sceneType == SceneType.MainScene
                ? typeof(MainSceneBase)
                : typeof(ModuleSceneBase);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(t => t.IsAbstract && !t.IsInterface)
                .Where(t => t == rootType || InheritsFrom(t, rootType))
                .Select(t => new GenerateBaseClassInfo
                {
                    TypeName = CleanTypeName(t.Name),
                    Namespace = t.Namespace ?? string.Empty,
                    IsGeneric = t.IsGenericTypeDefinition
                })
                .OrderBy(b => b.Namespace)
                .ThenBy(b => b.TypeName)
                .ToArray();
        }

        public static void GenerateFiles(string sceneName, SceneType sceneType, GenerateBaseClassInfo generateBaseClass, GenerateSettings settings, SceneScriptTemplate template)
        {
            if (settings == null)
            {
                LHLogger.LogError("[SceneScriptGenerator] GenerateSettings is null.");
                return;
            }

            if (template == null)
            {
                LHLogger.LogError("[SceneScriptGenerator] SceneScriptTemplate is null.");
                return;
            }

            var outputRoot = settings.SceneScriptOutputDirectory;
            if (string.IsNullOrEmpty(outputRoot))
            {
                LHLogger.LogError("[SceneScriptGenerator] Output directory is not configured in GenerateSettings.");
                return;
            }

            var templateDirPath = template.TemplateDirectoryPath;
            if (string.IsNullOrEmpty(templateDirPath))
            {
                LHLogger.LogError("[SceneScriptGenerator] Template directory is not configured in SceneScriptTemplate.");
                return;
            }

            var sceneTypeName = sceneType == SceneType.MainScene ? "MainScene" : "ModuleScene";
            var sceneFileName = sceneType == SceneType.MainScene ? $"{sceneName}Scene" : $"{sceneName}ModuleScene";

            // Placeholders available in template files and #OUTPUT directives:
            // {{NAMESPACE}}                    - Namespace of the output file
            // {{SCENE_NAME}}                   - Scene name (e.g. Sample)
            // {{SCENE_NAME_CAMEL}}             - Scene name in camelCase (e.g. sample)
            // {{SCENE_FILE_NAME}}              - Scene file name (e.g. SampleScene / SampleModuleScene)
            // {{SCENE_FILE_NAME_CAMEL}}        - Scene file name in camelCase (e.g. sampleScene)
            // {{BASE_CLASS}}                   - Base class name
            // {{BASE_CLASS_NAMESPACE}}         - Namespace of the base class
            // {{SCENE_ID_TYPE}}               - SceneId type name (MainSceneId / ModuleSceneId)
            // {{SCENE_ID_CLASS}}               - Generated SceneId class name
            // {{GENERATED_SCENE_ID_NAMESPACE}} - Namespace of the generated SceneId class
            var replacements = new Dictionary<string, string>
            {
                { "{{NAMESPACE}}", $"{settings.ProductNameSpace}.View.Scene.{sceneTypeName}.{sceneName}" },
                { "{{SCENE_NAME}}", sceneName },
                { "{{SCENE_NAME_CAMEL}}", ToCamelCase(sceneName) },
                { "{{SCENE_FILE_NAME}}", sceneFileName },
                { "{{SCENE_FILE_NAME_CAMEL}}", ToCamelCase(sceneFileName) },
                { "{{BASE_CLASS}}", generateBaseClass.GetBaseClassExpression(sceneFileName, sceneName) },
                { "{{BASE_CLASS_NAMESPACE}}", generateBaseClass.Namespace },
                { "{{SCENE_ID_TYPE}}", sceneType == SceneType.MainScene ? "MainSceneId" : "ModuleSceneId" },
                {
                    "{{SCENE_ID_CLASS}}",
                    sceneType == SceneType.MainScene
                        ? settings.GeneratedMainSceneIdClassName
                        : settings.GeneratedModuleSceneIdClassName
                },
                { "{{GENERATED_SCENE_ID_NAMESPACE}}", settings.GeneratedSceneIdNamespace }
            };

            var outputAssetDir = $"{outputRoot}/{sceneTypeName}/{sceneName}";
            var outputFsDir = AssetPathToFsPath(outputAssetDir);

            if (!Directory.Exists(outputFsDir))
            {
                Directory.CreateDirectory(outputFsDir);
            }

            var templateFsDir = AssetPathToFsPath(templateDirPath);
            var txtFiles = Directory.GetFiles(templateFsDir, "*.txt", SearchOption.TopDirectoryOnly);

            foreach (var txtFilePath in txtFiles)
            {
                var allText = File.ReadAllText(txtFilePath, Encoding.UTF8);
                var firstNewlineIndex = allText.IndexOf('\n');
                if (firstNewlineIndex < 0)
                {
                    LHLogger.LogWarning($"[SceneScriptGenerator] Template '{Path.GetFileName(txtFilePath)}' is empty or has no newline. Skipping.");
                    continue;
                }

                var firstLine = allText.Substring(0, firstNewlineIndex).TrimEnd('\r');
                if (!firstLine.StartsWith(OutputDirective))
                {
                    LHLogger.LogWarning($"[SceneScriptGenerator] Template '{Path.GetFileName(txtFilePath)}' is missing '{OutputDirective}' directive on the first line. Skipping.");
                    continue;
                }

                var outputFileNamePattern = firstLine.Substring(OutputDirective.Length).Trim();
                var outputFileName = replacements.Aggregate(outputFileNamePattern, (current, pair) => current.Replace(pair.Key, pair.Value));
                var templateContent = allText.Substring(firstNewlineIndex + 1);

                var outputAssetPath = $"{outputAssetDir}/{outputFileName}";
                WriteFromTemplate(templateContent, replacements, AssetPathToFsPath(outputAssetPath));
                AssetDatabase.ImportAsset(outputAssetPath, ImportAssetOptions.ForceSynchronousImport);
            }

            LHLogger.Log($"[SceneScriptGenerator] Generated scene scripts for '{sceneName}' at {outputAssetDir}");
        }

        public static string GetTemplateValidationError(GenerateSettings settings)
        {
            if (settings.SceneScriptTemplates == null || settings.SceneScriptTemplates.Length == 0)
            {
                return "No scene script templates are configured in GenerateSettings.\n"
                       + "Please add at least one SceneScriptTemplate under \"Scene Script Templates\".";
            }

            return null;
        }

        public static string GetTemplateValidationError(SceneScriptTemplate template)
        {
            if (template == null)
            {
                return "Selected template is null.";
            }

            var dirPath = template.TemplateDirectoryPath;
            if (string.IsNullOrEmpty(dirPath))
            {
                return $"Template directory is not configured in \"{template.TemplateName}\".";
            }

            var fsDirPath = AssetPathToFsPath(dirPath);
            if (!Directory.Exists(fsDirPath))
            {
                return $"Template directory does not exist: {dirPath}";
            }

            var txtFiles = Directory.GetFiles(fsDirPath, "*.txt", SearchOption.TopDirectoryOnly);
            if (txtFiles.Length == 0)
            {
                return $"No .txt template files found in \"{dirPath}\".";
            }

            return null;
        }

        static void WriteFromTemplate(string template, Dictionary<string, string> replacements, string fsPath)
        {
            if (string.IsNullOrEmpty(template))
            {
                return;
            }

            var content = replacements.Aggregate(template, (current, pair) => current.Replace(pair.Key, pair.Value));

            if (File.Exists(fsPath))
            {
                var existing = File.ReadAllText(fsPath, Encoding.UTF8);
                if (existing == content)
                {
                    return;
                }

                LHLogger.LogWarning($"[SceneScriptGenerator] Overwriting: {Path.GetFileName(fsPath)}");
            }

            File.WriteAllText(fsPath, content, new UTF8Encoding(false));
        }

        static string AssetPathToFsPath(string assetPath)
        {
            if (assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)));
            }

            if (assetPath.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
            {
                var info = PackageInfo.FindForAssetPath(assetPath);
                if (info != null)
                {
                    var relative = assetPath.Substring($"Packages/{info.name}/".Length);
                    return Path.GetFullPath(Path.Combine(info.resolvedPath, relative));
                }
            }

            return Path.GetFullPath(Path.Combine(Application.dataPath, assetPath));
        }

        // Traverse BaseType chain handling open generic types
        static bool InheritsFrom(Type type, Type baseType)
        {
            var current = type.BaseType;
            while (current != null && current != typeof(object))
            {
                var normalized = current.IsGenericType
                    ? current.GetGenericTypeDefinition()
                    : current;
                if (normalized == baseType)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        // Remove generic arity suffix (e.g. "MainSceneBase`1" -> "MainSceneBase")
        static string CleanTypeName(string name)
        {
            var backtick = name.IndexOf('`');
            return backtick >= 0 ? name.Substring(0, backtick) : name;
        }

        static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }
    }
}
