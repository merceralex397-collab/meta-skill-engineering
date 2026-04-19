using System;
using System.Text.RegularExpressions;

namespace MetaSkillStudio.Helpers
{
    /// <summary>
    /// Cache for compiled regex patterns with timeouts to avoid repeated compilation and prevent excessive backtracking.
    /// </summary>
    public static class RegexCache
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(100);

        // Model extraction patterns (PythonRuntimeService.cs - ExtractModelsFromOutput)
        public static readonly Regex ModelIdentifier = new Regex(
            @"[A-Za-z0-9][A-Za-z0-9._:/-]{2,}",
            RegexOptions.Compiled,
            DefaultTimeout);

        public static readonly Regex NumericOnlyPattern = new Regex(
            @"^[0-9.]+$",
            RegexOptions.Compiled,
            DefaultTimeout);

        // Skill description extraction patterns (PythonRuntimeService.cs - ExtractSkillDescription)
        public static readonly Regex FrontmatterDescription = new Regex(
            @"description:\s*(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline,
            DefaultTimeout);

        public static readonly Regex PurposeSection = new Regex(
            @"## Purpose\s*(.+?)(?=\n\n|\n##|$)",
            RegexOptions.Compiled | RegexOptions.Singleline,
            DefaultTimeout);

        // Judge output parsing patterns (PythonRuntimeService.cs - ParseJudgeOutput)
        public static readonly Regex QualityScore = new Regex(
            @"quality score.*?[:\s]+(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            DefaultTimeout);

        public static readonly Regex RoutingQualityNotes = new Regex(
            @"routing quality notes.*?[:\n]+(.*?)\n(?=\n\d+\.|\n[A-Z]|$)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline,
            DefaultTimeout);

        public static readonly Regex BehaviorQualityNotes = new Regex(
            @"behavior quality notes.*?[:\n]+(.*?)\n(?=\n\d+\.|\n[A-Z]|$)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline,
            DefaultTimeout);

        public static readonly Regex HighestPriorityFixes = new Regex(
            @"highest priority fixes.*?[:\n]+(.*?)\n(?=\n\d+\.|\n[A-Z]|$)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline,
            DefaultTimeout);

        // Static char arrays for trimming (avoids allocation on every call)
        public static readonly char[] TrimChars = new[] { '.', ',', ':', ';', '[', ']', '(', ')', '{', '}', '<', '>', '"', '\'' };
    }
}
