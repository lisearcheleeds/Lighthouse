using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LighthouseExtends.Language;
using R3;
using TMPro;
using VContainer;

namespace LighthouseExtends.Font
{
    /// <summary>
    /// Implements IFontService. Registers with ILanguageService and updates CurrentFont
    /// from LanguageFontSettings whenever the language changes.
    /// </summary>
    public sealed class FontService : IFontService, IDisposable
    {
        public static IFontService Instance { get; private set; }

        readonly ReactiveProperty<TMP_FontAsset> currentFont = new(null);
        readonly LanguageFontSettings settings;

        public ReadOnlyReactiveProperty<TMP_FontAsset> CurrentFont => currentFont;

        [Inject]
        public FontService(LanguageFontSettings settings, ILanguageService languageService)
        {
            Instance = this;
            this.settings = settings;
            languageService.RegisterChangeHandler(OnLanguageChange);
        }

        public void Dispose()
        {
            currentFont.Dispose();
            Instance = null;
        }

        public TMP_FontAsset GetFont(string languageCode)
        {
            return settings.GetFont(languageCode);
        }

        UniTask OnLanguageChange(string languageCode, CancellationToken cancellationToken)
        {
            currentFont.Value = settings.GetFont(languageCode);
            return UniTask.CompletedTask;
        }
    }
}
