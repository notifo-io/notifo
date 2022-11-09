// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Web.Dtos;

public class ConnectDto
{
    public ConnectionMode ConnectionMode { get; set; }

    public int PollingInterval { get; set; }
}
