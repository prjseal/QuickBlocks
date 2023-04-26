using System.Collections.Generic;
using HtmlAgilityPack;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace QuickBlocks.Models
{
    public class BlockItemModel
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string ConventionName { get; }
        public IEnumerable<PropertyModel> Properties { get; set; }
        public string Html { get; set; }
        public string IconClass { get; set; }

        public BlockItemModel(IShortStringHelper shortStringHelper, string name, HtmlNode node, 
            string suffix = " Item", string iconClass = "icon-science")
        {
            Name = name;
            var conventionName = name.TrimEnd(suffix) + suffix;
            ConventionName = conventionName;
            Alias = conventionName.ToSafeAlias(shortStringHelper, true);
            Html = node.OuterHtml;
            IconClass = iconClass;
        }
    }
}