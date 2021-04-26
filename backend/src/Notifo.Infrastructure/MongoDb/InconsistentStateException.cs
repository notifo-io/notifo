// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Notifo.Infrastructure.MongoDb
{
    [Serializable]
    public class InconsistentStateException : Exception
    {
        public string CurrentEtag { get; }

        public string ExpectedEtag { get; }

        public InconsistentStateException(string currentEtag, string expectedEtag, Exception? inner = null)
            : base(FormatMessage(currentEtag, expectedEtag), inner)
        {
            CurrentEtag = currentEtag;

            ExpectedEtag = expectedEtag;
        }

        protected InconsistentStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            CurrentEtag = info.GetString(nameof(CurrentEtag))!;

            ExpectedEtag = info.GetString(nameof(ExpectedEtag))!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(CurrentEtag), CurrentEtag);
            info.AddValue(nameof(ExpectedEtag), ExpectedEtag);

            base.GetObjectData(info, context);
        }

        private static string FormatMessage(string currentEtag, string expectedEtag)
        {
            return $"Requested Etag {expectedEtag}, but found {currentEtag}.";
        }
    }
}
