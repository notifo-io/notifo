// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.MessageBird
{
    public static class PhoneNumberHelper
    {
        private static readonly char[] TrimChars = { ' ', '+', '0' };

        public static string Trim(string input)
        {
            return input.Trim(TrimChars);
        }
    }
}
