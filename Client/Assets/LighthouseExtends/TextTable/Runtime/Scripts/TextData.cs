using System.Collections.Generic;

namespace LighthouseExtends.TextTable
{
    /// <summary>
    /// Concrete implementation of ITextData. The editor-only CreateTextData method is detected
    /// by TextTableGenerator to generate TSV entries and rewrite the call to a plain constructor.
    /// </summary>
    public class TextData : ITextData
    {
        public TextData(string textKey, IReadOnlyDictionary<string, object> textParams = null)
        {
            TextKey = textKey;
            Params = textParams;
        }

        public string TextKey { get; }
        public IReadOnlyDictionary<string, object> Params { get; }

#if UNITY_EDITOR
        /// <summary>
        ///     This method is for UnityEditor only.
        ///     By running the Lighthouse extension, this method call will be replaced with a constructor call,
        ///     and writing to the TextTable will be performed simultaneously.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="textKey"></param>
        /// <param name="text"></param>
        /// <param name="textParams"></param>
        /// <returns></returns>
        public static TextData CreateTextData(string category, string textKey, string text, IReadOnlyDictionary<string, object> textParams = null)
        {
            return new TextData(textKey, textParams);
        }
#endif
    }
}
