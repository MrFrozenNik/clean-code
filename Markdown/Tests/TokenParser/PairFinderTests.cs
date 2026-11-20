using FluentAssertions;
using Markdown.Nodes.Rules;
using Markdown.Nodes.Transformers;
using Markdown.Tokens;
using NUnit.Framework;

namespace Markdown.Tests.TokenParser;


[TestFixture]
public class PairFinderTests
{
    private static TextToken T(string value) => new(value);
    private static UnderscoreToken EmOpen() => new(CanOpen: true, CanClose: false, IsInsideWord: false);
    private static UnderscoreToken EmClose() => new(CanOpen: false, CanClose: true, IsInsideWord: false);

    private static DoubleUnderscoreToken StrongOpen() => new(CanOpen: true, CanClose: false, IsInsideWord: false);
    private static DoubleUnderscoreToken StrongClose() => new(CanOpen: false, CanClose: true, IsInsideWord: false);

    private static TestCaseData Case(string name, Token[] tokens, Dictionary<int, PairEnd> expected)
        => new TestCaseData(tokens, expected).SetName(name);


    private static IEnumerable<TestCaseData> PairCases()
    {
        yield return Case("SimpleEm",
            [EmOpen(), T("a"), EmClose()],
            new Dictionary<int, PairEnd> { [0] = new(2, EmphasisRule) });

        yield return Case("EmInsideStrong",
            [StrongOpen(), T("a "), EmOpen(), T("b"), EmClose(), T(" c"), StrongClose()],
            new Dictionary<int, PairEnd> { [2] = new(4, EmphasisRule), [0] = new(6, StrongPairingRule) });

        yield return Case("StrongInsideEm_IsNotPaired",
            [EmOpen(), T("a "), StrongOpen(), T("b"), StrongClose(), T(" c"), EmClose()],
            new Dictionary<int, PairEnd> { [0] = new(6, EmphasisRule) });

        yield return Case("Intersection_NoPairs",
            [StrongOpen(), T("a "), EmOpen(), T("b"), StrongClose(), T(" c"), EmClose()],
            new Dictionary<int, PairEnd>());

        yield return Case("Unpaired_NoPairs",
            [StrongOpen(), T("Непарные"), EmClose(), T(" символы")],
            new Dictionary<int, PairEnd>());

        yield return Case("EmptyContentWithValidPair_GivesPair",
            [StrongOpen(), StrongClose()],
            new Dictionary<int, PairEnd> { [0] = new(1, StrongPairingRule) });

        yield return Case("TripleUnderscore_StrongThenEm",
            [StrongOpen(), EmOpen(), T("a"), EmClose(), T(" bb"), StrongClose()],
            new Dictionary<int, PairEnd> { [1] = new(3, EmphasisRule), [0] = new(5, StrongPairingRule) });
    }
    
    private static readonly IPairingRule StrongPairingRule = new StrongRule();
    private static readonly IPairingRule EmphasisRule = new EmRule();

    private static readonly IReadOnlyList<IPairingRule> InlineRules =
    [
        StrongPairingRule,
        EmphasisRule,
    ];
    
    
    [TestCaseSource(nameof(PairCases))]
    public void Find_ReturnsExpectedPairs(IReadOnlyList<Token> tokens, Dictionary<int, PairEnd> expected)
    {
        var actual = tokens.FindPairs(InlineRules);

        actual.Should().BeEqualTo(expected);
    }
}
