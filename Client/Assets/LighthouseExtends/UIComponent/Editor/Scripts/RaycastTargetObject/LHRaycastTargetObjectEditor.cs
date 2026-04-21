using LighthouseExtends.UIComponent.RaycastTargetObject;
using UnityEditor;
using UnityEngine;

namespace LighthouseExtends.UIComponent.Editor.RaycastTargetObject
{
    [CustomEditor(typeof(LHRaycastTargetObject))]
    [CanEditMultipleObjects]
    public class LHRaycastTargetObjectEditor : UnityEditor.Editor
    {
        protected SerializedProperty isRaycastTargetProperty;
        protected SerializedProperty raycastPaddingProperty;

        protected LHRaycastTargetObject raycastTargetObject;
        protected RectTransform raycastTargetRectTransform;

        protected virtual void OnEnable()
        {
            isRaycastTargetProperty = serializedObject.FindProperty("m_RaycastTarget");
            raycastPaddingProperty = serializedObject.FindProperty("m_RaycastPadding");

            raycastTargetObject = (LHRaycastTargetObject)target;
            raycastTargetObject.color = Color.clear;

            raycastTargetRectTransform = raycastTargetObject.GetComponent<RectTransform>();
        }

        protected virtual void OnSceneGUI()
        {
            if (Selection.objects.Length > 1)
            {
                return;
            }

            DrawArea();
        }

        public override void OnInspectorGUI()
        {
            if (raycastTargetObject == null || raycastTargetRectTransform == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.PropertyField(isRaycastTargetProperty);
            EditorGUILayout.PropertyField(raycastPaddingProperty);

            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndChangeCheck();
        }

        protected void DrawArea()
        {
            var size = raycastTargetRectTransform.rect.size;
            size.x *= raycastTargetRectTransform.lossyScale.x;
            size.y *= raycastTargetRectTransform.lossyScale.y;

            var leftTop = raycastTargetRectTransform.position;
            leftTop.x -= size.x * raycastTargetRectTransform.pivot.x;
            leftTop.y += size.y * (1.0f - raycastTargetRectTransform.pivot.y);

            var rightTop = raycastTargetRectTransform.position;
            rightTop.x += size.x * (1.0f - raycastTargetRectTransform.pivot.x);
            rightTop.y += size.y * (1.0f - raycastTargetRectTransform.pivot.y);

            var rightBottom = raycastTargetRectTransform.position;
            rightBottom.x += size.x * (1.0f - raycastTargetRectTransform.pivot.x);
            rightBottom.y -= size.y * raycastTargetRectTransform.pivot.y;

            var leftBottom = raycastTargetRectTransform.position;
            leftBottom.x -= size.x * raycastTargetRectTransform.pivot.x;
            leftBottom.y -= size.y * raycastTargetRectTransform.pivot.y;

            var temp = Handles.color;
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(3.0f, leftTop, rightTop, rightBottom, leftBottom, leftTop);
            Handles.color = temp;
        }
    }
}