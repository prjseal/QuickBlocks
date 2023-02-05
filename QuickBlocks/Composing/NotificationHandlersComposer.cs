using Microsoft.Extensions.DependencyInjection;
using QuickBlocks.NotificationHandlers;
using QuickBlocks.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace QuickBlocks.Composing
{
    public class NotificationHandlersComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder
                .AddNotificationHandler<ServerVariablesParsingNotification,
                    ServerVariablesParsingNotificationHandler>();
        }
    }
}