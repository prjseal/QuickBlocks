using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.QuickBlocks.Models.DataTypeMappers;

namespace Umbraco.Community.QuickBlocks;

public class QuickBlocksComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.ManifestFilters().Append<QuickBlocksManifestFilter>();

        builder.QuickBlockDataTypeMappers()
                    .Append<ImgDataTypeMapper>()
                    .Append<HeadersDataTypeMapper>()
                    .Append<ParagraphDataTypeMapper>()
                    .Append<AnchorDataTypeMapper>();
    }
}

