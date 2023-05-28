using Umbraco.Community.QuickBlocks.Models;

namespace QuickBlocks.TestSite
{
    public class TextareaDataTypeMapper : IDataTypeMapper
    {
        public IEnumerable<string> HtmlElements => new[] {"h1"} ;

        public string DataTypeName => "textarea";
    }
}
