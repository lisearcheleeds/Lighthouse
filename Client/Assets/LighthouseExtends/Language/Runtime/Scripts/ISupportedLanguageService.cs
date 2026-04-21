using System.Collections.Generic;

namespace LighthouseExtends.Language
{
    /// <summary>
    /// Provides the list of supported language codes and the default language code.
    /// Intended for use by UI components that enumerate available language options.
    /// </summary>
    public interface ISupportedLanguageService
    {
        IReadOnlyList<string> SupportedLanguages { get; }
        string DefaultLanguage { get; }
    }
}
