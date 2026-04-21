using R3;

namespace LighthouseExtends.TextTable
{
    /// <summary>
    /// Resolves localized text by key for the current language.
    /// CurrentLanguage is exposed as a ReactiveProperty so LHTextMeshPro can subscribe to language changes.
    /// </summary>
    public interface ITextTableService
    {
        ReadOnlyReactiveProperty<string> CurrentLanguage { get; }
        string GetText(ITextData textData);
    }
}
