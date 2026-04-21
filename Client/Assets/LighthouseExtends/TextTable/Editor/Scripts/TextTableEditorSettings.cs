using UnityEngine;

namespace LighthouseExtends.TextTable.Editor
{
    /// <summary>
    /// ScriptableObject that stores the TSV folder path used by TextTableEditorWindow and TextTableGenerator.
    /// </summary>
    public class TextTableEditorSettings : ScriptableObject
    {
        [SerializeField] string textTableFolderPath = "Assets/StreamingAssets/TextTables";

        public string TextTableFolderPath => textTableFolderPath;
    }
}
