// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Notifo.Infrastructure;

public static class TagsConverter
{
    private const string Separator = "&&";

    public static string ToTags(this IEnumerable<string> values)
    {
        var result = string.Join(Separator, values);
        if (result.Length == 0)
        {
            return string.Empty;
        }

        return $"{Separator}{result}{Separator}";
    }

    public static IQueryable<T> WhereContainsAnyTag<T>(this IQueryable<T> source, Expression<Func<T, string>> property, IEnumerable<string>? values)
    {
        var tags = values?.ToList();
        if (tags == null || tags.Count == 0)
        {
            return source;
        }

        // The tags are surrounded by separators, so that a tag does not match the substring of another tag.
        var body =
            tags
                .Select(x => (Expression)Expression.Call(property.Body, nameof(string.Contains), null, ToParameter($"{Separator}{x}{Separator}")))
                .Aggregate(Expression.OrElse);

        return source.Where(Expression.Lambda<Func<T, bool>>(body, property.Parameters));
    }

    private static Expression ToParameter(string value)
    {
        // Use a member access, so that the value is sent as parameter and not as constant.
        return Expression.Field(Expression.Constant(new StrongBox<string>(value)), nameof(StrongBox<string>.Value));
    }
}
