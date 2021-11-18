// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Notifo.Domain.UserNotifications.MongoDb
{
    public static class Base64Extensions
    {
        public static string ToBase64(this string value)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(value));
        }

        public static string FromOptionalBase64(this string value)
        {
            try
            {
                return Encoding.Default.GetString(Convert.FromBase64String(value));
            }
            catch (FormatException)
            {
                return value;
            }
        }
    }
}
