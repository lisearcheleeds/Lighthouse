using System.Collections.Generic;

namespace LighthouseExtends.TextTable
{
    /// <summary>
    /// Holds a text key and optional runtime parameters.
    /// TextTableService.GetText substitutes {paramName} placeholders in the resolved text using Params.
    /// </summary>
    public interface ITextData
    {
        string TextKey { get; }
        IReadOnlyDictionary<string, object> Params { get; }
    }
}
