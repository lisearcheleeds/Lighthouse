using Lighthouse.Editor.PropertyDrawer;
using UnityEditor;
using UnityEngine;

namespace LighthouseExtends.ScreenStack.Editor.ScriptableObject
{
    [CreateAssetMenu(fileName = "ScreenStackScriptTemplate", menuName = "Lighthouse/ScreenStack Script Template")]
    public class ScreenStackScriptTemplate : UnityEngine.ScriptableObject
    {
        [SerializeField] string templateName = "Default";
        [SerializeField, FolderOnly] DefaultAsset templateDirectory;

        public string TemplateName => templateName;

        public string TemplateDirectoryPath
        {
            get
            {
                if (templateDirectory == null)
                {
                    return string.Empty;
                }

                return AssetDatabase.GetAssetPath(templateDirectory);
            }
        }

        public void InitializeDefaults(string name, string templateDirAssetPath)
        {
            templateName = name;
            templateDirectory = AssetDatabase.LoadAssetAtPath<DefaultAsset>(templateDirAssetPath);
        }
    }
}
