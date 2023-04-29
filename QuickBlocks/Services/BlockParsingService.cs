using HtmlAgilityPack;
using QuickBlocks.Models;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Strings;

namespace QuickBlocks.Services
{
    public class BlockParsingService : IBlockParsingService
    {
        private readonly IShortStringHelper _shortStringHelper;

        public BlockParsingService(IShortStringHelper shortStringHelper)
        {
            _shortStringHelper = shortStringHelper;
        }

        public List<BlockListModel> GetLists(HtmlNode node, bool isNestedList, string prefix = "[BlockList]")
        {
            var xpath = isNestedList ? "//*[@data-sub-list-name]" : "//*[@data-list-name]";
            var name = isNestedList ? "data-sub-list-name" : "data-list-name";

            var lists = new List<BlockListModel>();

            var listNodes = node.SelectNodes(xpath);

            if (listNodes == null || !listNodes.Any()) return lists;

            foreach (var listNode in listNodes)
            {
                var listName = listNode.GetAttributeValue(name, "");
                var maxPropertyWidth = listNode.GetAttributeValue("data-list-maxwidth", "");
                var useSingleBlockMode = listNode.GetAttributeValue("data-list-single", "false");
                var useLiveEditing = listNode.GetAttributeValue("data-list-live", "false");
                var useInlineEditingAsDefault = listNode.GetAttributeValue("data-list-inline", "false");
                var validationLimitMin = listNode.GetAttributeValue("data-list-min", "0");
                var validationLimitMax = listNode.GetAttributeValue("data-list-max", "0");

                var list = new BlockListModel(prefix + " " + listName);
                if (!string.IsNullOrWhiteSpace(maxPropertyWidth))
                {
                    list.MaxPropertyWidth = maxPropertyWidth;
                }

                if (bool.TryParse(useLiveEditing, out var live))
                {
                    list.UseLiveEditing = live;
                }

                if (bool.TryParse(useInlineEditingAsDefault, out var inline))
                {
                    list.UseInlineEditingAsDefault = inline;
                }

                if (bool.TryParse(useSingleBlockMode, out var single))
                {
                    list.UseSingleBlockMode = single;
                }

                if (single)
                {
                    list.ValidationLimitMin = 1;
                    list.ValidationLimitMax = 1;
                }
                else
                {
                    if (!int.TryParse(validationLimitMin, out var min))
                    {
                        min = 0;
                    }

                    if (!int.TryParse(validationLimitMax, out var max))
                    {
                        max = 0;
                    }

                    list.ValidationLimitMin = min;
                    list.ValidationLimitMax = max;
                }



                var rows = this.GetRows(listNode, isNestedList);
                list.Rows = rows;

                lists.Add(list);
            }

            return lists;
        }

        public List<RowModel> GetRows(HtmlNode node, bool isNestedList)
        {
            var xpath = isNestedList ? "//*[@data-item-name]" : "//*[@data-row-name]";
            var name = isNestedList ? "data-item-name" : "data-row-name";

            var rows = new List<RowModel>();

            var rowNodes = node.SelectNodes(xpath);

            if (rowNodes == null || !rowNodes.Any()) return rows;

            foreach (var rowNode in rowNodes)
            {
                var rowName = rowNode.GetAttributeValue(name, "");
                var settingsName = rowNode.GetAttributeValue("data-settings-name", "");
                var hasSettingsValue = rowNode.GetAttributeValue("data-has-settings", "true");
                var iconClass = rowNode.GetAttributeValue("data-icon-class", "icon-science");
                var iconColour = rowNode.GetAttributeValue("data-icon-colour", "color-indigo");

                bool.TryParse(hasSettingsValue, out var hasSettings);

                var ignoreNamingConventionValue = rowNode.GetAttributeValue("data-ignore-convention", "false");

                bool.TryParse(ignoreNamingConventionValue, out var ignoreNamingConvention);

                var row = new RowModel(_shortStringHelper, rowName, rowNode, 
                    settingsName, hasSettings, ignoreNamingConvention, 
                    iconClass: string.Join(" ", (new List<string>() { iconClass, iconColour }).Where(x => !string.IsNullOrWhiteSpace(x))));

                var properties = GetProperties(rowNode, isNestedList ? "item" : "row");
                row.Properties = properties;

                rows.Add(row);
            }

            return rows;
        }

        public List<BlockItemModel> GetBlocks(HtmlNode node, string rowName)
        {
            var blocks = new List<BlockItemModel>();

            var descendants = node.Descendants();
            if (descendants == null || !descendants.Any()) return blocks;

            foreach (var descendant in descendants)
            {
                var itemName = descendant.GetAttributeValue("data-item-name", "");
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    var item = new BlockItemModel(_shortStringHelper, itemName, descendant);

                    var properties = GetProperties(descendant, "item");
                    item.Properties = properties;

                    blocks.Add(item);
                }
            }

            return blocks;
        }

        public List<PropertyModel> GetProperties(HtmlNode node, string context)
        {
            var properties = new List<PropertyModel>();

            var descendants = node.Descendants();
            if (descendants == null || !descendants.Any()) return properties;

            foreach (var descendant in descendants)
            {
                var itemLocation = descendant.GetAttributeValue("data-prop-location", "");
                var itemName = descendant.GetAttributeValue("data-prop-name", "");
                var itemType = descendant.GetAttributeValue("data-prop-type", "");

                if (context == "page" && itemLocation != "page") continue;

                if (context == "row" && itemLocation != "row") continue;

                if (!string.IsNullOrWhiteSpace(itemName) && string.IsNullOrWhiteSpace(itemType))
                {
                    switch (descendant.OriginalName.ToLower())
                    {
                        case "img":
                            itemType = "Image Media Picker";
                            break;
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                            itemType = "Textstring";
                            break;
                        case "p":
                            itemType = "Richtext editor";
                            break;
                        case "a":
                            itemType = "Single Url Picker";
                            break;
                        default:
                            itemType = "Textstring";
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    var item = new PropertyModel(itemName, itemType, descendant);
                    properties.Add(item);
                }
            }

            return properties;
        }

        public ContentTypeModel GetContentType(HtmlNode node)
        {
            var descendants = node.Descendants();
            if (descendants == null || !descendants.Any()) return null;

            foreach(var descendant in descendants)
            {
                var itemName = descendant.GetAttributeValue("data-content-type-name", "");
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    var item = new ContentTypeModel(_shortStringHelper, itemName);
                    return item;
                }
            }

            return null;
        }
    }
}