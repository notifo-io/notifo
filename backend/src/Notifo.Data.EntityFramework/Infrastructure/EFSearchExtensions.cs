// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Notifo.Infrastructure;

public static class EFSearchExtensions
{
    private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

    public static IQueryable<T> WhereContainsIgnoreCase<T>(this IQueryable<T> source, string? search, params Expression<Func<T, string?>>[] properties)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return source;
        }

        var parameter = Expression.Parameter(typeof(T), "x");

        // Use a member access, so that the search term is sent as parameter and not as constant.
        var value = Expression.Field(Expression.Constant(new StrongBox<string>(search.ToLowerInvariant())), nameof(StrongBox<string>.Value));

        // Not all databases support case insensitive comparisons, therefore we compare the lower case values.
        var body =
            properties
                .Select(x => (Expression)Expression.Call(Expression.Call(new ParameterReplacer(x.Parameters[0], parameter).Visit(x.Body), ToLowerMethod), ContainsMethod, value))
                .Aggregate(Expression.OrElse);

        return source.Where(Expression.Lambda<Func<T, bool>>(body, parameter));
    }

    private sealed class ParameterReplacer(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}
