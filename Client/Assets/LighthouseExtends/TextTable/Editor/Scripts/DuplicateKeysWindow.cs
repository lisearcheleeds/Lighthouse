using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LighthouseExtends.TextTable.Editor
{
    /// <summary>
    /// EditorWindow that lists TextKeys duplicated across multiple TSV scopes.
    /// Opened by the Check Duplicates button in TextTableEditorWindow.
    /// </summary>
    public class DuplicateKeysWindow : EditorWindow
    {
        List<(string key, HashSet<string> scopes)> duplicates;
        Vector2 scroll;

        void OnGUI()
        {
            EditorGUILayout.LabelField(
                $"{duplicates.Count} duplicate key(s) found across scopes:",
                EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach (var (key, scopes) in duplicates)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(key, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(
                        $"Scopes: {string.Join(", ", scopes.OrderBy(s => s))}",
                        EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        public static void Show(List<(string, HashSet<string>)> duplicates)
        {
            var window = GetWindow<DuplicateKeysWindow>(true, "Duplicate TextKeys", true);
            window.duplicates = duplicates;
            window.minSize = new Vector2(440, 300);
        }
    }
}
