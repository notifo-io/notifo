// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Email
{
    [Serializable]
    public sealed class EmailFormattingError
    {
        public string Message { get; }

        public int Line { get; }

        public EmailFormattingError(string message, int line = -1)
        {
            Message = message;

            Line = line;
        }
    }
}
