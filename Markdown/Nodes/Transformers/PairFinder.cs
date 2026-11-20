using Markdown.Nodes.Rules;
using Markdown.Tokens;

namespace Markdown.Nodes.Transformers;

public static class PairFinder
{
    public static Dictionary<int, int> FindPairs(this IReadOnlyList<Token> tokens,
        IReadOnlyList<IPairingRule> inlineRules)
    {
        var pairs = new Dictionary<int, int>();
        var openerIndexes = new Stack<int>();

        for (var tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++) {
            if (tokens[tokenIndex] is not PairedToken token)
                continue;

            if (token.CanClose &&
                TryGetPairToken(tokens, openerIndexes, tokenIndex, out var closer))
            {
                var openerIndex = openerIndexes.Peek();

                if (tokens[openerIndex].Type != closer.Type)
                {
                    do
                    {
                        openerIndexes.Pop();
                    }
                    while (tokens[openerIndexes.Peek()].Type != closer.Type);

                    continue;
                }

                var opener = (PairedToken)tokens[openerIndex];
                var content = tokens.Skip(openerIndex + 1).Take(tokenIndex - openerIndex + 1).ToArray();

                var rule = closer.GetRuleFor(inlineRules);
                if (!rule.CanPair(opener, closer, content))
                    continue;

                openerIndexes.Pop();
                pairs[openerIndex] = tokenIndex;
            }

            if (token.CanOpen && CanOpenInside(tokens, openerIndexes, inlineRules)) openerIndexes.Push(tokenIndex);
        }

        return pairs;
    }

    private static bool TryGetPairToken(IReadOnlyList<Token> tokens, Stack<int> openerTokenIndexes, int closeIndex,
        out PairedToken close)
    {
        close = (PairedToken)tokens[closeIndex];
        var closeType = close.Type;
        return openerTokenIndexes.Any(ind => tokens[ind].Type == closeType);
    }

    private static IPairingRule GetRuleFor(this Token token, IReadOnlyList<IPairingRule> inlineRules) =>
        inlineRules.First(r => r.CanHandle(token));

    private static bool CanOpenInside(IReadOnlyList<Token> tokens, Stack<int> openerTokenIndexes,
        IReadOnlyList<IPairingRule> inlineRules) => openerTokenIndexes.Count == 0 ||
                                                    tokens[openerTokenIndexes.Peek()].GetRuleFor(inlineRules)
                                                        .AllowsChild();
}