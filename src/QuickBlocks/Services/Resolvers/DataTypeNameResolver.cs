using Microsoft.Extensions.Options;

namespace Umbraco.Community.QuickBlocks.Services.Resolvers;
public class DataTypeNameResolver : IDataTypeNameResolver
{
    private readonly DataTypeMappersCollection _dataTypeMappers;
    private readonly IOptions<QuickBlocksDefaultOptions> _defaultOptions;

    public DataTypeNameResolver(DataTypeMappersCollection dataTypeMappers, IOptions<QuickBlocksDefaultOptions> defaultOptions)
    {
        _dataTypeMappers = dataTypeMappers;
        _defaultOptions = defaultOptions;
    }

    public string GetDataTypeName(string htmlElement)
    {

        var dt = _dataTypeMappers.LastOrDefault(dt=>dt.HtmlElements.Contains(htmlElement));
        
        return dt?.DataTypeName ?? _defaultOptions.Value.DefaultDataTypeName;
                
    }
}
