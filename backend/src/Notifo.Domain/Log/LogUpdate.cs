// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Domain.Log;

public sealed record LogWrite(string AppId, string? UserId, int EventCode, string Message, string System);
