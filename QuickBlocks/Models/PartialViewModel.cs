using HtmlAgilityPack;

namespace QuickBlocks.Models;
public class PartialViewModel
{
    public string Name { get; set; }
    public string Html { get; set; }

    public PartialViewModel(string name, HtmlNode node)
    {
        Name = name;
        Html = node?.OuterHtml;
    }
}
