﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Notifo.Infrastructure
{
    public static class Telemetry
    {
        public static readonly ActivitySource Activities = new ActivitySource("Notifo");

        public static Activity? StartMethod(this ActivitySource activity, Type type, [CallerMemberName] string? memberName = null)
        {
            return activity.StartActivity($"{type.Name}/{memberName}", ActivityKind.Internal);
        }

        public static Activity? StartMethod<T>(this ActivitySource activity, [CallerMemberName] string? memberName = null)
        {
            return activity.StartActivity($"{typeof(T).Name}/{memberName}", ActivityKind.Internal);
        }

        public static Activity? StartMethod(this ActivitySource activity, string objectName, [CallerMemberName] string? memberName = null)
        {
            return activity.StartActivity($"{objectName}/{memberName}", ActivityKind.Internal);
        }
    }
}
