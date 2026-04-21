using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace LighthouseExtends.Language
{
    /// <summary>
    /// Service interface for language switching. SetLanguage runs all registered handlers in parallel
    /// before updating CurrentLanguage, so dependents such as FontService can reload assets first.
    /// </summary>
    public interface ILanguageService
    {
        ReadOnlyReactiveProperty<string> CurrentLanguage { get; }
        void RegisterChangeHandler(Func<string, CancellationToken, UniTask> handler);
        UniTask SetLanguage(string languageCode, CancellationToken cancellationToken);
    }
}
