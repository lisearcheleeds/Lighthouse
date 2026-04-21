using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LighthouseExtends.ScreenStack.Editor.ScriptableObject;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace LighthouseExtends.ScreenStack.Editor.ScriptGenerator
{
    public class ScreenStackDialogBaseClassInfo
    {
        public string TypeName { get; set; }
        public string Namespace { get; set; }

        public string DropdownLabel => $"{TypeName}  ({Namespace})";
    }

    /// <summary>
    /// This is an EditorScript that generates the ScreenStack dialog script.
    ///
    /// Placeholders available in template files and #OUTPUT directives:
    /// {{NAMESPACE}}            - Namespace of the output file
    /// {{DIALOG_NAME}}          - Dialog name (e.g. Sample)
    /// {{DIALOG_NAME_CAMEL}}    - Dialog name in camelCase (e.g. sample)
    /// {{BASE_CLASS}}           - Base class name
    /// {{BASE_CLASS_NAMESPACE}} - Namespace of the base class
    /// </summary>
    public static class ScreenStackDialogScriptGenerator
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

            var loadedNames =
                new HashSet<string>(AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name));

            return compiledNames.Any(name => !loadedNames.Contains(name));
        }

        public static ScreenStackDialogBaseClassInfo[] CollectBaseClasses()
        {
            var rootType = typeof(ScreenStackBase);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(t => t.IsAbstract && !t.IsInterface)
                .Where(t => t == rootType || InheritsFrom(t, rootType))
                .Select(t => new ScreenStackDialogBaseClassInfo
                {
                    TypeName = t.Name,
                    Namespace = t.Namespace ?? string.Empty
                })
                .OrderBy(b => b.Namespace)
                .ThenBy(b => b.TypeName)
                .ToArray();
        }

        public static void GenerateFiles(string dialogName, ScreenStackDialogBaseClassInfo baseClass,
            ScreenStackGenerateSettings settings, ScreenStackScriptTemplate template)
        {
            if (settings == null)
            {
                Debug.LogError("[ScreenStackDialogScriptGenerator] ScreenStackGenerateSettings is null.");
                return;
            }

            if (template == null)
            {
                Debug.LogError("[ScreenStackDialogScriptGenerator] ScreenStackScriptTemplate is null.");
                return;
            }

            var outputRoot = settings.ScreenStackDialogScriptOutputDirectory;
            if (string.IsNullOrEmpty(outputRoot))
            {
                Debug.LogError(
                    "[ScreenStackDialogScriptGenerator] Output directory is not configured in ScreenStackGenerateSettings.");
                return;
            }

            var templateDirPath = template.TemplateDirectoryPath;
            if (string.IsNullOrEmpty(templateDirPath))
            {
                Debug.LogError("[ScreenStackDialogScriptGenerator] Template directory is not configured in ScreenStackScriptTemplate.");
                return;
            }

            var namespaceName = string.IsNullOrEmpty(settings.ScreenStackDialogScriptNamespace)
                ? dialogName
                : $"{settings.ScreenStackDialogScriptNamespace}.{dialogName}";

            // Placeholders available in template files and #OUTPUT directives:
            // {{NAMESPACE}}            - Namespace of the output file
            // {{DIALOG_NAME}}          - Dialog name (e.g. Sample)
            // {{DIALOG_NAME_CAMEL}}    - Dialog name in camelCase (e.g. sample)
            // {{BASE_CLASS}}           - Base class name
            // {{BASE_CLASS_NAMESPACE}} - Namespace of the base class
            var replacements = new Dictionary<string, string>
            {
                { "{{NAMESPACE}}", namespaceName },
                { "{{DIALOG_NAME}}", dialogName },
                { "{{DIALOG_NAME_CAMEL}}", ToCamelCase(dialogName) },
                { "{{BASE_CLASS}}", baseClass.TypeName },
                { "{{BASE_CLASS_NAMESPACE}}", baseClass.Namespace }
            };

            var outputAssetDir = $"{outputRoot}/{dialogName}";
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
                    Debug.LogWarning($"[ScreenStackDialogScriptGenerator] Template '{Path.GetFileName(txtFilePath)}' is empty or has no newline. Skipping.");
                    continue;
                }

                var firstLine = allText.Substring(0, firstNewlineIndex).TrimEnd('\r');
                if (!firstLine.StartsWith(OutputDirective))
                {
                    Debug.LogWarning($"[ScreenStackDialogScriptGenerator] Template '{Path.GetFileName(txtFilePath)}' is missing '{OutputDirective}' directive on the first line. Skipping.");
                    continue;
                }

                var outputFileNamePattern = firstLine.Substring(OutputDirective.Length).Trim();
                var outputFileName = replacements.Aggregate(outputFileNamePattern, (current, pair) => current.Replace(pair.Key, pair.Value));
                var templateContent = allText.Substring(firstNewlineIndex + 1);

                var outputAssetPath = $"{outputAssetDir}/{outputFileName}";
                WriteFromTemplate(templateContent, replacements, AssetPathToFsPath(outputAssetPath));
                AssetDatabase.ImportAsset(outputAssetPath, ImportAssetOptions.ForceSynchronousImport);
            }

            Debug.Log(
                $"[ScreenStackDialogScriptGenerator] Generated dialog scripts for '{dialogName}' at {outputAssetDir}");
        }

        public static string GetTemplateValidationError(ScreenStackGenerateSettings settings)
        {
            if (settings.ScreenStackScriptTemplates == null || settings.ScreenStackScriptTemplates.Length == 0)
            {
                return "No script templates are configured in ScreenStackGenerateSettings.\n"
                       + "Please add at least one ScreenStackScriptTemplate under \"ScreenStack Dialog Script Templates\".";
            }

            return null;
        }

        public static string GetTemplateValidationError(ScreenStackScriptTemplate template)
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

                Debug.LogWarning($"[ScreenStackDialogScriptGenerator] Overwriting: {Path.GetFileName(fsPath)}");
            }

            File.WriteAllText(fsPath, content, new UTF8Encoding(false));
        }

        static string AssetPathToFsPath(string assetPath) => ScreenStackEditorPaths.AssetPathToFsPath(assetPath);

        static bool InheritsFrom(Type type, Type baseType)
        {
            var current = type.BaseType;
            while (current != null && current != typeof(object))
            {
                if (current == baseType)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
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
