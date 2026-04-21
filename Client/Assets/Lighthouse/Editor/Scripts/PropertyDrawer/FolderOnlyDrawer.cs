using UnityEditor;
using UnityEngine;

namespace Lighthouse.Editor.PropertyDrawer
{
    [CustomPropertyDrawer(typeof(FolderOnlyAttribute))]
    public class FolderOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var newObj = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(DefaultAsset), false);

            if (newObj == null)
            {
                property.objectReferenceValue = null;
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(newObj);
                if (AssetDatabase.IsValidFolder(path))
                    property.objectReferenceValue = newObj;
            }

            EditorGUI.EndProperty();
        }
    }
}
