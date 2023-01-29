using System.Collections.Generic;
using HtmlAgilityPack;
using QuickBlocks.Models;

namespace QuickBlocks.Services
{
    public interface IBlockParsingService
    {
        List<RowModel> GetRows(HtmlNode node);
        List<BlockItemModel> GetBlocks(HtmlNode node, string rowName);
        List<PropertyModel> GetProperties(HtmlNode node);
        bool CreateRow(RowModel row);
        void CreateDataType(string name, IEnumerable<RowModel> rows);
    }
}