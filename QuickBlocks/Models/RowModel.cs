using HtmlAgilityPack;
using System.Collections.Generic;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace QuickBlocks.Models
{
    public class RowModel
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string ConventionName { get; }
        public IEnumerable<BlockItemModel> Blocks { get; set; }
        public IEnumerable<PropertyModel> Properties { get; set; }
        public string Html { get; set; }

        public RowModel(IShortStringHelper shortStringHelper, string name, HtmlNode node, string suffix = "Row")
        {
            Name = name;
            var conventionName = name + suffix;
            ConventionName = conventionName;
            Alias = conventionName.ToSafeAlias(shortStringHelper, true);
            Html = node.OuterHtml;
        }
    }
}