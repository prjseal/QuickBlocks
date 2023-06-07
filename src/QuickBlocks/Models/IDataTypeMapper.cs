namespace Umbraco.Community.QuickBlocks.Models;

public interface IDataTypeMapper
{
    IEnumerable<string> HtmlElements { get; }
    string DataTypeName { get; }
}