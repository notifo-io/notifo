// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Pipeline
{
    public sealed class NotEmptyRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!values.TryGetValue(routeKey, out var routeValue))
            {
                return false;
            }

            if (routeValue is string @string)
            {
                // Do not allow whitespaces only and leading and trailing whitespaces.
                return !string.IsNullOrWhiteSpace(@string) && @string.AsSpan().Trim().Length == @string.Length;
            }

            return routeValue != null;
        }
    }
}
