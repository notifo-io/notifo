// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain;

public sealed class NotificationFormatting<TText> where TText : class
{
    public TText Subject { get; set; }

    public TText? Body { get; set; }

    public TText? ConfirmText { get; set; }

    public TText? ImageSmall { get; set; }

    public TText? ImageLarge { get; set; }

    public TText? LinkUrl { get; set; }

    public TText? LinkText { get; set; }

    public ConfirmMode? ConfirmMode { get; set; }
}
