namespace Umbraco.Community.QuickBlocks.Models.DataTypeMappers;


public class ImgDataTypeMapper : IDataTypeMapper
{
    public IEnumerable<string> HtmlElements => new[] { "img" };
    public string DataTypeName => "Image Media Picker";
}

public class HeadersDataTypeMapper : IDataTypeMapper
{
    public IEnumerable<string> HtmlElements => new[] { "h1", "h2", "h3", "h4", "h5", "h6" };
    public string DataTypeName => "Textstring";
}

public class ParagraphDataTypeMapper : IDataTypeMapper
{
    public IEnumerable<string> HtmlElements => new[] { "p" };
    public string DataTypeName => "Richtext editor";
}

public class AnchorDataTypeMapper : IDataTypeMapper
{
    public IEnumerable<string> HtmlElements => new[] { "a" };
    public string DataTypeName => "Single Url Picker";
}

