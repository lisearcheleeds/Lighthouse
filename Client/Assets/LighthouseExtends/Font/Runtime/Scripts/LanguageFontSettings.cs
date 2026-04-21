using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LighthouseExtends.Font
{
    /// <summary>
    /// ScriptableObject that maps language codes to TMP_FontAssets.
    /// Returns the default font when no entry matches the requested language code.
    /// </summary>
    [CreateAssetMenu(menuName = "Lighthouse/Font/Language Font Settings")]
    public class LanguageFontSettings : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string languageCode;
            public TMP_FontAsset fontAsset;
        }

        [SerializeField] List<Entry> entries = new();
        [SerializeField] TMP_FontAsset defaultFont;

        public TMP_FontAsset GetFont(string languageCode)
        {
            foreach (var entry in entries)
            {
                if (entry.languageCode == languageCode)
                {
                    return entry.fontAsset;
                }
            }

            return defaultFont;
        }
    }
}
