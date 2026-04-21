using R3;
using TMPro;

namespace LighthouseExtends.Font
{
    /// <summary>
    /// Provides the TMP_FontAsset for the current language as a ReactiveProperty.
    /// Also supports direct font lookup by language code via GetFont.
    /// </summary>
    public interface IFontService
    {
        ReadOnlyReactiveProperty<TMP_FontAsset> CurrentFont { get; }
        TMP_FontAsset GetFont(string languageCode);
    }
}
