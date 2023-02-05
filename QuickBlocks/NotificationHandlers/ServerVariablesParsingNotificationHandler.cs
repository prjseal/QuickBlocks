using Microsoft.AspNetCore.Routing;
using QuickBlocks.Controllers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace QuickBlocks.NotificationHandlers;

public class ServerVariablesParsingNotificationHandler : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;

    public ServerVariablesParsingNotificationHandler(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public void Handle(ServerVariablesParsingNotification notification)
    {
            
        notification.ServerVariables.Add("QuickBlocks", new
        {
            QuickBlocksApi = _linkGenerator.GetPathByAction(nameof(QuickBlocksApiController.Build), 
                ControllerExtensions.GetControllerName<QuickBlocksApiController>())
        });
    }
}