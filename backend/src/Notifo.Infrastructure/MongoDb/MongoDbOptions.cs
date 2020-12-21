// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.MongoDb
{
    public sealed class MongoDbOptions
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }
}
