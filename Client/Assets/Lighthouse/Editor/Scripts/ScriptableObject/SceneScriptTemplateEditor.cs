using System.Text;
using UnityEditor;
using UnityEngine;

namespace Lighthouse.Editor.ScriptableObject
{
    [CustomEditor(typeof(SceneScriptTemplate))]
    public class SceneScriptTemplateEditor : UnityEditor.Editor
    {
        static readonly (string Key, string Description)[] Replacements =
        {
            ("{{NAMESPACE}}",                    "Namespace of the output file"),
            ("{{SCENE_NAME}}",                   "Scene name (e.g. Sample)"),
            ("{{SCENE_NAME_CAMEL}}",             "Scene name in camelCase (e.g. sample)"),
            ("{{SCENE_FILE_NAME}}",              "Scene file name (e.g. SampleScene / SampleModuleScene)"),
            ("{{SCENE_FILE_NAME_CAMEL}}",        "Scene file name in camelCase (e.g. sampleScene)"),
            ("{{BASE_CLASS}}",                   "Base class name"),
            ("{{BASE_CLASS_NAMESPACE}}",         "Namespace of the base class"),
            ("{{SCENE_ID_TYPE}}",                "SceneId type name (MainSceneId / ModuleSceneId)"),
            ("{{SCENE_ID_CLASS}}",               "Generated SceneId class name"),
            ("{{GENERATED_SCENE_ID_NAMESPACE}}", "Namespace of the generated SceneId class"),
        };

        bool showReplacementInfo = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            showReplacementInfo = EditorGUILayout.Foldout(showReplacementInfo, "Replacement Info", true, EditorStyles.foldoutHeader);
            if (!showReplacementInfo)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[#OUTPUT Directive]");
            sb.AppendLine("Write on the first line of each template file.");
            sb.AppendLine("Specifies the output file name. Placeholders are supported.");
            sb.AppendLine("e.g. #OUTPUT {{SCENE_NAME}}LifetimeScope.cs");
            sb.AppendLine();
            sb.AppendLine("[Available Placeholders]");
            foreach (var (key, description) in Replacements)
            {
                sb.AppendLine($"{key}");
                sb.AppendLine($"  -> {description}");
            }

            EditorGUILayout.HelpBox(sb.ToString().TrimEnd(), MessageType.Info);
        }
    }
}
