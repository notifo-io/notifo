// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Notifo.Infrastructure;

namespace Notifo.Domain;

public readonly partial record struct TopicId
{
    private static readonly Regex FormatRegex = FormatFactory();

    [GeneratedRegex("^[^\\/\\n\\$]+(\\/[^\\/\n\\$]+)*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex FormatFactory();

    public readonly string Id;

    public TopicId(string id)
    {
        if (!IsValid(id))
        {
            ThrowHelper.ArgumentException("Invalid id", nameof(id));
        }

        Id = id;
    }

    public static bool IsValid(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && FormatRegex.IsMatch(id);
    }

    public string[] GetParts()
    {
        return Id.Split('/');
    }

    public bool StartsWith(TopicId id)
    {
        return Id.StartsWith(id, StringComparison.OrdinalIgnoreCase);
    }

    public bool StartsWith(string id)
    {
        return Id.StartsWith(id, StringComparison.OrdinalIgnoreCase);
    }

    public static implicit operator string(TopicId topicId)
    {
        return topicId.Id;
    }

    public static implicit operator TopicId(string value)
    {
        return new TopicId(value);
    }

    public override string ToString()
    {
        return Id;
    }
}
