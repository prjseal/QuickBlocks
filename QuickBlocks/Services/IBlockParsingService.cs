using System.Collections.Generic;
using HtmlAgilityPack;
using QuickBlocks.Models;

namespace QuickBlocks.Services
{
    public interface IBlockParsingService
    {
        List<BlockListModel> GetLists(string html, bool isNestedList, string prefix = "[BlockList]");
        List<RowModel> GetRows(string html, bool isNestedList);
        List<BlockItemModel> GetBlocks(string html, string rowName);
        List<PropertyModel> GetProperties(string html, string location);
        ContentTypeModel GetContentType(HtmlNode node);
        List<PartialViewModel> GetPartialViews(HtmlNode node);
    }
}