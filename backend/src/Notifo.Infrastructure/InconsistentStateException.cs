// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.MongoDb;

[Serializable]
public class InconsistentStateException(string currentEtag, string expectedEtag, Exception? inner = null) : Exception(FormatMessage(currentEtag, expectedEtag), inner)
{
    public string CurrentEtag { get; } = currentEtag;

    public string ExpectedEtag { get; } = expectedEtag;

    private static string FormatMessage(string currentEtag, string expectedEtag)
    {
        return $"Requested Etag {expectedEtag}, but found {currentEtag}.";
    }
}
