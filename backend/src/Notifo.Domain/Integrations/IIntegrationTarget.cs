// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations
{
    public interface IIntegrationTarget
    {
        public bool Test { get; }

        public IEnumerable<KeyValuePair<string, object>>? Properties { get; }
    }
}
