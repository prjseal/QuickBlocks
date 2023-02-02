using System.Collections.Generic;
using HtmlAgilityPack;
using QuickBlocks.Models;

namespace QuickBlocks.Services
{
    public interface IBlockParsingService
    {
        List<BlockListModel> GetLists(HtmlNode node);
        List<RowModel> GetRows(HtmlNode node);
        List<BlockItemModel> GetBlocks(HtmlNode node, string rowName);
        List<PropertyModel> GetProperties(HtmlNode node);
        bool CreateRowPartial(RowModel row);
        void CreateList(BlockListModel row);
    }
}