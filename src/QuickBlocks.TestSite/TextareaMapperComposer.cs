using Umbraco.Cms.Core.Composing;
using Umbraco.Community.QuickBlocks;

namespace QuickBlocks.TestSite
{
    // Use to test the ability to add a new data type mapper to the datatype mappers collection
    [ComposeAfter(typeof(QuickBlocksComposer))]
    internal class TextAreaMapperComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Example: how to add a new data type mapper to the datatype mappers collection
            //builder.QuickBlockDataTypeMappers()
            //            .InsertAfter<HeadersDataTypeMapper, TextareaDataTypeMapper>();

        }
    }
}
