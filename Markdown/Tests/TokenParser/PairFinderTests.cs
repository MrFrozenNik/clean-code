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

    private static TestCaseData Case(string name, Token[] tokens, Dictionary<int, int> expected)
        => new TestCaseData(tokens, expected).SetName(name);


    private static IEnumerable<TestCaseData> PairCases()
    {
        yield return Case("SimpleEm",
            [EmOpen(), T("a"), EmClose()],
            new Dictionary<int, int> { [0] = 2 });

        yield return Case("EmInsideStrong",
            [StrongOpen(), T("a "), EmOpen(), T("b"), EmClose(), T(" c"), StrongClose()],
            new Dictionary<int, int> { [2] = 4, [0] = 6 });

        yield return Case("StrongInsideEm_IsNotPaired",
            [EmOpen(), T("a "), StrongOpen(), T("b"), StrongClose(), T(" c"), EmClose()],
            new Dictionary<int, int> { [0] = 6 });

        yield return Case("Intersection_NoPairs",
            [StrongOpen(), T("a "), EmOpen(), T("b"), StrongClose(), T(" c"), EmClose()],
            new Dictionary<int, int>());

        yield return Case("Unpaired_NoPairs",
            [StrongOpen(), T("Непарные"), EmClose(), T(" символы")],
            new Dictionary<int, int>());

        yield return Case("EmptyContent_NoPairs",
            [StrongOpen(), StrongClose()],
            new Dictionary<int, int>{[0] = 1});

    yield return Case("TripleUnderscore_StrongThenEm",
            [StrongOpen(), EmOpen(), T("a"), EmClose(), T(" bb"), StrongClose()],
            new Dictionary<int, int> { [1] = 3, [0] = 5 });
    }
    
    private static readonly IReadOnlyList<IPairingRule> InlineRules =
    [
        new StrongRule(),
        new EmRule(),
    ];
    
    
    [TestCaseSource(nameof(PairCases))]
    public void Find_ReturnsExpectedPairs(IReadOnlyList<Token> tokens, Dictionary<int, int> expected)
    {
        var actual = tokens.FindPairs(InlineRules);

        actual.Should().BeEqualTo(expected);
    }
}