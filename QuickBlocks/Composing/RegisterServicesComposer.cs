using Microsoft.Extensions.DependencyInjection;
using QuickBlocks.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace QuickBlocks.Composing
{
    public class RegisterServicesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IBlockCreationService, BlockCreationService>();
            builder.Services.AddTransient<IBlockParsingService, BlockParsingService>();
        }
    }
}