// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;

namespace Notifo.Domain.Settings;

public static class VerifySettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DontScrubSolutionDirectory();
        VerifierSettings.DontScrubProjectDirectory();

        // Firebase marks the message token as obsolete, but it is still what we send.
        VerifierSettings.IncludeObsoletes();
    }
}
