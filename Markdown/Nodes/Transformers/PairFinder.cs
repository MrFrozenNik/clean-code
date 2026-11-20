using Markdown.Nodes.Rules;
using Markdown.Tokens;

namespace Markdown.Nodes.Transformers;

public static class PairFinder
{
    public static Dictionary<int, int> FindPairs(this IReadOnlyList<Token> tokens,
        IReadOnlyList<IPairingRule> inlineRules)
    {
        var pairs = new Dictionary<int, int>();

        var openerTokenIndexes = new Stack<int>();


        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is not PairedToken token) continue;

            if (token.CanClose && TryGetPairToken(tokens, openerTokenIndexes, i, out var close))
            {
                var skipped = false;

                while (tokens[openerTokenIndexes.Peek()].Type != close.Type)
                {
                    openerTokenIndexes.Pop();
                    skipped = true;
                }

                if (skipped) continue;

                var openIndex = openerTokenIndexes.Peek();
                var open = (PairedToken)tokens[openIndex];

                var content = tokens.Skip(openIndex + 1).Take(i - openIndex + 1).ToArray();

                if (!close.GetRuleFor(inlineRules).CanPair(open, close, content))
                    continue;
                openerTokenIndexes.Pop();
                pairs[openIndex] = i;
            }

            if (token.CanOpen && CanOpenInside(tokens, openerTokenIndexes, inlineRules)) openerTokenIndexes.Push(i);
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