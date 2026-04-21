using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LighthouseExtends.TextTable.Editor
{
    /// <summary>
    /// Editor tool that scans all .cs files for TextData.CreateTextData calls, writes TSV entries,
    /// and rewrites each call to a plain new TextData(...) constructor.
    /// Only static string literal arguments can be processed; dynamic expressions are skipped with a warning.
    /// </summary>
    public static class TextTableGenerator
    {
        static readonly Regex CallRegex = new Regex(@"TextData\.CreateTextData\s*\(");

        [MenuItem("Lighthouse/TextTable/Generate TextTable raw from scripts")]
        public static void Generate()
        {
            var settings = LoadSettings();
            if (settings == null)
            {
                Debug.LogError("[TextTableGenerator] TextTableEditorSettings not found. Open the TextTable Editor window first.");
                return;
            }

            var tsvFolder = ToAbsolutePath(settings.TextTableFolderPath);
            if (!Directory.Exists(tsvFolder))
            {
                Debug.LogError($"[TextTableGenerator] TSV folder not found: '{tsvFolder}'. Check TextTableEditorSettings.");
                return;
            }

            var csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            var replaced = 0;
            var skipped = 0;
            var errors = 0;

            foreach (var csFile in csFiles)
            {
                ProcessFile(csFile, tsvFolder, ref replaced, ref skipped, ref errors);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[TextTableGenerator] Complete — Replaced: {replaced}, Skipped (dynamic text): {skipped}, Errors: {errors}");
        }

        // ─────────────────────────────────────────────────────────────────
        // File processing

        static void ProcessFile(string filePath, string tsvFolder, ref int replaced, ref int skipped, ref int errors)
        {
            var source = File.ReadAllText(filePath, Encoding.UTF8);
            if (!source.Contains("CreateTextData"))
            {
                return;
            }

            var replacements = new List<(int start, int end, string newText)>();
            var stringMask = BuildStringCommentMask(source);

            var match = CallRegex.Match(source);
            while (match.Success)
            {
                var openParen = match.Index + match.Length - 1;

                // Skip matches inside string literals or comments
                if (stringMask[match.Index])
                {
                    match = match.NextMatch();
                    continue;
                }

                if (!TryParseArguments(source, openParen, out var args, out var closeParen))
                {
                    var line = GetLineNumber(source, match.Index);
                    Debug.LogError($"[TextTableGenerator] Failed to parse arguments at '{Path.GetFileName(filePath)}':{line}");
                    errors++;
                    match = match.NextMatch();
                    continue;
                }

                if (args.Count < 3)
                {
                    var line = GetLineNumber(source, match.Index);
                    Debug.LogError($"[TextTableGenerator] Expected at least 3 arguments at '{Path.GetFileName(filePath)}':{line}");
                    errors++;
                    match = match.NextMatch();
                    continue;
                }

                var categoryArg = args[0].Trim();
                var textKeyArg = args[1].Trim();
                var textArg = args[2].Trim();
                var textParamsArg = args.Count > 3 ? args[3].Trim() : null;
                var lineNum = GetLineNumber(source, match.Index);

                if (!TryExtractStringLiteral(categoryArg, out var category))
                {
                    Debug.LogError(
                        $"[TextTableGenerator] '{Path.GetFileName(filePath)}':{lineNum} — " +
                        $"category argument '{categoryArg}' is not a string literal. Cannot generate TSV entry.");
                    errors++;
                    match = match.NextMatch();
                    continue;
                }

                if (!TryExtractStringLiteral(textKeyArg, out var textKey))
                {
                    Debug.LogError(
                        $"[TextTableGenerator] '{Path.GetFileName(filePath)}':{lineNum} — " +
                        $"textKey argument '{textKeyArg}' is not a string literal. Cannot generate TSV entry.");
                    errors++;
                    match = match.NextMatch();
                    continue;
                }

                if (!TryExtractStringLiteral(textArg, out var text))
                {
                    var shortExpr = textArg.Length > 60 ? textArg.Substring(0, 60) + "..." : textArg;
                    Debug.LogWarning(
                        $"[TextTableGenerator] '{Path.GetFileName(filePath)}':{lineNum} — " +
                        $"Cannot extract text for key '{textKey}' (category: '{category}').\n" +
                        $"Text argument '{shortExpr}' is a dynamic expression that cannot be resolved at edit time.\n\n" +
                        $"To generate this entry, replace the dynamic expression with a static template string\n" +
                        $"and pass the runtime value via textParams:\n\n" +
                        $"  TextData.CreateTextData(\"{category}\", \"{textKey}\", \"your text with {{paramName}}\",\n" +
                        $"      new Dictionary<string, object> {{ {{ \"paramName\", yourRuntimeValue }} }})\n\n" +
                        $"The {{{{paramName}}}} placeholder will be substituted at runtime by TextTableService.GetText().");
                    skipped++;
                    match = match.NextMatch();
                    continue;
                }

                var prefixedKey = category + textKey;
                WriteTsvEntry(tsvFolder, category, prefixedKey, text, Path.GetFileName(filePath), lineNum);

                var newCall = textParamsArg != null
                    ? $"new TextData(\"{prefixedKey}\", {textParamsArg})"
                    : $"new TextData(\"{prefixedKey}\")";

                replacements.Add((match.Index, closeParen + 1, newCall));
                replaced++;

                match = match.NextMatch();
            }

            if (replacements.Count == 0)
            {
                return;
            }

            replacements.Sort((a, b) => b.start.CompareTo(a.start));
            var sb = new StringBuilder(source);
            foreach (var (start, end, newText) in replacements)
            {
                sb.Remove(start, end - start);
                sb.Insert(start, newText);
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        // ─────────────────────────────────────────────────────────────────
        // Argument parsing

        static bool TryParseArguments(string source, int openParen, out List<string> args, out int closeParen)
        {
            args = new List<string>();
            closeParen = -1;

            var pos = openParen + 1;
            var depth = 1;
            var argStart = pos;

            while (pos < source.Length)
            {
                var c = source[pos];

                if (IsStringStart(source, pos))
                {
                    pos = SkipStringLiteral(source, pos);
                    continue;
                }

                if (c == '\'')
                {
                    pos = SkipCharLiteral(source, pos);
                    continue;
                }

                if (c == '/' && pos + 1 < source.Length)
                {
                    if (source[pos + 1] == '/')
                    {
                        var nl = source.IndexOf('\n', pos);
                        pos = nl >= 0 ? nl : source.Length;
                        continue;
                    }

                    if (source[pos + 1] == '*')
                    {
                        var end = source.IndexOf("*/", pos + 2, System.StringComparison.Ordinal);
                        pos = end >= 0 ? end + 2 : source.Length;
                        continue;
                    }
                }

                if (c == '(' || c == '[' || c == '{')
                {
                    depth++;
                }
                else if (c == ')' || c == ']' || c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        var arg = source.Substring(argStart, pos - argStart);
                        if (args.Count > 0 || arg.Trim().Length > 0)
                        {
                            args.Add(arg);
                        }

                        closeParen = pos;
                        return true;
                    }
                }
                else if (c == ',' && depth == 1)
                {
                    args.Add(source.Substring(argStart, pos - argStart));
                    argStart = pos + 1;
                }

                pos++;
            }

            return false;
        }

        static bool IsStringStart(string source, int pos)
        {
            var c = source[pos];
            if (c == '"') { return true; }
            if (c == '@' && pos + 1 < source.Length && source[pos + 1] == '"') { return true; }
            if (c == '@' && pos + 2 < source.Length && source[pos + 1] == '$' && source[pos + 2] == '"') { return true; }
            if (c == '$' && pos + 1 < source.Length && source[pos + 1] == '"') { return true; }
            if (c == '$' && pos + 2 < source.Length && source[pos + 1] == '@' && source[pos + 2] == '"') { return true; }
            return false;
        }

        // Returns position after the closing " of the string.
        static int SkipStringLiteral(string source, int pos)
        {
            var isVerbatim = false;
            var isInterpolated = false;

            while (pos < source.Length && source[pos] != '"')
            {
                if (source[pos] == '@') { isVerbatim = true; }
                if (source[pos] == '$') { isInterpolated = true; }
                pos++;
            }

            pos++; // skip opening "

            if (isInterpolated)
            {
                while (pos < source.Length)
                {
                    var c = source[pos];
                    if (c == '"') { return pos + 1; }
                    if (c == '{' && pos + 1 < source.Length && source[pos + 1] == '{') { pos += 2; continue; }
                    if (c == '}' && pos + 1 < source.Length && source[pos + 1] == '}') { pos += 2; continue; }
                    if (c == '{')
                    {
                        pos++;
                        var depth = 1;
                        while (pos < source.Length && depth > 0)
                        {
                            var ic = source[pos];
                            if (IsStringStart(source, pos))
                            {
                                pos = SkipStringLiteral(source, pos);
                                continue;
                            }

                            if (ic == '{' || ic == '(' || ic == '[') { depth++; }
                            else if (ic == '}' || ic == ')' || ic == ']') { depth--; }

                            if (depth > 0) { pos++; }
                        }

                        if (pos < source.Length) { pos++; } // skip closing }

                        continue;
                    }

                    if (!isVerbatim && c == '\\') { pos += 2; continue; }

                    pos++;
                }
            }
            else if (isVerbatim)
            {
                while (pos < source.Length)
                {
                    if (source[pos] == '"')
                    {
                        pos++;
                        if (pos < source.Length && source[pos] == '"') { pos++; continue; }
                        return pos;
                    }

                    pos++;
                }
            }
            else
            {
                while (pos < source.Length)
                {
                    var c = source[pos];
                    if (c == '"') { return pos + 1; }
                    if (c == '\\') { pos += 2; continue; }
                    pos++;
                }
            }

            return pos;
        }

        static int SkipCharLiteral(string source, int pos)
        {
            pos++; // skip '
            if (pos < source.Length && source[pos] == '\\') { pos++; } // skip escape char

            pos++; // skip the char itself
            if (pos < source.Length && source[pos] == '\'') { pos++; } // skip closing '
            return pos;
        }

        // ─────────────────────────────────────────────────────────────────
        // String literal extraction

        // Extracts the runtime value of a C# string literal argument.
        // Returns false if the argument is not a plain string literal (e.g. variable, interpolation with expressions).
        static bool TryExtractStringLiteral(string arg, out string value)
        {
            value = null;
            arg = arg.Trim();

            var pos = 0;
            var isVerbatim = false;
            var isInterpolated = false;

            while (pos < arg.Length && arg[pos] != '"')
            {
                if (arg[pos] == '@') { isVerbatim = true; }
                if (arg[pos] == '$') { isInterpolated = true; }
                pos++;
            }

            if (pos >= arg.Length) { return false; }

            pos++; // skip opening "

            if (isInterpolated)
            {
                // Reject if any non-escaped { exists (i.e. actual interpolation expression)
                var scanPos = pos;
                while (scanPos < arg.Length)
                {
                    var c = arg[scanPos];
                    if (c == '{')
                    {
                        if (scanPos + 1 < arg.Length && arg[scanPos + 1] == '{') { scanPos += 2; continue; }
                        return false; // Contains expression
                    }

                    if (c == '}' && scanPos + 1 < arg.Length && arg[scanPos + 1] == '}') { scanPos += 2; continue; }
                    if (c == '"') { break; }
                    if (!isVerbatim && c == '\\') { scanPos += 2; continue; }
                    scanPos++;
                }
            }

            var sb = new StringBuilder();

            if (isVerbatim)
            {
                while (pos < arg.Length)
                {
                    var c = arg[pos];
                    if (c == '"')
                    {
                        if (pos + 1 < arg.Length && arg[pos + 1] == '"') { sb.Append('"'); pos += 2; continue; }
                        break;
                    }

                    // For $@ strings, {{ }} are escaped braces
                    if (isInterpolated && c == '{' && pos + 1 < arg.Length && arg[pos + 1] == '{') { sb.Append('{'); pos += 2; continue; }
                    if (isInterpolated && c == '}' && pos + 1 < arg.Length && arg[pos + 1] == '}') { sb.Append('}'); pos += 2; continue; }

                    sb.Append(c);
                    pos++;
                }
            }
            else
            {
                while (pos < arg.Length)
                {
                    var c = arg[pos];
                    if (c == '"') { break; }
                    if (c == '\\' && pos + 1 < arg.Length)
                    {
                        pos++;
                        switch (arg[pos])
                        {
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '0': sb.Append('\0'); break;
                            case '{': sb.Append('{'); break; // for $" strings
                            case '}': sb.Append('}'); break;
                            default: sb.Append('\\'); sb.Append(arg[pos]); break;
                        }

                        pos++;
                        continue;
                    }

                    // For $ strings, {{ and }} are escaped braces
                    if (isInterpolated && c == '{' && pos + 1 < arg.Length && arg[pos + 1] == '{') { sb.Append('{'); pos += 2; continue; }
                    if (isInterpolated && c == '}' && pos + 1 < arg.Length && arg[pos + 1] == '}') { sb.Append('}'); pos += 2; continue; }

                    sb.Append(c);
                    pos++;
                }
            }

            value = sb.ToString();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────
        // TSV writing

        static void WriteTsvEntry(string tsvFolder, string category, string textKey, string text, string sourceFile, int lineNum)
        {
            var files = Directory.GetFiles(tsvFolder, $"{category}.*.tsv");

            if (files.Length == 0)
            {
                var languages = DetectLanguages(tsvFolder);
                if (languages.Count == 0)
                {
                    Debug.LogWarning(
                        $"[TextTableGenerator] '{sourceFile}':{lineNum} — " +
                        $"No TSV files found in '{tsvFolder}'. Cannot detect languages. " +
                        $"Create at least one TSV file via the TextTable Editor window, then re-run.");
                    return;
                }

                var sanitized = SanitizeTsvValue(text);
                foreach (var lang in languages)
                {
                    var newPath = Path.Combine(tsvFolder, $"{category}.{lang}.tsv");
                    File.WriteAllText(newPath, $"key\ttext\n{textKey}\t{sanitized}\n", Encoding.UTF8);
                    Debug.Log($"[TextTableGenerator] Created '{category}.{lang}.tsv' and added key '{textKey}'.");
                }

                return;
            }

            foreach (var filePath in files)
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var lines = content.Split('\n').ToList();

                var keyExists = lines.Skip(1).Any(line =>
                {
                    var trimmed = line.TrimEnd('\r');
                    var tabIdx = trimmed.IndexOf('\t');
                    if (tabIdx < 0) { return false; }
                    return trimmed.Substring(0, tabIdx).Trim('\0', '\uFEFF') == textKey;
                });

                if (keyExists)
                {
                    Debug.LogWarning(
                        $"[TextTableGenerator] Key '{textKey}' already exists in '{Path.GetFileName(filePath)}'. " +
                        $"Skipping TSV write, but replacing the source call.");
                    continue;
                }

                var sanitized = SanitizeTsvValue(text);

                while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
                {
                    lines.RemoveAt(lines.Count - 1);
                }

                lines.Add($"{textKey}\t{sanitized}");
                File.WriteAllText(filePath, string.Join("\n", lines) + "\n", Encoding.UTF8);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Helpers

        // Returns a mask where true = position is inside a string literal or comment.
        static bool[] BuildStringCommentMask(string source)
        {
            var mask = new bool[source.Length];
            var pos = 0;

            while (pos < source.Length)
            {
                var c = source[pos];

                if (IsStringStart(source, pos))
                {
                    var start = pos;
                    pos = SkipStringLiteral(source, pos);
                    for (var i = start; i < pos && i < mask.Length; i++) { mask[i] = true; }
                    continue;
                }

                if (c == '\'')
                {
                    var start = pos;
                    pos = SkipCharLiteral(source, pos);
                    for (var i = start; i < pos && i < mask.Length; i++) { mask[i] = true; }
                    continue;
                }

                if (c == '/' && pos + 1 < source.Length)
                {
                    if (source[pos + 1] == '/')
                    {
                        var start = pos;
                        var nl = source.IndexOf('\n', pos);
                        pos = nl >= 0 ? nl : source.Length;
                        for (var i = start; i < pos && i < mask.Length; i++) { mask[i] = true; }
                        continue;
                    }

                    if (source[pos + 1] == '*')
                    {
                        var start = pos;
                        var end = source.IndexOf("*/", pos + 2, System.StringComparison.Ordinal);
                        pos = end >= 0 ? end + 2 : source.Length;
                        for (var i = start; i < pos && i < mask.Length; i++) { mask[i] = true; }
                        continue;
                    }
                }

                pos++;
            }

            return mask;
        }

        // Detects languages from existing TSV filenames (e.g. "Global.ja.tsv" → "ja").
        static IReadOnlyList<string> DetectLanguages(string tsvFolder)
        {
            return Directory.GetFiles(tsvFolder, "*.tsv")
                .Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    var dotIdx = name.LastIndexOf('.');
                    return dotIdx >= 0 ? name.Substring(dotIdx + 1) : null;
                })
                .Where(l => l != null)
                .Distinct()
                .ToList();
        }

        static TextTableEditorSettings LoadSettings()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(TextTableEditorSettings)}");
            if (guids.Length == 0) { return null; }
            return AssetDatabase.LoadAssetAtPath<TextTableEditorSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        static string ToAbsolutePath(string unityPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", unityPath)).Replace('\\', '/');
        }

        static string SanitizeTsvValue(string value)
        {
            return value.Replace("\r\n", "\\n").Replace("\r", "\\n").Replace("\n", "\\n");
        }

        static int GetLineNumber(string source, int pos)
        {
            var line = 1;
            for (var i = 0; i < pos && i < source.Length; i++)
            {
                if (source[i] == '\n') { line++; }
            }

            return line;
        }
    }
}
