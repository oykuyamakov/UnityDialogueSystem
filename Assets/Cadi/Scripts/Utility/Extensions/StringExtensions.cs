using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Cadi.Scripts.Utility.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex s_CamelCaseRegex = new(
            @"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])",
            RegexOptions.Compiled
        );

        private static readonly Regex s_IntRegex = new(
            @"-?\d+",
            RegexOptions.Compiled
        );

        public static string Simplify(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            value = value.Trim();

            StringBuilder builder = new(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                char c = char.ToLowerInvariant(value[i]);

                builder.Append(IsSimplifiedAllowedChar(c) ? c : '_');
            }

            return builder.ToString();
        }

        public static string SplitCamelCase(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return s_CamelCaseRegex.Replace(value.Trim(), " ");
        }

        public static int ExtractInt(this string value)
        {
            if (!TryExtractInt(value, out int result))
                throw new FormatException($"String does not contain a valid int: {value}");

            return result;
        }

        public static bool TryExtractInt(this string value, out int result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            Match match = s_IntRegex.Match(value);

            return match.Success && int.TryParse(match.Value, out result);
        }

        private static bool IsSimplifiedAllowedChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '-';
        }
    }
}