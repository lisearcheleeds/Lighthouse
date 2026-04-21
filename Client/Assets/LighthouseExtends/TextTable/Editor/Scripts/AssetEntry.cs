namespace LighthouseExtends.TextTable.Editor
{
    /// <summary>
    /// Data class representing a Scene or Prefab asset in the TextTableEditorWindow asset list.
    /// Holds the asset path, display name, TSV base name, and whether the asset is a scene.
    /// </summary>
    public class AssetEntry
    {
        public readonly string assetPath;
        public readonly string displayName;
        public readonly bool isScene;
        public readonly string tsvBaseName;

        public AssetEntry(string assetPath, string displayName, string tsvBaseName, bool isScene)
        {
            this.assetPath = assetPath;
            this.displayName = displayName;
            this.tsvBaseName = tsvBaseName;
            this.isScene = isScene;
        }
    }
}
