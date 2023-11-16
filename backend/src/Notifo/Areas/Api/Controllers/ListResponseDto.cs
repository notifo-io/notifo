﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers;

public class ListResponseDto<T>
{
    /// <summary>
    /// The items.
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// The total number of items.
    /// </summary>
    public long Total { get; set; }
}
