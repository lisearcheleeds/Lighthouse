using UnityEditor;
using UnityEngine;

namespace LighthouseExtends.Animation.Editor
{
    [CustomEditor(typeof(LHSceneTransitionAnimatorManager))]
    public class LHSceneTransitionAnimatorManagerEditor : UnityEditor.Editor
    {
        SerializedProperty isAutoCollectProp;
        SerializedProperty sceneTransitionAnimatorListProp;

        void OnEnable()
        {
            isAutoCollectProp = serializedObject.FindProperty("isAutoCollect");
            sceneTransitionAnimatorListProp = serializedObject.FindProperty("sceneTransitionAnimatorList");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(isAutoCollectProp);

            EditorGUILayout.Space(4);

            using (new EditorGUI.DisabledScope(isAutoCollectProp.boolValue))
            {
                EditorGUILayout.PropertyField(sceneTransitionAnimatorListProp, true);
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Collect Animators"))
            {
                var manager = (LHSceneTransitionAnimatorManager)target;
                manager.CollectAnimators();
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
