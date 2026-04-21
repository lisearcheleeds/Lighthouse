using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LighthouseExtends.TextTable.Editor
{
    /// <summary>
    /// Static utility for reading TSV files and detecting available language codes from the TSV folder.
    /// Shared by TextTableEditorWindow and the CheckMissing operation.
    /// </summary>
    public static class TextTableTsvRepository
    {
        public static List<string> DetectLanguages(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return new List<string>();
            }

            return Directory.GetFiles(folder, "*.tsv")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(n => n.Contains('.'))
                .Select(n => n.Substring(n.LastIndexOf('.') + 1))
                .Distinct()
                .OrderBy(l => l)
                .ToList();
        }

        public static (Dictionary<string, Dictionary<string, string>> data,
            Dictionary<string, Dictionary<string, string>> sourceMap,
            List<string> errors)
            LoadAllTsvData(string folder)
        {
            var data = new Dictionary<string, Dictionary<string, string>>();
            var sourceMap = new Dictionary<string, Dictionary<string, string>>();
            var errors = new List<string>();

            if (!Directory.Exists(folder))
            {
                return (data, sourceMap, errors);
            }

            foreach (var file in Directory.GetFiles(folder, "*.tsv"))
            {
                try
                {
                    var nameNoExt = Path.GetFileNameWithoutExtension(file);
                    var dot = nameNoExt.LastIndexOf('.');
                    if (dot < 0)
                    {
                        continue;
                    }

                    var lang = nameNoExt.Substring(dot + 1);
                    var baseName = nameNoExt.Substring(0, dot);

                    foreach (var line in File.ReadAllLines(file).Skip(1))
                    {
                        var trimmed = line.TrimEnd('\r');
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        var tab = trimmed.IndexOf('\t');
                        if (tab < 0)
                        {
                            continue;
                        }

                        var key = trimmed.Substring(0, tab);
                        var text = trimmed.Substring(tab + 1);

                        if (!data.ContainsKey(key))
                        {
                            data[key] = new Dictionary<string, string>();
                        }

                        data[key][lang] = text;

                        if (!sourceMap.ContainsKey(key))
                        {
                            sourceMap[key] = new Dictionary<string, string>();
                        }

                        sourceMap[key][lang] = baseName;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(
                        $"'{Path.GetFileName(file)}': {e.Message} (Fix: Ensure the file is not open in another app and is encoded as UTF-8)");
                    Debug.LogException(e);
                }
            }

            return (data, sourceMap, errors);
        }
    }
}
