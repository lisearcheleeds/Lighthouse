using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using LighthouseExtends.ScreenStack.Editor.ScriptableObject;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace LighthouseExtends.ScreenStack.Editor.ScriptGenerator
{
    public static class ScreenStackEntityFactoryGenerator
    {
        const string SettingsDefaultAssetPath = "Assets/Settings/ScreenStackGenerateSettings.asset";

        [DidReloadScripts]
        static void OnScriptsReloaded()
        {
            Generate();
        }

        [MenuItem("Lighthouse/Generate/Auto/Generate \"ScreenStackEntityFactory\" manually")]
        static void GenerateFromMenu()
        {
            Generate();
        }

        [MenuItem("Lighthouse/Settings/ScreenStackGenerateSettings")]
        static void ShowSettings()
        {
            var settings = LoadOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
            }
        }

        static void Generate()
        {
            var settings = LoadSettings();
            if (settings == null || string.IsNullOrEmpty(settings.ScreenStackEntityFactoryFilePath))
            {
                return;
            }

            if (settings.ScreenStackEntityFactoryTemplate == null)
            {
                Debug.LogWarning(
                    "[ScreenStackEntityFactoryGenerator] Template is not assigned in ScreenStackGenerateSettings.");
                return;
            }

            var screenStackDataInterface = typeof(IScreenStackData);
            var screenStackSetupOpenType = typeof(IScreenStackSetup<>);

            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .ToArray();

            var dataTypes = allTypes
                .Where(t => t.IsClass && !t.IsAbstract && (t.IsPublic || t.IsNestedPublic) && screenStackDataInterface.IsAssignableFrom(t))
                .ToArray();

            if (dataTypes.Length == 0)
            {
                return;
            }

            var mappings = new List<(Type Data, Type ScreenStack)>();

            foreach (var dataType in dataTypes)
            {
                var screenStackType = allTypes.FirstOrDefault(t =>
                    t.IsClass && !t.IsAbstract &&
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == screenStackSetupOpenType &&
                        i.GetGenericArguments()[0] == dataType));

                if (screenStackType == null)
                {
                    Debug.LogWarning(
                        $"[ScreenStackEntityFactoryGenerator] No IScreenStackSetup implementation found for {dataType.Name}. Skipped.");
                    continue;
                }

                mappings.Add((dataType, screenStackType));
            }

            if (mappings.Count == 0)
            {
                return;
            }

            var content = BuildContent(settings.ScreenStackEntityFactoryTemplate.text,
                settings.ScreenStackEntityFactoryClassName, settings.ScreenStackEntityFactoryNamespace, mappings);
            WriteContent(content, settings.ScreenStackEntityFactoryFilePath);
        }

        static string BuildContent(string template, string className, string namespaceName,
            List<(Type Data, Type ScreenStack)> mappings)
        {
            var extraNamespaces = mappings
                .SelectMany(m => new[] { m.Data.Namespace, m.ScreenStack.Namespace })
                .Where(ns => !string.IsNullOrEmpty(ns) && ns != namespaceName)
                .Distinct()
                .OrderBy(ns => ns)
                .ToArray();

            var extraUsings = extraNamespaces.Length > 0
                ? "\n" + string.Join("\n", extraNamespaces.Select(ns => $"using {ns};"))
                : string.Empty;

            var switchCases = string.Join("\n", mappings.Select(m =>
                $"                {m.Data.Name} d => CreateScreenStackEntityAsync<{m.ScreenStack.Name}, {m.Data.Name}>(\"{m.ScreenStack.Name}\", d, ct),"));

            return template
                .Replace("{{NAMESPACE}}", namespaceName)
                .Replace("{{CLASS_NAME}}", className)
                .Replace("{{EXTRA_USINGS}}", extraUsings)
                .Replace("{{SWITCH_CASES}}", switchCases)
                .Replace("\r\n", "\n");
        }

        static void WriteContent(string content, string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (File.Exists(filePath))
            {
                var existing = File.ReadAllText(filePath, Encoding.UTF8);
                if (existing == content)
                {
                    return;
                }
            }

            File.WriteAllText(filePath, content, new UTF8Encoding(false));

            var assetPath = "Assets" + filePath.Substring(Application.dataPath.Length).Replace('\\', '/');
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            Debug.Log("[ScreenStackEntityFactoryGenerator] Generated.");
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

        static ScreenStackGenerateSettings LoadOrCreateSettings()
        {
            var settings = LoadSettings();
            if (settings != null)
            {
                return settings;
            }

            var fsDirPath = Path.Combine(Application.dataPath, "Settings");
            if (!Directory.Exists(fsDirPath))
            {
                Directory.CreateDirectory(fsDirPath);
                AssetDatabase.Refresh();
            }

            settings = UnityEngine.ScriptableObject.CreateInstance<ScreenStackGenerateSettings>();
            AssetDatabase.CreateAsset(settings, SettingsDefaultAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
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
                    $"[ScreenStackEntityFactoryGenerator] Multiple {nameof(ScreenStackGenerateSettings)} found. Using the first one.");
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return (ScreenStackGenerateSettings)AssetDatabase.LoadAssetAtPath(path,
                typeof(ScreenStackGenerateSettings));
        }
    }
}
