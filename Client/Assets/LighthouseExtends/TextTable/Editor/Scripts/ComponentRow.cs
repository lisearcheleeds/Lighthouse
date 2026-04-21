using System.Collections.Generic;
using LighthouseExtends.TextMeshPro;

namespace LighthouseExtends.TextTable.Editor
{
    /// <summary>
    /// Represents one row in the TextTableEditorWindow table, corresponding to a single LHTextMeshPro component.
    /// Holds the hierarchy path, text key, per-language text data, and tracks unsaved key changes via IsTextKeyDirty.
    /// </summary>
    public class ComponentRow
    {
        public readonly LHTextMeshPro component;
        public readonly string hierarchyPath;
        public readonly Dictionary<string, string> langData;
        public readonly string placeholderText;
        string savedTextKey;
        public string textKey;

        public ComponentRow(string hierarchyPath, string textKey, string placeholderText,
            LHTextMeshPro component, Dictionary<string, string> langData)
        {
            this.hierarchyPath = hierarchyPath;
            this.textKey = textKey;
            savedTextKey = textKey;
            this.placeholderText = placeholderText;
            this.component = component;
            this.langData = langData;
        }

        public bool IsTextKeyDirty => textKey != savedTextKey;

        public void MarkSaved()
        {
            savedTextKey = textKey;
        }
    }
}
