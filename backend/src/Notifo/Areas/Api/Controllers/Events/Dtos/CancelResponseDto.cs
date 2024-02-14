// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Events.Dtos;

public class CancelResponseDto
{
    /// <summary>
    /// True if something has been cancelled.
    /// </summary>
    public bool HasCancelled { get; set; }
}
