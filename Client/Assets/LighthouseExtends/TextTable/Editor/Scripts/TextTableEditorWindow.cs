using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using LighthouseExtends.TextMeshPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LighthouseExtends.TextTable.Editor
{
    /// <summary>
    /// EditorWindow for editing LHTextMeshPro text keys and TSV translations across Scenes and Prefabs.
    /// Provides LHTextMeshPro View and TextKey View tabs, language addition, Global/Local key management,
    /// and duplicate and missing key checks.
    /// </summary>
    public class TextTableEditorWindow : EditorWindow
    {
        const string MenuPath = "Lighthouse/TextTable/Open Editor";
        const string SettingsDefaultDirectory = "Assets/Settings/Lighthouse";
        const float LeftPanelMinWidth = 120f;
        const float LeftPanelDefaultWidth = 220f;
        const float ResizeHandleWidth = 4f;
        const float ColHandleWidth = 6f;
        const float MinColWidth = 60f;
        const float DeleteButtonWidth = 22f;
        const float ActionButtonWidth = 60f;
        const float ScopeColWidth = 64f;

        readonly List<Rect> colHandleRects = new();
        readonly List<float> langColWidths = new();

        TableViewTab activeTab;
        Rect addLanguageButtonRect;
        Dictionary<string, Dictionary<string, string>> allTsvData = new();
        string assetListError;
        Vector2 assetListScroll;
        GUIStyle deleteButtonStyle;
        HashSet<string> deletedGlobalTsvKeys = new();
        HashSet<string> deletedTsvKeys = new();
        Dictionary<string, HashSet<string>> deletedOtherTsvKeys = new();
        GUIStyle dirtyTextFieldStyle;
        bool globalTsvDirty;
        bool isDirty;
        bool isResizingPanel;
        float keyColWidth = 180f;
        Dictionary<string, Dictionary<string, string>> keySourceMap = new();
        List<string> languages = new();
        GUIStyle leftAlignedButtonStyle;
        float leftPanelWidth = LeftPanelDefaultWidth;
        float pathColWidth = 200f;
        int pendingFocusRowIndex = -1;
        bool autoNewLine = true;
        float placeholderColWidth = 160f;
        List<AssetEntry> prefabEntries = new();
        float resizeStartColWidth;
        float resizeStartMouseX;
        int resizingColIndex = -1;
        List<ComponentRow> rows = new();
        List<AssetEntry> sceneEntries = new();
        AssetEntry selectedEntry;
        TextTableEditorSettings settings;
        string tableLoadError;
        Vector2 tableScroll;
        List<string> tsvLoadErrors = new();

        // ─────────────────────────────────────────────────────────────────
        // EditorWindow lifecycle
        // ─────────────────────────────────────────────────────────────────

        [MenuItem(MenuPath)]
        static void Open()
        {
            GetWindow<TextTableEditorWindow>("TextTable Editor");
        }

        void OnEnable()
        {
            LoadOrCreateSettings();
            RefreshAssetList();
            if (selectedEntry != null)
            {
                LoadTableData();
            }
        }

        void OnGUI()
        {
            EnsureStyles();
            DrawToolbar();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawAssetList();
                DrawPanelResizeHandle();
                DrawTable();
            }

            HandlePanelResize();
            HandleColumnResize();
        }

        // ─────────────────────────────────────────────────────────────────
        // Styles
        // ─────────────────────────────────────────────────────────────────

        void EnsureStyles()
        {
            if (leftAlignedButtonStyle == null)
            {
                leftAlignedButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (dirtyTextFieldStyle == null)
            {
                dirtyTextFieldStyle = new GUIStyle(EditorStyles.textField);
                dirtyTextFieldStyle.normal.textColor = new Color(1f, 0.85f, 0f);
                dirtyTextFieldStyle.focused.textColor = new Color(1f, 0.85f, 0f);
            }

            if (deleteButtonStyle == null)
            {
                deleteButtonStyle = new GUIStyle(EditorStyles.miniButton);
                deleteButtonStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Toolbar
        // ─────────────────────────────────────────────────────────────────

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var next = (TextTableEditorSettings)EditorGUILayout.ObjectField(
                    settings, typeof(TextTableEditorSettings), false, GUILayout.Width(320));
                if (next != settings)
                {
                    settings = next;
                    selectedEntry = null;
                    rows = new List<ComponentRow>();
                    allTsvData = new Dictionary<string, Dictionary<string, string>>();
                    RefreshAssetList();
                }

                using (new EditorGUI.DisabledScope(settings == null))
                {
                    if (GUILayout.Button("Check Duplicates", EditorStyles.toolbarButton, GUILayout.Width(110)))
                    {
                        CheckDuplicateKeys();
                    }

                    if (GUILayout.Button("Check Missing", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    {
                        CheckMissingKeys();
                    }

                    var newAutoNewLine = GUILayout.Toggle(
                        autoNewLine,
                        new GUIContent(@"\n as ↵", @"When enabled, \n in TSV values is shown as a real newline in text fields and converted back on edit."),
                        EditorStyles.toolbarButton,
                        GUILayout.Width(70));
                    if (newAutoNewLine != autoNewLine)
                    {
                        autoNewLine = newAutoNewLine;
                        Repaint();
                    }
                }

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(settings == null))
                {
                    if (GUILayout.Button("Add Language", EditorStyles.toolbarButton, GUILayout.Width(90)))
                    {
                        var rect = addLanguageButtonRect;
                        EditorApplication.delayCall += () => PopupWindow.Show(rect, new AddLanguagePopup(AddLanguage));
                    }

                    if (Event.current.type == EventType.Repaint)
                    {
                        addLanguageButtonRect = GUILayoutUtility.GetLastRect();
                    }
                }

                using (new EditorGUI.DisabledScope(selectedEntry == null))
                {
                    if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    {
                        ReloadTableData();
                    }
                }

                using (new EditorGUI.DisabledScope(!isDirty))
                {
                    if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    {
                        Save();
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Asset list panel
        // ─────────────────────────────────────────────────────────────────

        void DrawAssetList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(leftPanelWidth), GUILayout.ExpandHeight(true)))
            {
                assetListScroll = EditorGUILayout.BeginScrollView(assetListScroll);

                if (!string.IsNullOrEmpty(assetListError))
                {
                    EditorGUILayout.HelpBox(assetListError, MessageType.Error);
                    if (GUILayout.Button("Retry", GUILayout.Width(leftPanelWidth - 8)))
                    {
                        RefreshAssetList();
                    }
                }
                else
                {
                    DrawAssetGroup("Scenes", sceneEntries);
                    GUILayout.Space(6);
                    DrawAssetGroup("Prefabs", prefabEntries);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        void DrawAssetGroup(string label, List<AssetEntry> entries)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            var groups = entries
                .GroupBy(e => Path.GetDirectoryName(e.assetPath).Replace('\\', '/'))
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(group.Key, EditorStyles.miniLabel, GUILayout.Width(leftPanelWidth - 16));
                EditorGUI.indentLevel--;

                foreach (var entry in group)
                {
                    DrawAssetButton(entry);
                }
            }
        }

        void DrawAssetButton(AssetEntry entry)
        {
            var isSelected = selectedEntry == entry;
            using (new BackgroundColorScope(isSelected ? new Color(0.4f, 0.6f, 1f) : Color.white))
            {
                if (GUILayout.Button(entry.displayName, leftAlignedButtonStyle))
                {
                    SelectAsset(entry);
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(entry.assetPath);
                    if (asset != null)
                    {
                        EditorGUIUtility.PingObject(asset);
                        if (!entry.isScene)
                        {
                            Selection.activeObject = asset;
                        }
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Panel and column resize
        // ─────────────────────────────────────────────────────────────────

        void DrawPanelResizeHandle()
        {
            var handleRect = GUILayoutUtility.GetRect(ResizeHandleWidth, 0,
                GUILayout.Width(ResizeHandleWidth), GUILayout.ExpandHeight(true));
            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeHorizontal);
            EditorGUI.DrawRect(handleRect, new Color(0f, 0f, 0f, 0.2f));

            if (Event.current.type == EventType.MouseDown && handleRect.Contains(Event.current.mousePosition))
            {
                isResizingPanel = true;
                Event.current.Use();
            }
        }

        void HandlePanelResize()
        {
            if (!isResizingPanel)
            {
                return;
            }

            if (Event.current.type == EventType.MouseDrag)
            {
                leftPanelWidth = Mathf.Max(LeftPanelMinWidth, Event.current.mousePosition.x);
                Repaint();
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                isResizingPanel = false;
                Event.current.Use();
            }
        }

        void HandleColumnResize()
        {
            if (activeTab != TableViewTab.LHTextMeshPro)
            {
                return;
            }

            var e = Event.current;

            for (var i = 0; i < colHandleRects.Count; i++)
            {
                EditorGUIUtility.AddCursorRect(colHandleRects[i], MouseCursor.ResizeHorizontal);
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                for (var i = 0; i < colHandleRects.Count; i++)
                {
                    if (!colHandleRects[i].Contains(e.mousePosition))
                    {
                        continue;
                    }

                    resizingColIndex = i;
                    resizeStartMouseX = e.mousePosition.x;
                    resizeStartColWidth = GetColWidth(i);
                    e.Use();
                    break;
                }
            }
            else if (e.type == EventType.MouseDrag && 0 <= resizingColIndex)
            {
                SetColWidth(resizingColIndex, resizeStartColWidth + (e.mousePosition.x - resizeStartMouseX));
                Repaint();
                e.Use();
            }
            else if (e.type == EventType.MouseUp && 0 <= resizingColIndex)
            {
                resizingColIndex = -1;
                e.Use();
            }
        }

        float GetColWidth(int index)
        {
            if (index == 0) { return pathColWidth; }
            if (index == 1) { return placeholderColWidth; }
            if (index == 2) { return keyColWidth; }

            var li = index - 3;
            return li < langColWidths.Count ? langColWidths[li] : 160f;
        }

        void SetColWidth(int index, float width)
        {
            var w = Mathf.Max(MinColWidth, width);
            if (index == 0) { pathColWidth = w; return; }
            if (index == 1) { placeholderColWidth = w; return; }
            if (index == 2) { keyColWidth = w; return; }

            var li = index - 3;
            if (li < langColWidths.Count)
            {
                langColWidths[li] = w;
            }
        }

        void SyncLangColWidths()
        {
            while (langColWidths.Count < languages.Count)
            {
                langColWidths.Add(160f);
            }

            while (languages.Count < langColWidths.Count)
            {
                langColWidths.RemoveAt(langColWidths.Count - 1);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Table (dispatcher)
        // ─────────────────────────────────────────────────────────────────

        void DrawTable()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (selectedEntry == null)
                {
                    EditorGUILayout.HelpBox("Select a Scene or Prefab from the list.", MessageType.Info);
                    return;
                }

                DrawTabBar();

                if (!string.IsNullOrEmpty(tableLoadError))
                {
                    EditorGUILayout.HelpBox(tableLoadError, MessageType.Error);
                    if (GUILayout.Button("Retry"))
                    {
                        LoadTableData();
                    }

                    return;
                }

                if (0 < tsvLoadErrors.Count)
                {
                    foreach (var err in tsvLoadErrors)
                    {
                        EditorGUILayout.HelpBox(err, MessageType.Warning);
                    }
                }

                if (languages.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        $"No TSV files found.\nFix: Check 'Text Table Folder Path' in Settings, or add a language via Add Language.\nPath: {settings?.TextTableFolderPath}",
                        MessageType.Warning);
                }

                DrawTableHeader();

                tableScroll = EditorGUILayout.BeginScrollView(tableScroll);

                if (activeTab == TableViewTab.LHTextMeshPro)
                {
                    for (var i = 0; i < rows.Count; i++)
                    {
                        DrawLHTextMeshProRow(rows[i], i);
                    }
                }
                else
                {
                    DrawTextKeyView();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        void DrawTabBar()
        {
            var newTab = (TableViewTab)GUILayout.Toolbar(
                (int)activeTab,
                new[] { "LHTextMeshPro View", "TextKey View" },
                EditorStyles.toolbarButton);

            if (newTab != activeTab)
            {
                activeTab = newTab;
                pendingFocusRowIndex = -1;
                Repaint();
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // LHTextMeshPro view
        // ─────────────────────────────────────────────────────────────────

        void DrawTableHeader()
        {
            if (activeTab == TableViewTab.LHTextMeshPro)
            {
                DrawLHTextMeshProHeader();
            }
        }

        void DrawLHTextMeshProHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                DrawHeaderCell("Hierarchy Path", 0);
                DrawHeaderCell("PlaceHolder", 1);
                DrawHeaderCell("TextKey", 2);
                EditorGUILayout.LabelField("Scope", EditorStyles.toolbarButton, GUILayout.Width(ScopeColWidth));
                for (var i = 0; i < languages.Count; i++)
                {
                    DrawHeaderCell(languages[i], 3 + i);
                }
            }
        }

        void DrawHeaderCell(string label, int colIndex)
        {
            var width = GetColWidth(colIndex);
            EditorGUILayout.LabelField(label, EditorStyles.toolbarButton, GUILayout.Width(width));

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var cellRect = GUILayoutUtility.GetLastRect();
            var handleRect = new Rect(cellRect.xMax - ColHandleWidth * 0.5f, cellRect.y, ColHandleWidth,
                cellRect.height);

            while (colHandleRects.Count <= colIndex)
            {
                colHandleRects.Add(Rect.zero);
            }

            colHandleRects[colIndex] = handleRect;
        }

        void DrawLHTextMeshProRow(ComponentRow row, int rowIndex)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Column 1: Hierarchy Path
                if (GUILayout.Button(row.hierarchyPath, leftAlignedButtonStyle, GUILayout.Width(pathColWidth)))
                {
                    SelectGameObject(row);
                }

                // Column 2: PlaceHolder (selectable, multiline)
                var placeholderDisplay = row.placeholderText.Replace("\\n", "\n");
                var placeholderLineCount = Mathf.Max(1, placeholderDisplay.Count(c => c == '\n') + 1);
                var placeholderHeight = placeholderLineCount * EditorGUIUtility.singleLineHeight;
                EditorGUILayout.SelectableLabel(placeholderDisplay,
                    GUILayout.Width(placeholderColWidth),
                    GUILayout.Height(placeholderHeight));

                // Column 3: TextKey (yellow if unsaved)
                var controlName = $"TextKeyField_{rowIndex}";
                GUI.SetNextControlName(controlName);
                var keyStyle = row.IsTextKeyDirty ? dirtyTextFieldStyle : EditorStyles.textField;
                var nextKey = EditorGUILayout.TextField(row.textKey, keyStyle, GUILayout.Width(keyColWidth));
                if (nextKey != row.textKey)
                {
                    row.textKey = nextKey;
                    foreach (var lang in languages)
                    {
                        row.langData[lang] = allTsvData.TryGetValue(nextKey, out var langMap) &&
                                             langMap.TryGetValue(lang, out var text)
                            ? text
                            : string.Empty;
                    }

                    isDirty = true;
                }

                // Focus request from TextKey View
                if (pendingFocusRowIndex == rowIndex)
                {
                    EditorGUI.FocusTextInControl(controlName);
                    pendingFocusRowIndex = -1;
                }

                // Scope column
                var isGlobal = IsGlobalKey(row.textKey);
                var scopeLabel = isGlobal ? "(Global)" : "(Local)";
                var scopeColor = isGlobal ? new Color(0.4f, 0.75f, 1f) : new Color(0.6f, 0.6f, 0.6f);
                var prevColor = GUI.contentColor;
                GUI.contentColor = scopeColor;
                EditorGUILayout.LabelField(scopeLabel, EditorStyles.miniLabel, GUILayout.Width(ScopeColWidth));
                GUI.contentColor = prevColor;

                // Column 4+: Language text
                for (var i = 0; i < languages.Count; i++)
                {
                    var lang = languages[i];
                    var colW = i < langColWidths.Count ? langColWidths[i] : 160f;
                    var current = row.langData.TryGetValue(lang, out var t) ? t : string.Empty;
                    var hasKey = !string.IsNullOrEmpty(row.textKey);
                    var isReadOnly = hasKey &&
                                     keySourceMap.TryGetValue(row.textKey, out var sm) &&
                                     sm.TryGetValue(lang, out var src) &&
                                     src != selectedEntry.tsvBaseName &&
                                     src != "Global";

                    var langControlName = $"LangField_{rowIndex}_{lang}";
                    GUI.SetNextControlName(langControlName);

                    var displayValue = autoNewLine ? current.Replace("\\n", "\n") : current;
                    var lineCount = Mathf.Max(1, displayValue.Count(c => c == '\n') + 1);
                    var cellHeight = Mathf.Min(lineCount * EditorGUIUtility.singleLineHeight + 4f, 160f);
                    string next;

                    using (new EditorGUI.DisabledScope(!hasKey || isReadOnly))
                    {
                        var nextDisplay = EditorGUILayout.TextArea(displayValue, GUILayout.Width(colW), GUILayout.Height(cellHeight));
                        next = autoNewLine ? nextDisplay.Replace("\n", "\\n") : nextDisplay;
                    }

                    if (hasKey && !isReadOnly && next != current)
                    {
                        foreach (var r in rows)
                        {
                            if (r.textKey == row.textKey)
                            {
                                r.langData[lang] = next;
                            }
                        }

                        isDirty = true;
                        if (isGlobal)
                        {
                            globalTsvDirty = true;
                        }
                    }
                }
            }

            EditorGUILayout.Space(1);
        }

        // ─────────────────────────────────────────────────────────────────
        // TextKey view
        // ─────────────────────────────────────────────────────────────────

        void DrawTextKeyView()
        {
            var globalKeys = keySourceMap
                .Where(kvp => kvp.Value.Values.Any(v => v == "Global") && !deletedGlobalTsvKeys.Contains(kvp.Key))
                .Select(kvp => kvp.Key)
                .ToHashSet();

            var localTsvKeys = keySourceMap
                .Where(kvp =>
                    kvp.Value.Values.Any(v => v == selectedEntry.tsvBaseName) && !deletedTsvKeys.Contains(kvp.Key))
                .Select(kvp => kvp.Key)
                .ToHashSet();

            foreach (var r in rows)
            {
                if (!string.IsNullOrEmpty(r.textKey) && !deletedTsvKeys.Contains(r.textKey) &&
                    !globalKeys.Contains(r.textKey))
                {
                    localTsvKeys.Add(r.textKey);
                }
            }

            if (0 < globalKeys.Count)
            {
                EditorGUILayout.LabelField("Global", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);

                foreach (var key in globalKeys.OrderBy(k => k))
                {
                    var refs = rows.Where(r => r.textKey == key).ToList();
                    DrawTextKeyGroupRow(key, refs, true);

                    foreach (var row in refs)
                    {
                        DrawTextKeyChildRow(row);
                    }

                    EditorGUILayout.Space(2);
                }

                EditorGUILayout.Space(6);
            }

            if (0 < localTsvKeys.Count)
            {
                EditorGUILayout.LabelField(selectedEntry.tsvBaseName, EditorStyles.boldLabel);
                EditorGUILayout.Space(2);

                foreach (var key in localTsvKeys.OrderBy(k => k))
                {
                    var refs = rows.Where(r => r.textKey == key).ToList();
                    DrawTextKeyGroupRow(key, refs, false);

                    foreach (var row in refs)
                    {
                        DrawTextKeyChildRow(row);
                    }

                    EditorGUILayout.Space(2);
                }
            }
        }

        void DrawTextKeyGroupRow(string key, List<ComponentRow> referencingRows, bool isGlobal)
        {
            var rowRect = EditorGUILayout.BeginHorizontal();

            if (Event.current.type == EventType.Repaint && 0 < rowRect.height)
            {
                EditorGUI.DrawRect(rowRect, new Color(0f, 0f, 0f, 0.1f));
            }

            var label = isGlobal ? "Localize" : "Globalize";
            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Width(ActionButtonWidth)))
            {
                if (isGlobal)
                {
                    LocalizeKey(key);
                }
                else
                {
                    GlobalizeKey(key);
                }
            }

            if (GUILayout.Button("×", deleteButtonStyle, GUILayout.Width(DeleteButtonWidth)))
            {
                if (isGlobal)
                {
                    deletedGlobalTsvKeys.Add(key);
                    globalTsvDirty = true;
                }
                else
                {
                    deletedTsvKeys.Add(key);
                }

                foreach (var r in referencingRows)
                {
                    r.textKey = string.Empty;
                }

                isDirty = true;
            }

            EditorGUILayout.SelectableLabel(key, EditorStyles.boldLabel,
                GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            var preview = GetTextKeyPreview(key);
            if (!string.IsNullOrEmpty(preview))
            {
                EditorGUILayout.LabelField(preview, EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawTextKeyChildRow(ComponentRow row)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(ActionButtonWidth + DeleteButtonWidth);

                if (GUILayout.Button("×", deleteButtonStyle, GUILayout.Width(DeleteButtonWidth)))
                {
                    row.textKey = string.Empty;
                    isDirty = true;
                }

                if (GUILayout.Button(row.hierarchyPath, leftAlignedButtonStyle))
                {
                    SwitchToLHTextMeshProViewAndFocus(row);
                }
            }
        }

        void SwitchToLHTextMeshProViewAndFocus(ComponentRow row)
        {
            var rowIndex = rows.IndexOf(row);
            if (rowIndex < 0)
            {
                return;
            }

            activeTab = TableViewTab.LHTextMeshPro;
            pendingFocusRowIndex = rowIndex;

            var estimatedRowHeight = EditorGUIUtility.singleLineHeight + 3f;
            tableScroll.y = rowIndex * estimatedRowHeight;

            Repaint();
        }

        // ─────────────────────────────────────────────────────────────────
        // GameObject selection
        // ─────────────────────────────────────────────────────────────────

        void SelectGameObject(ComponentRow row)
        {
            if (selectedEntry.isScene)
            {
                Selection.activeGameObject = row.component.gameObject;
            }
            else
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(selectedEntry.assetPath);
                if (prefab == null)
                {
                    return;
                }

                AssetDatabase.OpenAsset(prefab);
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (stage == null)
                {
                    return;
                }

                var go = FindGameObjectByPath(stage.prefabContentsRoot, row.hierarchyPath);
                if (go != null)
                {
                    Selection.activeGameObject = go;
                }
            }
        }

        static GameObject FindGameObjectByPath(GameObject root, string path)
        {
            var parts = path.Split('/');
            if (parts.Length == 0 || root.name != parts[0])
            {
                return null;
            }

            var current = root;
            for (var i = 1; i < parts.Length; i++)
            {
                Transform found = null;
                foreach (Transform child in current.transform)
                {
                    if (child.name == parts[i])
                    {
                        found = child;
                        break;
                    }
                }

                if (found == null)
                {
                    return null;
                }

                current = found.gameObject;
            }

            return current;
        }

        // ─────────────────────────────────────────────────────────────────
        // Globalize / Localize
        // ─────────────────────────────────────────────────────────────────

        void GlobalizeKey(string key)
        {
            if (!keySourceMap.TryGetValue(key, out var sm))
            {
                sm = new Dictionary<string, string>();
                keySourceMap[key] = sm;
            }

            var folder = ToAbsolutePath(settings.TextTableFolderPath);
            var otherFiles = FindOtherFilesContainingKey(key, folder);

            if (otherFiles.Count > 0)
            {
                var fileList = string.Join("\n", otherFiles.Select(s => $"  • {s}"));
                var message =
                    $"The key \"{key}\" also exists in the following TSV files:\n\n{fileList}\n\n" +
                    "After globalizing, these files will still define the same key, " +
                    "causing a runtime duplicate (last-loaded file wins).\n\n" +
                    "Remove this key from those files on Save?";

                var choice = EditorUtility.DisplayDialogComplex(
                    "Duplicate Key Detected",
                    message,
                    "Remove on Save",
                    "Cancel",
                    "Keep As-Is");

                if (choice == 1)
                {
                    return;
                }

                if (choice == 0)
                {
                    foreach (var baseName in otherFiles)
                    {
                        if (!deletedOtherTsvKeys.ContainsKey(baseName))
                        {
                            deletedOtherTsvKeys[baseName] = new HashSet<string>();
                        }

                        deletedOtherTsvKeys[baseName].Add(key);
                    }
                }
            }

            foreach (var lang in languages)
            {
                sm[lang] = "Global";
            }

            deletedTsvKeys.Add(key);
            globalTsvDirty = true;
            isDirty = true;
        }

        bool IsGlobalKey(string key)
        {
            return !string.IsNullOrEmpty(key) &&
                   keySourceMap.TryGetValue(key, out var sm) &&
                   sm.Values.Any(v => v == "Global");
        }

        List<string> FindOtherFilesContainingKey(string key, string folder)
        {
            var result = new List<string>();
            if (!Directory.Exists(folder))
            {
                return result;
            }

            var seen = new HashSet<string>();
            foreach (var file in Directory.GetFiles(folder, "*.tsv"))
            {
                var nameNoExt = Path.GetFileNameWithoutExtension(file);
                var dot = nameNoExt.LastIndexOf('.');
                if (dot < 0)
                {
                    continue;
                }

                var baseName = nameNoExt.Substring(0, dot);
                if (baseName == "Global" || baseName == selectedEntry.tsvBaseName || !seen.Add(baseName))
                {
                    continue;
                }

                foreach (var line in File.ReadAllLines(file).Skip(1))
                {
                    var trimmed = line.TrimEnd('\r');
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        continue;
                    }

                    var tab = trimmed.IndexOf('\t');
                    if (tab < 0)
                    {
                        continue;
                    }

                    if (trimmed.Substring(0, tab) == key)
                    {
                        result.Add(baseName);
                        break;
                    }
                }
            }

            return result;
        }

        void LocalizeKey(string key)
        {
            if (selectedEntry == null)
            {
                return;
            }

            if (!keySourceMap.TryGetValue(key, out var sm))
            {
                return;
            }

            foreach (var lang in languages)
            {
                if (sm.TryGetValue(lang, out var src) && src == "Global")
                {
                    sm[lang] = selectedEntry.tsvBaseName;
                }
            }

            deletedGlobalTsvKeys.Add(key);
            globalTsvDirty = true;
            isDirty = true;
        }

        // ─────────────────────────────────────────────────────────────────
        // Check operations
        // ─────────────────────────────────────────────────────────────────

        void CheckDuplicateKeys()
        {
            if (settings == null)
            {
                return;
            }

            var folder = ToAbsolutePath(settings.TextTableFolderPath);
            if (!Directory.Exists(folder))
            {
                EditorUtility.DisplayDialog("Folder Not Found",
                    $"TSV folder not found:\n{settings.TextTableFolderPath}", "OK");
                return;
            }

            var keyToScopes = new Dictionary<string, HashSet<string>>();

            foreach (var file in Directory.GetFiles(folder, "*.tsv"))
            {
                var nameNoExt = Path.GetFileNameWithoutExtension(file);
                var dot = nameNoExt.LastIndexOf('.');
                if (dot < 0)
                {
                    continue;
                }

                var baseName = nameNoExt.Substring(0, dot);

                foreach (var line in File.ReadAllLines(file).Skip(1))
                {
                    var trimmed = line.TrimEnd('\r');
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        continue;
                    }

                    var tab = trimmed.IndexOf('\t');
                    if (tab < 0)
                    {
                        continue;
                    }

                    var key = trimmed.Substring(0, tab);
                    if (!keyToScopes.ContainsKey(key))
                    {
                        keyToScopes[key] = new HashSet<string>();
                    }

                    keyToScopes[key].Add(baseName);
                }
            }

            var duplicates = keyToScopes
                .Where(kvp => 1 < kvp.Value.Count)
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();

            if (duplicates.Count == 0)
            {
                EditorUtility.DisplayDialog("No Duplicates",
                    "No duplicate TextKeys found across scopes.", "OK");
                return;
            }

            DuplicateKeysWindow.Show(duplicates);
        }

        void CheckMissingKeys()
        {
            if (settings == null)
            {
                return;
            }

            var folder = ToAbsolutePath(settings.TextTableFolderPath);
            if (!Directory.Exists(folder))
            {
                EditorUtility.DisplayDialog("Folder Not Found",
                    $"TSV folder not found:\n{settings.TextTableFolderPath}", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Check Missing",
                    "This will open all scenes to scan components. Your active scene may change.\nContinue?",
                    "Continue",
                    "Cancel"))
            {
                return;
            }

            var (_, sourceMap, _) = TextTableTsvRepository.LoadAllTsvData(folder);

            var globalKeys = sourceMap
                .Where(kvp => kvp.Value.Values.Any(v => v == "Global"))
                .Select(kvp => kvp.Key)
                .ToHashSet();

            var scopeKeys = new Dictionary<string, HashSet<string>>();
            foreach (var (key, langSources) in sourceMap)
            {
                foreach (var src in langSources.Values.Where(v => v != "Global").Distinct())
                {
                    if (!scopeKeys.ContainsKey(src))
                    {
                        scopeKeys[src] = new HashSet<string>();
                    }

                    scopeKeys[src].Add(key);
                }
            }

            var missing = new List<(string scope, string hierarchyPath, string textKey)>();

            foreach (var entry in sceneEntries.Concat(prefabEntries))
            {
                List<(LHTextMeshPro comp, string path)> comps;
                try
                {
                    comps = FindComponents(entry);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[TextTable] CheckMissing: failed to load '{entry.displayName}': {e.Message}");
                    continue;
                }

                var available = globalKeys.ToHashSet();
                if (scopeKeys.TryGetValue(entry.tsvBaseName, out var localKeys))
                {
                    foreach (var k in localKeys)
                    {
                        available.Add(k);
                    }
                }

                foreach (var (comp, path) in comps)
                {
                    var key = ReadTextKey(comp);
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    if (!available.Contains(key))
                    {
                        missing.Add((entry.displayName, path, key));
                    }
                }
            }

            if (missing.Count == 0)
            {
                EditorUtility.DisplayDialog("Check Missing", "No missing TextKey references found.", "OK");
                return;
            }

            MissingKeysWindow.Show(missing);
        }

        // ─────────────────────────────────────────────────────────────────
        // Data management
        // ─────────────────────────────────────────────────────────────────

        void SelectAsset(AssetEntry entry)
        {
            if (isDirty)
            {
                var choice = EditorUtility.DisplayDialogComplex(
                    "Unsaved Changes",
                    "There are unsaved changes.",
                    "Save",
                    "Cancel",
                    "Discard");

                if (choice == 1)
                {
                    return;
                }

                if (choice == 0)
                {
                    Save();
                }
            }

            selectedEntry = entry;
            LoadTableData();
            Repaint();
        }

        void LoadTableData()
        {
            rows = new List<ComponentRow>();
            allTsvData = new Dictionary<string, Dictionary<string, string>>();
            keySourceMap = new Dictionary<string, Dictionary<string, string>>();
            deletedTsvKeys = new HashSet<string>();
            deletedGlobalTsvKeys = new HashSet<string>();
            deletedOtherTsvKeys = new Dictionary<string, HashSet<string>>();
            globalTsvDirty = false;
            isDirty = false;
            tableLoadError = null;
            tsvLoadErrors = new List<string>();

            if (selectedEntry == null || settings == null)
            {
                return;
            }

            var folder = ToAbsolutePath(settings.TextTableFolderPath);

            try
            {
                languages = TextTableTsvRepository.DetectLanguages(folder);
            }
            catch (Exception e)
            {
                tableLoadError = "Failed to read the TSV folder.\n" +
                                 $"Fix: Check 'Text Table Folder Path' in Settings. Current value: {settings.TextTableFolderPath}\n" +
                                 $"Detail: {e.Message}";
                Debug.LogException(e);
                return;
            }

            SyncLangColWidths();

            try
            {
                var (loaded, loadedSourceMap, errors) = TextTableTsvRepository.LoadAllTsvData(folder);
                allTsvData = loaded;
                keySourceMap = loadedSourceMap;
                tsvLoadErrors = errors;
            }
            catch (Exception e)
            {
                tableLoadError = "Failed to load TSV files.\n" +
                                 $"Fix: Verify the TSV files in '{settings.TextTableFolderPath}' are correctly formatted.\n" +
                                 $"Detail: {e.Message}";
                Debug.LogException(e);
                return;
            }

            List<(LHTextMeshPro comp, string path)> componentList;
            try
            {
                componentList = FindComponents(selectedEntry);
            }
            catch (Exception e)
            {
                tableLoadError = $"Failed to load components from '{selectedEntry.displayName}'.\n" +
                                 $"Fix: Check the file is not corrupted. Path: {selectedEntry.assetPath}\n" +
                                 $"Detail: {e.Message}";
                Debug.LogException(e);
                return;
            }

            foreach (var (comp, path) in componentList)
            {
                var key = ReadTextKey(comp);
                var langData = new Dictionary<string, string>();
                foreach (var lang in languages)
                {
                    langData[lang] = allTsvData.TryGetValue(key, out var langMap) &&
                                     langMap.TryGetValue(lang, out var text)
                        ? text
                        : string.Empty;
                }

                var placeholder = ReadPlaceholderText(comp);
                rows.Add(new ComponentRow(path, key, placeholder, comp, langData));
            }
        }

        void ReloadTableData()
        {
            if (isDirty && !EditorUtility.DisplayDialog(
                    "Unsaved Changes",
                    "There are unsaved changes. Discard and reload?",
                    "Reload",
                    "Cancel"))
            {
                return;
            }

            LoadTableData();
            Repaint();
        }

        void AddLanguage(string langCode)
        {
            if (settings == null)
            {
                return;
            }

            var folder = ToAbsolutePath(settings.TextTableFolderPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var baseName = selectedEntry != null ? selectedEntry.tsvBaseName : "TextTable";
            var filePath = Path.Combine(folder, $"{baseName}.{langCode}.tsv");

            if (File.Exists(filePath))
            {
                EditorUtility.DisplayDialog(
                    "Already Exists",
                    $"A file for language \"{langCode}\" already exists.",
                    "OK");
                return;
            }

            File.WriteAllText(filePath, "key\ttext\n", Encoding.UTF8);
            AssetDatabase.Refresh();
            LoadTableData();
            Repaint();
        }

        // ─────────────────────────────────────────────────────────────────
        // Settings
        // ─────────────────────────────────────────────────────────────────

        void LoadOrCreateSettings()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(TextTableEditorSettings)}");
            if (0 < guids.Length)
            {
                if (1 < guids.Length)
                {
                    Debug.LogWarning("[TextTable] Multiple TextTableEditorSettings found. Using the first one.");
                }

                settings = AssetDatabase.LoadAssetAtPath<TextTableEditorSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                return;
            }

            settings = CreateInstance<TextTableEditorSettings>();
            if (!Directory.Exists(SettingsDefaultDirectory))
            {
                Directory.CreateDirectory(SettingsDefaultDirectory);
            }

            AssetDatabase.CreateAsset(settings, $"{SettingsDefaultDirectory}/{nameof(TextTableEditorSettings)}.asset");
            AssetDatabase.SaveAssets();
        }

        // ─────────────────────────────────────────────────────────────────
        // Asset discovery
        // ─────────────────────────────────────────────────────────────────

        void RefreshAssetList()
        {
            assetListError = null;
            try
            {
                sceneEntries = AssetDatabase.FindAssets("t:SceneAsset")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.StartsWith("Assets/"))
                    .Select(p => new AssetEntry(p, Path.GetFileNameWithoutExtension(p), $"Scene{Path.GetFileNameWithoutExtension(p)}", true))
                    .ToList();

                var prefabPaths = AssetDatabase.FindAssets("t:Prefab")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.StartsWith("Assets/"))
                    .ToList();

                var baseNames = ComputePrefabBaseNames(prefabPaths);
                prefabEntries = prefabPaths
                    .Select((p, i) => new AssetEntry(p, Path.GetFileNameWithoutExtension(p), baseNames[i], false))
                    .ToList();
            }
            catch (Exception e)
            {
                assetListError = "Failed to load Scene / Prefab list.\n" +
                                 "Fix: Verify the AssetDatabase is initialized. Try restarting Unity or running Assets > Reimport All.\n" +
                                 $"Detail: {e.Message}";
                sceneEntries = new List<AssetEntry>();
                prefabEntries = new List<AssetEntry>();
                Debug.LogException(e);
            }
        }

        static List<string> ComputePrefabBaseNames(List<string> paths)
        {
            var depth = 1;
            var names = paths.Select(p => GetParentDirSuffix(p, depth)).ToList();

            for (var iter = 0; iter < 10 && HasDuplicates(names); iter++)
            {
                depth++;
                var duplicates = names.GroupBy(n => n)
                    .Where(g => 1 < g.Count())
                    .Select(g => g.Key)
                    .ToHashSet();

                for (var i = 0; i < names.Count; i++)
                {
                    if (duplicates.Contains(names[i]))
                    {
                        names[i] = GetParentDirSuffix(paths[i], depth);
                    }
                }
            }

            return names.Select(n => $"Prefab{n}").ToList();
        }

        static string GetParentDirSuffix(string assetPath, int depth)
        {
            var parts = assetPath.Replace('\\', '/').Split('/');
            var taken = new List<string>();
            for (var d = 1; d <= depth && d < parts.Length - 1; d++)
            {
                taken.Insert(0, parts[parts.Length - 1 - d]);
            }

            return string.Concat(taken);
        }

        static bool HasDuplicates(List<string> list)
        {
            return list.Count != list.Distinct().Count();
        }

        // ─────────────────────────────────────────────────────────────────
        // Component discovery
        // ─────────────────────────────────────────────────────────────────

        static List<(LHTextMeshPro comp, string path)> FindComponents(AssetEntry entry)
        {
            LHTextMeshPro[] comps;
            if (entry.isScene)
            {
                EditorSceneManager.OpenScene(entry.assetPath, OpenSceneMode.Single);
                comps = SceneManager.GetActiveScene()
                    .GetRootGameObjects()
                    .SelectMany(go => go.GetComponentsInChildren<LHTextMeshPro>(true))
                    .ToArray();
            }
            else
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.assetPath);
                comps = prefab != null
                    ? prefab.GetComponentsInChildren<LHTextMeshPro>(true)
                    : Array.Empty<LHTextMeshPro>();
            }

            return comps.Select(c => (c, GetHierarchyPath(c.gameObject))).ToList();
        }

        // ─────────────────────────────────────────────────────────────────
        // Save
        // ─────────────────────────────────────────────────────────────────

        void Save()
        {
            if (selectedEntry == null || settings == null)
            {
                return;
            }

            if (rows.Any(r => r.IsTextKeyDirty))
            {
                foreach (var row in rows)
                {
                    if (!row.IsTextKeyDirty)
                    {
                        continue;
                    }

                    var so = new SerializedObject(row.component);
                    so.FindProperty("textKey").stringValue = row.textKey;
                    so.ApplyModifiedProperties();
                }

                if (selectedEntry.isScene)
                {
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                }
                else
                {
                    AssetDatabase.SaveAssets();
                }
            }

            var folder = ToAbsolutePath(settings.TextTableFolderPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (var lang in languages)
            {
                var filePath = Path.Combine(folder, $"{selectedEntry.tsvBaseName}.{lang}.tsv");

                var existing = new Dictionary<string, string>();
                if (File.Exists(filePath))
                {
                    foreach (var line in File.ReadAllLines(filePath).Skip(1))
                    {
                        var trimmed = line.TrimEnd('\r');
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        var tab = trimmed.IndexOf('\t');
                        if (tab < 0)
                        {
                            continue;
                        }

                        existing[trimmed.Substring(0, tab)] = trimmed.Substring(tab + 1);
                    }
                }

                foreach (var deleted in deletedTsvKeys)
                {
                    existing.Remove(deleted);
                }

                var writtenKeys = new HashSet<string>();
                foreach (var row in rows)
                {
                    var key = row.textKey;
                    if (string.IsNullOrEmpty(key) || !writtenKeys.Add(key))
                    {
                        continue;
                    }

                    if (keySourceMap.TryGetValue(key, out var sm) &&
                        sm.TryGetValue(lang, out var src) &&
                        src != selectedEntry.tsvBaseName)
                    {
                        continue;
                    }

                    if (row.langData.TryGetValue(lang, out var text))
                    {
                        existing[key] = text;
                    }
                }

                var sb = new StringBuilder("key\ttext\n");
                foreach (var (key, text) in existing)
                {
                    sb.Append($"{key}\t{SanitizeTsvValue(text)}\n");
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }

            if (globalTsvDirty)
            {
                SaveGlobalTsv(folder);
                deletedGlobalTsvKeys.Clear();
                globalTsvDirty = false;
            }

            foreach (var (baseName, keysToDelete) in deletedOtherTsvKeys)
            {
                foreach (var lang in languages)
                {
                    var otherFilePath = Path.Combine(folder, $"{baseName}.{lang}.tsv");
                    if (!File.Exists(otherFilePath))
                    {
                        continue;
                    }

                    var existing = new Dictionary<string, string>();
                    foreach (var line in File.ReadAllLines(otherFilePath).Skip(1))
                    {
                        var trimmed = line.TrimEnd('\r');
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        var tab = trimmed.IndexOf('\t');
                        if (tab < 0)
                        {
                            continue;
                        }

                        existing[trimmed.Substring(0, tab)] = trimmed.Substring(tab + 1);
                    }

                    foreach (var k in keysToDelete)
                    {
                        existing.Remove(k);
                    }

                    var otherSb = new StringBuilder("key\ttext\n");
                    foreach (var (k, v) in existing)
                    {
                        otherSb.Append($"{k}\t{SanitizeTsvValue(v)}\n");
                    }

                    File.WriteAllText(otherFilePath, otherSb.ToString(), Encoding.UTF8);
                }
            }

            deletedOtherTsvKeys.Clear();

            AssetDatabase.Refresh();

            foreach (var row in rows)
            {
                row.MarkSaved();
            }

            isDirty = false;
        }

        void SaveGlobalTsv(string folder)
        {
            foreach (var lang in languages)
            {
                var filePath = Path.Combine(folder, $"Global.{lang}.tsv");
                var sb = new StringBuilder("key\ttext\n");
                var writtenKeys = new HashSet<string>();

                foreach (var (key, langSources) in keySourceMap)
                {
                    if (deletedGlobalTsvKeys.Contains(key))
                    {
                        continue;
                    }

                    if (!langSources.TryGetValue(lang, out var src) || src != "Global")
                    {
                        continue;
                    }

                    if (!writtenKeys.Add(key))
                    {
                        continue;
                    }

                    var text = string.Empty;
                    var refRow = rows.FirstOrDefault(r => r.textKey == key);
                    if (refRow != null && refRow.langData.TryGetValue(lang, out var rowText))
                    {
                        text = rowText;
                    }
                    else if (allTsvData.TryGetValue(key, out var langMap) && langMap.TryGetValue(lang, out var tsvText))
                    {
                        text = tsvText;
                    }

                    sb.Append($"{key}\t{SanitizeTsvValue(text)}\n");
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────

        string GetTextKeyPreview(string key)
        {
            if (!allTsvData.TryGetValue(key, out var langMap) || langMap.Count == 0)
            {
                return null;
            }

            var userLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (langMap.TryGetValue(userLang, out var userText) && !string.IsNullOrEmpty(userText))
            {
                return $"({userLang}: {userText})";
            }

            if (langMap.TryGetValue("en", out var enText) && !string.IsNullOrEmpty(enText))
            {
                return $"(en: {enText})";
            }

            foreach (var (lang, text) in langMap)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    return $"({lang}: {text})";
                }
            }

            return null;
        }

        static string ReadTextKey(LHTextMeshPro comp)
        {
            var so = new SerializedObject(comp);
            return so.FindProperty("textKey")?.stringValue ?? string.Empty;
        }

        static string ReadPlaceholderText(LHTextMeshPro comp)
        {
            var so = new SerializedObject(comp);
            return so.FindProperty("m_text")?.stringValue ?? string.Empty;
        }

        static string GetHierarchyPath(GameObject go)
        {
            var parts = new List<string>();
            var t = go.transform;
            while (t != null)
            {
                parts.Insert(0, t.name);
                t = t.parent;
            }

            return string.Join("/", parts);
        }

        static string SanitizeTsvValue(string value)
        {
            return value.Replace("\r\n", "\\n").Replace("\r", "\\n").Replace("\n", "\\n");
        }

        static string ToAbsolutePath(string unityPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", unityPath))
                .Replace('\\', '/');
        }

        // ─────────────────────────────────────────────────────────────────
        // Types
        // ─────────────────────────────────────────────────────────────────

        enum TableViewTab
        {
            LHTextMeshPro,
            TextKey
        }

        struct BackgroundColorScope : IDisposable
        {
            readonly Color previous;

            public BackgroundColorScope(Color color)
            {
                previous = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose()
            {
                GUI.backgroundColor = previous;
            }
        }
    }
}
