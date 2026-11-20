using Markdown.DOM;
using Markdown.Nodes.Transformers;
using Markdown.Tokens.Transformers;
using AngleSharp;

namespace Markdown;

public static class Md
{
    public static string Convert(string markdown)
    {
        var tokens = StringTokenizer.Tokenize(markdown);

        var nodes = TokenParser.Parse(tokens);

        var document = HtmlDocument.CreateHtmlDocument();
        var body = document.Body ?? throw new InvalidOperationException();
        foreach (var node in nodes)
            body.AppendChild(HtmlDocument.ToDomNode(document, node));

        return document.ToHtml();
    }
}