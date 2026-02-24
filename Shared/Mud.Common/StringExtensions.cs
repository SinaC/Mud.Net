using System.Text;

namespace Mud.Common;

public static class StringExtensions
{
    public static string AfterLast(this string s, char c)
        => s[(s.LastIndexOf(c) + 1)..];

    public static string CenterText(this string text, int length)
    {
        if (text.Length >= length)
            return text;
        var space = length - text.Length;
        var left = space / 2;
        //int right = space/2 + (space%2);
        return text.PadLeft(left + text.Length).PadRight(length);
    }

    public static string UpperFirstLetter(this string text, string ifNull = "???")
    {
        if (text == null)
            return ifNull;

        if (text.Length > 1)
            return char.ToUpper(text[0]) + text.Substring(1);

        return text.ToUpper();
    }

    public static IEnumerable<string> Tokenize(this string expression, bool preserveQuote)
    {
        if (string.IsNullOrWhiteSpace(expression))
            yield break;
        var sb = new StringBuilder();
        bool inQuote = false;
        foreach (char c in expression)
        {
            if ((c == '"' || c == '\'') && !inQuote)
            {
                inQuote = true;
                continue;
            }
            if (c != '"' && c != '\'' && !(char.IsWhiteSpace(c) && !inQuote))
            {
                sb.Append(c);
                continue;
            }
            if (sb.Length > 0)
            {
                if (inQuote && preserveQuote)
                {
                    sb.Insert(0, '\'');
                    sb.Append('\'');
                }
                var result = sb.ToString();
                sb.Clear();
                inQuote = false;
                yield return result;
            }
        }
        if (sb.Length > 0)
            yield return sb.ToString();
    }

    public static string MaxLength(this string input, int length)
        => input?[..Math.Min(length, input.Length)] ?? string.Empty;

    public static string ToPascalCase(this string s)
        => string.Join(" ", s.Split(' ').Select(token => TokenToSkipForPascalCase.Contains(token) ? token : char.ToUpperInvariant(token[0]) + token[1..]));

    private static string[] TokenToSkipForPascalCase { get; } = ["of", "in", "on", "to", "the"];
}
