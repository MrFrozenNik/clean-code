using Markdown.Tokens;

namespace Markdown.Nodes.Rules;

public interface IBlockRule
{
    bool Matches(IReadOnlyList<Token> line);
    IReadOnlyList<Token> StripTokenLine(IReadOnlyList<Token> line);
    BlockNode Create(IReadOnlyList<TextNode> inlineNodes);
}

public sealed class HeaderRule : IBlockRule
{
    public bool Matches(IReadOnlyList<Token> line) => line is [HeaderMarkerToken, ..];
    public IReadOnlyList<Token> StripTokenLine(IReadOnlyList<Token> line) => [.. line.Skip(1)];
    public BlockNode Create(IReadOnlyList<TextNode> inlineNodes) => new HeaderBlock(inlineNodes);
}


public sealed class ListItemRule : IBlockRule
{
    public bool Matches(IReadOnlyList<Token> line) => line is [ListItemMarkerToken, ..];
    public IReadOnlyList<Token> StripTokenLine(IReadOnlyList<Token> line) => [.. line.Skip(1)];
    public BlockNode Create(IReadOnlyList<TextNode> inlineNodes) => new UnorderedListItemBlock(inlineNodes);
}



public sealed class ParagraphRule : IBlockRule
{
    public bool Matches(IReadOnlyList<Token> line) => true;
    public IReadOnlyList<Token> StripTokenLine(IReadOnlyList<Token> line) => line;
    public BlockNode Create(IReadOnlyList<TextNode> inlineNodes) => new TextBlock(inlineNodes);
}