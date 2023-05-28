using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.QuickBlocks.Models;

namespace Umbraco.Community.QuickBlocks;



public class DataTypeMappersCollection : BuilderCollectionBase<IDataTypeMapper>
{
    public DataTypeMappersCollection(Func<IEnumerable<IDataTypeMapper>> items) : base(items)
    {
    }
}

public class DataTypeMappersCollectionBuilder : OrderedCollectionBuilderBase<DataTypeMappersCollectionBuilder, DataTypeMappersCollection, IDataTypeMapper>
{
    protected override DataTypeMappersCollectionBuilder This => this;
}

public static class WebCompositionExtensions
{
    public static DataTypeMappersCollectionBuilder QuickBlockDataTypeMappers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<DataTypeMappersCollectionBuilder>();


}
