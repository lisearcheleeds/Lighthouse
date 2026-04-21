using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LighthouseExtends.Language;
using R3;
using UnityEngine;
using VContainer;

namespace LighthouseExtends.TextTable
{
    /// <summary>
    /// Implements ITextTableService. Registers with ILanguageService and reloads the active TSV
    /// via ITextTableLoader on each language change. Also exposes a static Instance for access without DI.
    /// </summary>
    public sealed class TextTableService : ITextTableService, IDisposable
    {
        public static ITextTableService Instance { get; private set; }

        readonly ITextTableLoader loader;
        readonly ReactiveProperty<string> currentLanguage = new(string.Empty);

        IReadOnlyDictionary<string, string> activeTable;

        public ReadOnlyReactiveProperty<string> CurrentLanguage => currentLanguage;

        [Inject]
        public TextTableService(ITextTableLoader loader, ILanguageService languageService)
        {
            Instance = this;
            this.loader = loader;
            languageService.RegisterChangeHandler(LoadTableAsync);
        }

        public string GetText(ITextData textData)
        {
            if (activeTable == null)
            {
                return textData.TextKey;
            }

            if (!activeTable.TryGetValue(textData.TextKey, out var text))
            {
                Debug.LogWarning($"[TextTable] Key not found: '{textData.TextKey}' (language: '{currentLanguage.Value}')");
                return textData.TextKey;
            }

            if (textData.Params != null && 0 < textData.Params.Count)
            {
                text = FormatText(text, textData.Params);
            }

            return text;
        }

        public void Dispose()
        {
            currentLanguage.Dispose();
            Instance = null;
        }

        async UniTask LoadTableAsync(string languageCode, CancellationToken cancellationToken)
        {
            activeTable = await loader.LoadAsync(languageCode, cancellationToken);
            currentLanguage.Value = languageCode;
        }

        static string FormatText(string text, IReadOnlyDictionary<string, object> textParams)
        {
            foreach (var kvp in textParams)
            {
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
            }

            return text;
        }
    }
}
