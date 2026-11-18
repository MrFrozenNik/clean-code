using System.Text;
using Markdown.Tokens;

namespace Markdown.Nodes.Rules;

public interface IPairingRule
{
    bool CanHandle(Token token);
    bool CanPair(PairedToken open, PairedToken close, IReadOnlyList<Token> content);
    bool AllowsChild();
    TextNode Create(IReadOnlyList<TextNode> children);
}

public sealed class StrongRule : IPairingRule
{
    public bool CanHandle(Token token) => token.Type == TokenType.DoubleUnderscore;
    public bool AllowsChild() => true;
    public bool CanPair(PairedToken open, PairedToken close, IReadOnlyList<Token> content)=>
        content.HasContent() && !PairingChecks.IsAcrossWords(open, close, content);

    public TextNode Create(IReadOnlyList<TextNode> children) => new Strong(children);
}

public sealed class EmRule : IPairingRule
{
    public bool CanHandle(Token token) => token.Type == TokenType.Underscore;

    public bool CanPair(PairedToken open, PairedToken close, IReadOnlyList<Token> content) =>
        content.HasContent() && !PairingChecks.IsAcrossWords(open, close, content);

    public bool AllowsChild() => false;

    public TextNode Create(IReadOnlyList<TextNode> children)
    {
        var sb = new StringBuilder();
        foreach (var child in children)
        {
            if (child is not Text text)
                throw new InvalidOperationException($"Emphasis can contain only text, got {child.GetType().Name}");
            sb.Append(text.Value);
        }

        return new Emphasis(sb.ToString());
    }
}

internal static class PairingChecks
{
    public static bool HasContent(this IReadOnlyList<Token> content) => content.Count > 0;

    public static bool IsAcrossWords(PairedToken open, PairedToken close, IReadOnlyList<Token> content)
        => open.IsInsideWord && close.IsInsideWord && content.ContainsWhiteSpace();

    private static bool ContainsWhiteSpace(this IReadOnlyList<Token> content)
        => content.OfType<TextToken>().Any(token => token.Value.Any(char.IsWhiteSpace));
}