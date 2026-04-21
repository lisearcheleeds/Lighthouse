using System.Text;
using UnityEditor;

namespace LighthouseExtends.ScreenStack.Editor.ScriptableObject
{
    [CustomEditor(typeof(ScreenStackScriptTemplate))]
    public class ScreenStackScriptTemplateEditor : UnityEditor.Editor
    {
        static readonly (string Key, string Description)[] Replacements =
        {
            ("{{NAMESPACE}}",            "Namespace of the output file"),
            ("{{DIALOG_NAME}}",          "Dialog name (e.g. Sample)"),
            ("{{DIALOG_NAME_CAMEL}}",    "Dialog name in camelCase (e.g. sample)"),
            ("{{BASE_CLASS}}",           "Base class name"),
            ("{{BASE_CLASS_NAMESPACE}}", "Namespace of the base class"),
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
            sb.AppendLine("e.g. #OUTPUT {{DIALOG_NAME}}Dialog.cs");
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
