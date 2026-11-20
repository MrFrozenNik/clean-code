using FluentAssertions;
using NUnit.Framework;
using System.Collections;

namespace Markdown.Tests;

[TestFixture]
public class MarkdownTests
{
    
    
    
    public static IEnumerable HeadersAndParagraphsCases
    {
        get
        {
            yield return new TestCaseData<string, string>(
                    "#Заголовок", "<p>#Заголовок</p>")
                .SetName("Heading: no whitespaces gives no heading");
            yield return new TestCaseData<string, string>(
                    "# Заголовок", "<h1>Заголовок</h1>")
                .SetName("Heading: happy path");
            yield return new TestCaseData<string, string>(
                    "Первый абзац\nВторой абзац", "<p>Первый абзац</p><p>Второй абзац</p>")
                .SetName("Paragraphs: two lines give two paragraphs");
            yield return new TestCaseData<string, string>(
                    "# Заголовок\nПараграф", "<h1>Заголовок</h1><p>Параграф</p>")
                .SetName("Mixed: paragraph and heading");
        }
    }
    
    public static IEnumerable EmphasisAndStrongCases
    {
        get
        {
            yield return new TestCaseData<string, string>(
                    "_слово_", "<p><em>слово</em></p>")
                .SetName("Emphasis: wraps a word");

            yield return new TestCaseData<string, string>(
                    "__два слова__", "<p><strong>два слова</strong></p>")
                .SetName("Strong: wraps two words");

            yield return new TestCaseData<string, string>(
                    "сер_еди_на", "<p>сер<em>еди</em>на</p>")
                .SetName("Emphasis: inside a word");

            yield return new TestCaseData<string, string>(
                    "# Заголовок с __выделением__",
                    "<h1>Заголовок с <strong>выделением</strong></h1>")
                .SetName("Strong: inside a heading");
        }
    }
    public static IEnumerable NestedEmphasisCases
    {
        get
        {
            yield return new TestCaseData<string, string>(
                    "__до _слово_ после__",
                    "<p><strong>до <em>слово</em> после</strong></p>")
                .SetName("Nesting: emphasis inside strong");

            yield return new TestCaseData<string, string>(
                    "_до __слово__ после_",
                    "<p><em>до __слово__ после</em></p>")
                .SetName("Nesting: double underscores inside emphasis stay text");

            yield return new TestCaseData<string, string>(
                    "__до _слово__ после_",
                    "<p>__до _слово__ после_</p>")
                .SetName("Nesting: overlapping delimiters stay text");

            yield return new TestCaseData<string, string>(
                    "___слово___",
                    "<p>___слово___</p>")
                .SetName("Nesting: triple underscores stay text");
        }
    }
    public static IEnumerable EscapingCases
    {
        get
        {
            yield return new TestCaseData<string, string>(
                    @"\_слово\_", "<p>_слово_</p>")
                .SetName("Escaping: escaped underscores stay text");

            yield return new TestCaseData<string, string>(
                    @"\\_слово_", @"<p>\<em>слово</em></p>")
                .SetName("Escaping: escaped backslash before emphasis");

            yield return new TestCaseData<string, string>(
                    @"сим\вол", @"<p>сим\вол</p>")
                .SetName("Escaping: backslash before a letter stays text");

            yield return new TestCaseData<string, string>(
                    @"слово\", @"<p>слово\</p>")
                .SetName("Escaping: trailing backslash stays text");
        }
    }
    public static IEnumerable UnpairedDelimiterCases
    {
        get
        {
            yield return new TestCaseData<string, string>(
                    "_слово", "<p>_слово</p>")
                .SetName("Delimiters: unpaired opening underscore stays text");

            yield return new TestCaseData<string, string>(
                    "слово__", "<p>слово__</p>")
                .SetName("Delimiters: unpaired closing double underscore stays text");

            yield return new TestCaseData<string, string>(
                    "__слово_", "<p>__слово_</p>")
                .SetName("Delimiters: single and double underscores do not pair");

            yield return new TestCaseData<string, string>(
                    "до ____ после", "<p>до ____ после</p>")
                .SetName("Delimiters: empty strong pair stays text");

            yield return new TestCaseData<string, string>(
                    "цена_12_3", "<p>цена_12_3</p>")
                .SetName("Delimiters: underscores next to digits stay text");

            yield return new TestCaseData<string, string>(
                    "_ слово_", "<p>_ слово_</p>")
                .SetName("Delimiters: space after opening underscore");

            yield return new TestCaseData<string, string>(
                    "_слово _", "<p>_слово _</p>")
                .SetName("Delimiters: space before closing underscore");
        }
    }
    
    public static IEnumerable UnorderedListCases
    {
        get
        {
            yield return new TestCaseData<string, string>(
                    "- первый\n- второй\n- третий",
                    "<ul><li>первый</li><li>второй</li><li>третий</li></ul>")
                .SetName("List: consecutive items form one list");

            yield return new TestCaseData<string, string>(
                    "- первый\n\n- второй",
                    "<ul><li>первый</li></ul><p></p><ul><li>второй</li></ul>")
                .SetName("List: empty paragraph separates two lists");

            yield return new TestCaseData<string, string>(
                    "- пункт\n# Заголовок\nАбзац",
                    "<ul><li>пункт</li></ul><h1>Заголовок</h1><p>Абзац</p>")
                .SetName("List: followed by a heading and a paragraph");

            yield return new TestCaseData<string, string>(
                    "- __жирный _курсив_ текст__\n- # просто текст",
                    "<ul><li><strong>жирный <em>курсив</em> текст</strong></li>" +
                    "<li># просто текст</li></ul>")
                .SetName("List: nested emphasis works and hash stays literal");

            yield return new TestCaseData<string, string>(
                    "- пункт\n\\- обычный текст",
                    "<ul><li>пункт</li></ul><p>- обычный текст</p>")
                .SetName("List: escaped marker starts a plain paragraph");
        }
    }
    
    [TestCaseSource(nameof(UnorderedListCases))]
    [TestCaseSource(nameof(UnpairedDelimiterCases))]
    [TestCaseSource(nameof(EscapingCases))]
    [TestCaseSource(nameof(NestedEmphasisCases))]
    [TestCaseSource(nameof(EmphasisAndStrongCases))]
    [TestCaseSource(nameof(HeadersAndParagraphsCases))]
    public void Convert_ReturnsExpectedHtml(string markdown, string expected)
    {
        var result = Md.Convert(markdown);

        result.Should().Contain(expected);
    }
    
}