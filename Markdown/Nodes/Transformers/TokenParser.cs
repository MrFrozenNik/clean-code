using Markdown.Tokens;

namespace Markdown.Nodes.Transformers;

public static class TokenParser
{
    private static readonly IReadOnlyList<IBlockRule> BlockRules =
    [
        new HeaderRule(),
        new ParagraphRule(),
    ];

    private static readonly IReadOnlyList<IPairingRule> InlineRules =
    [
        new StrongRule(),
        new EmRule(),
    ];

    private static IBlockRule MatchBlock(IReadOnlyList<Token> tokenLine)
    {
        foreach (var rule in BlockRules)
        {
            if (rule.Matches(tokenLine))
                return rule;
        }


        throw new InvalidOperationException(
            "No block rule matched; There must be an always true Matches() method containing rule inside BlockRules list");
    }


    public static IReadOnlyList<Node> Parse(IReadOnlyList<Token> tokens)
    {
        var nodes = new List<BlockNode>();

        foreach (var tokenLine in tokens.SplitByParagraphBreak())
        {
            var blockRule = MatchBlock(tokenLine);

            var inlineNodes = ParseTokenLine(blockRule.StripTokenLine(tokenLine));
            var block = blockRule.Create(inlineNodes);
            nodes.Add(block);
        }

        return nodes;
    }

    private static IReadOnlyList<TextNode> ParseTokenLine(IReadOnlyList<Token> tokenLine)
    {
        var inlineNodes = new List<TextNode>();
        foreach (var token in tokenLine)
        {
            if (token is TextToken textToken)
            {
                inlineNodes.Add(new Text(textToken.Value));
            }
        }

        return inlineNodes;
    }
}