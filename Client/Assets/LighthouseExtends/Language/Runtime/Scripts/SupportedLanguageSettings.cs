using System.Collections.Generic;
using UnityEngine;

namespace LighthouseExtends.Language
{
    /// <summary>
    /// ScriptableObject that holds the supported language codes and the default language code.
    /// Manage the language list in the Inspector and inject this into SupportedLanguageService.
    /// </summary>
    [CreateAssetMenu(menuName = "Lighthouse/Language/Supported Language Settings")]
    public class SupportedLanguageSettings : ScriptableObject
    {
        [SerializeField] List<string> supportedLanguages = new() { "en" };
        [SerializeField] string defaultLanguage = "en";

        public IReadOnlyList<string> SupportedLanguages => supportedLanguages;
        public string DefaultLanguage => defaultLanguage;
    }
}
