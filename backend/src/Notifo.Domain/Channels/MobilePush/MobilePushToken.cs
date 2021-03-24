// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushToken : IEquatable<MobilePushToken>
    {
        public string Token { get; set; }

        public MobileDeviceType DeviceType { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as MobilePushToken);
        }

        public bool Equals(MobilePushToken? other)
        {
            return string.Equals(other?.Token, Token);
        }

        public override int GetHashCode()
        {
            return Token?.GetHashCode() ?? 0;
        }
    }
}
