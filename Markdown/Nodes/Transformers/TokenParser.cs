using System.Text;
using Markdown.Nodes.Rules;
using Markdown.Tokens;

namespace Markdown.Nodes.Transformers;

public static class TokenParser
{
    private static readonly IReadOnlyList<IBlockRule> BlockRules =
    [
        new HeaderRule(),
        new ListItemRule(),
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
        var listBuffer = new List<UnorderedListItemBlock>();

        foreach (var tokenLine in tokens.SplitByParagraphBreak())
        {
            var blockRule = MatchBlock(tokenLine);

            var inlineNodes = ParseTokenLine(blockRule.StripTokenLine(tokenLine));
            var block = blockRule.Create(inlineNodes);

            if (block is UnorderedListItemBlock listItem)
            {
                listBuffer.Add(listItem);
                continue;
            }

            FlushListItems(listBuffer, nodes);
            nodes.Add(block);
        }

        FlushListItems(listBuffer, nodes);

        return nodes;
    }

    private static void FlushListItems(List<UnorderedListItemBlock> listBuffer, List<BlockNode> nodes)
    {
        if (listBuffer.Count > 0)
        {
            nodes.Add(new UnorderedListBlock([.. listBuffer]));
            listBuffer.Clear();
        }
    }

    private static IReadOnlyList<TextNode> ParseTokenLine(IReadOnlyList<Token> tokenLine)
    {
        var pairs = tokenLine.FindPairs(InlineRules);

        return ParseRange(0, tokenLine.Count);


        IReadOnlyList<TextNode> ParseRange(int startIndex, int endExclusive)
        {
            var buffer = new StringBuilder();
            var nodes = new List<TextNode>();


            for (var i = startIndex; i < endExclusive; i++)
            {
                if (pairs.TryGetValue(i, out var pairEnd) && HasContent(i, pairEnd))
                {
                    FlushBuffer();
                    buffer.Clear();
                    var insideNodes = ParseRange(i + 1, pairEnd.CloseIndex);
                    nodes.Add(pairEnd.Rule.Create(insideNodes));
                    i = pairEnd.CloseIndex;
                    continue;
                }

                buffer.Append(tokenLine[i].Value);
            }

            FlushBuffer();
            return nodes;


            void FlushBuffer()
            {
                if (buffer.Length > 0)
                    nodes.Add(new Text(buffer.ToString()));
            }
        }
    }


    private static bool HasContent(int pairStart, PairEnd pairEnd) => pairEnd.CloseIndex > pairStart + 1;
}