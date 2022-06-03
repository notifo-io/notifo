// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Notifo.Infrastructure;

namespace Notifo.Domain
{
    public record struct TopicId
    {
#pragma warning disable MA0023 // Add RegexOptions.ExplicitCapture
        private static readonly Regex Regex = new Regex("^[^\\/\\n\\$]+(\\/[^\\/\n\\$]+)*$", RegexOptions.Compiled);
#pragma warning restore MA0023 // Add RegexOptions.ExplicitCapture

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
            return !string.IsNullOrWhiteSpace(id) && Regex.IsMatch(id);
        }

        public readonly string[] GetParts()
        {
            return Id.Split('/');
        }

        public readonly bool StartsWith(TopicId id)
        {
            return Id.StartsWith(id, StringComparison.OrdinalIgnoreCase);
        }

        public readonly bool StartsWith(string id)
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

        public override readonly string ToString()
        {
            return Id;
        }
    }
}
