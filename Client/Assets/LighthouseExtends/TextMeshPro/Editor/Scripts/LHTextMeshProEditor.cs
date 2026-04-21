using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace LighthouseExtends.TextMeshPro.Editor
{
    /// <summary>
    /// Custom Inspector for LHTextMeshPro. Extends TMP_EditorPanelUI to prepend
    /// a TextKey field above the standard TextMeshPro Inspector.
    /// </summary>
    [CustomEditor(typeof(LHTextMeshPro))]
    [CanEditMultipleObjects]
    public class LHTextMeshProEditor : TMP_EditorPanelUI
    {
        SerializedProperty textKeyProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            textKeyProperty = serializedObject.FindProperty("textKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("TextTable", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(textKeyProperty, new GUIContent("Text Key"));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
    }
}
