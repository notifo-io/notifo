// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using HtmlAgilityPack;

namespace Notifo.Domain.Channels.Email
{
    internal static class Extensions
    {
        public static HtmlNode TryRemove(this HtmlNode parent, string selector)
        {
            var child = parent.SelectSingleNode(selector);

            if (child != null)
            {
                parent.RemoveChild(child);
            }

            return parent;
        }
    }
}
