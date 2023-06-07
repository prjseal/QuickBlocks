using Microsoft.Extensions.DependencyInjection;
using Umbraco.Community.QuickBlocks.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.QuickBlocks.Services.Resolvers;

namespace Umbraco.Community.QuickBlocks.Composing;

public class RegisterServicesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IBlockCreationService, BlockCreationService>();
        builder.Services.AddTransient<IBlockParsingService, BlockParsingService>();
        builder.Services.AddTransient<IDataTypeNameResolver, DataTypeNameResolver>();
    }
}