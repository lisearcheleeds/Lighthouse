using System;
using UnityEditor;
using UnityEngine;

namespace LighthouseExtends.TextTable.Editor
{
    /// <summary>
    /// PopupWindow for entering a language code and adding a new TSV file.
    /// Opened by the Add Language button in TextTableEditorWindow.
    /// </summary>
    public class AddLanguagePopup : PopupWindowContent
    {
        readonly Action<string> onConfirm;
        bool focusRequested;
        string languageCode = string.Empty;

        public AddLanguagePopup(Action<string> onConfirm)
        {
            this.onConfirm = onConfirm;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(240, 68);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField("Language code (e.g. ja, en):");

            GUI.SetNextControlName("LangCodeField");
            languageCode = EditorGUILayout.TextField(languageCode);

            if (!focusRequested)
            {
                EditorGUI.FocusTextInControl("LangCodeField");
                focusRequested = true;
            }

            EditorGUILayout.Space(2);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(languageCode)))
                {
                    if (GUILayout.Button("OK"))
                    {
                        Confirm();
                    }
                }

                if (GUILayout.Button("Cancel"))
                {
                    editorWindow.Close();
                }
            }

            if (Event.current.type == EventType.KeyDown &&
                Event.current.keyCode == KeyCode.Return &&
                !string.IsNullOrWhiteSpace(languageCode))
            {
                Confirm();
                Event.current.Use();
            }
        }

        void Confirm()
        {
            onConfirm?.Invoke(languageCode.Trim());
            editorWindow.Close();
        }
    }
}
