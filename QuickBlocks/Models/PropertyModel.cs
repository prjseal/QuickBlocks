using HtmlAgilityPack;

namespace QuickBlocks.Models
{
    public class PropertyModel
    {
        public string Name { get; }
        public string PropertyType { get; set; }
        public string Html { get; set; }
        public string Value { get; set; }

        public PropertyModel(string name, string propertyType, HtmlNode node)
        {
            Name = name;
            PropertyType = propertyType;
            Html = node?.OuterHtml;
            Value = node?.InnerHtml;
        }
    }
}