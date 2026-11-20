namespace Markdown.Tokens.Lexing;

public sealed record Symbol(string Pattern, bool LineStartOnly = false);

public static class MarkdownSyntax
{
    public const string HeaderPrefix = "# ";
    public const string ParagraphBreaker = "\n";
    public const string ListItemPrefix = "- ";
    
    public const string DoubleUnderscore = "__";
    public const string Underscore = "_";


    public static readonly IReadOnlyList<Symbol> Symbols =
    [
        new(HeaderPrefix, true),
        new(ListItemPrefix, true),
        new(DoubleUnderscore),
        new(Underscore),
    ];

    public static bool IsEscapable(char character) => character == '\\' || Symbols.Any(s => s.Pattern[0] == character);
}