// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Identity.MongoDb;

public sealed class MongoDbConfiguration<T>
{
    public string Id { get; set; }

    public T Value { get; set; }

    public DateTime Expires { get; set; }
}
