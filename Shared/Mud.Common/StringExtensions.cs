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

    public static string MaxLength(this string input, int length)
        => input?[..Math.Min(length, input.Length)] ?? string.Empty;

    public static string ToPascalCase(this string s)
        => string.Join(" ", s.Split(' ').Select(token => TokenToSkipForPascalCase.Contains(token) ? token : char.ToUpperInvariant(token[0]) + token[1..]));

    private static string[] TokenToSkipForPascalCase { get; } = ["of", "in", "on", "to", "the"];
}
