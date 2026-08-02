// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Notifo.Pipeline;

public sealed class LocalhostOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!IsLocal(context.HttpContext.Connection))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

        base.OnActionExecuting(context);
    }

    private static bool IsLocal(ConnectionInfo connection)
    {
        var remoteIp = connection.RemoteIpAddress;
        // If there is no remote address the request has not been made over the network (e.g. in-memory test host).
        if (remoteIp == null)
        {
            return true;
        }

        // Any loopback address (127.0.0.1, ::1) is considered local.
        if (IPAddress.IsLoopback(remoteIp))
        {
            return true;
        }

        // The request is local when the remote address matches the address the server received it on.
        var localIp = connection.LocalIpAddress;
        if (localIp != null && remoteIp.Equals(localIp))
        {
            return true;
        }

        return false;
    }
}
