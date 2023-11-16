// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.MongoDb;

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

    private static string FormatMessage(string currentEtag, string expectedEtag)
    {
        return $"Requested Etag {expectedEtag}, but found {currentEtag}.";
    }
}
