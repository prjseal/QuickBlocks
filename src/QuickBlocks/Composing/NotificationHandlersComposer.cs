using Microsoft.Extensions.DependencyInjection;
using Umbraco.Community.QuickBlocks.NotificationHandlers;
using Umbraco.Community.QuickBlocks.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Community.QuickBlocks.Composing;
public class NotificationHandlersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder
            .AddNotificationHandler<ServerVariablesParsingNotification,
                ServerVariablesParsingNotificationHandler>();
    }
}