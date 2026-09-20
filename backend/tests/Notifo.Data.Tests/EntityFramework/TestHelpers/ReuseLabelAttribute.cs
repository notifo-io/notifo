// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.EntityFramework.TestHelpers;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ReuseLabelAttribute(string label) : Attribute
{
    public string Label { get; } = label;
}
