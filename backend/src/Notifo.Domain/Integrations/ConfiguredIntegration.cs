// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations
{
    public sealed record ConfiguredIntegration(string Type, IntegrationProperties Properties)
    {
        public bool Enabled { get; set; }

        public int Priority { get; set; }

        public IntegrationStatus Status { get; set; }
    }
}