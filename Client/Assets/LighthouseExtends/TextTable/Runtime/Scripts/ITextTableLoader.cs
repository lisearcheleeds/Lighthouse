using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LighthouseExtends.TextTable
{
    /// <summary>
    /// Asynchronously loads a key-to-text dictionary for the given language code.
    /// Swap implementations to change the data source (e.g. Addressables, Resources, remote API).
    /// </summary>
    public interface ITextTableLoader
    {
        UniTask<IReadOnlyDictionary<string, string>> LoadAsync(string languageCode, CancellationToken cancellationToken);
    }
}
