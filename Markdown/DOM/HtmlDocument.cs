using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Markdown.Nodes;

namespace Markdown.DOM;

public static class HtmlDocument
{
    public static IDocument CreateHtmlDocument() =>
        new HtmlParser().ParseDocument("""
                                       <!DOCTYPE html>
                                       <html>
                                       <head><meta charset="utf-8"><title>Converted from Markdown</title></head>
                                       <body></body>
                                       </html>
                                       """);

    public static INode ToDomNode(IDocument document, Nodes.Node node) => node switch
    {
        Text text => document.CreateTextNode(text.Value),
        TextBlock paragraph => CreateElement(document, "p", paragraph.Children),
        HeaderBlock header => CreateElement(document, "h1", header.Children),
        Strong strong => CreateElement(document, "strong", strong.Children),
        Emphasis emphasis => CreateElement(document, "em", [new Text(emphasis.Value)]),
        _ => throw new NotSupportedException($"Unable to identify node {node.GetType().Name}")
    };
    
    private static IElement CreateElement(
        IDocument document, string tag, IReadOnlyList<TextNode> children)
    {
        var element = document.CreateElement(tag);

        foreach (var child in children)
            element.AppendChild(ToDomNode(document, child));

        return element;
    }
}