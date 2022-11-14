// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Topics;

public abstract class TopicCommand : AppCommandBase<Topic>
{
    public TopicId Path { get; set; }
}
