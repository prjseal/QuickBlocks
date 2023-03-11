using System.Collections.Generic;
using HtmlAgilityPack;
using QuickBlocks.Models;

namespace QuickBlocks.Services
{
    public interface IBlockParsingService
    {
        List<BlockListModel> GetLists(HtmlNode node, bool isNestedList, string prefix = "[BlockList]");
        List<RowModel> GetRows(HtmlNode node, bool isNestedList);
        List<BlockItemModel> GetBlocks(HtmlNode node, string rowName);
        List<PropertyModel> GetProperties(HtmlNode node);
    }
}