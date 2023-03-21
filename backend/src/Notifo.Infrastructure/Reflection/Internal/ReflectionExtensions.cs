// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;

namespace Notifo.Infrastructure.Reflection.Internal;

public static class ReflectionExtensions
{
    public static PropertyInfo[] GetPublicProperties(this Type type)
    {
        const BindingFlags bindingFlags =
            BindingFlags.FlattenHierarchy |
            BindingFlags.Public |
            BindingFlags.Instance;

        if (!type.IsInterface)
        {
            return type.GetProperties(bindingFlags);
        }

        var flattenProperties = new HashSet<PropertyInfo>();

        var considered = new List<Type>
        {
            type
        };

        var queue = new Queue<Type>();

        queue.Enqueue(type);

        while (queue.Count > 0)
        {
            var subType = queue.Dequeue();

            foreach (var subInterface in subType.GetInterfaces())
            {
                if (considered.Contains(subInterface))
                {
                    continue;
                }

                considered.Add(subInterface);

                queue.Enqueue(subInterface);
            }

            var typeProperties = subType.GetProperties(bindingFlags);

            foreach (var property in typeProperties)
            {
                flattenProperties.Add(property);
            }
        }

        return flattenProperties.ToArray();
    }

    public static bool Implements<T>(this Type type)
    {
        return type.GetInterfaces().Contains(typeof(T));
    }

    public static string GetManifestResourceString(this Assembly assembly, string resourceName)
    {
        var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new ArgumentException("Resource not found.", nameof(resourceName));
        }

        using (stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
