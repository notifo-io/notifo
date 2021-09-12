// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notifo.Infrastructure.Reflection
{
    public sealed class EnumMap<T1, T2> where T1 : notnull where T2 : notnull
    {
        private readonly Dictionary<T1, T2> dictionary1 = new Dictionary<T1, T2>();
        private readonly Dictionary<T2, T1> dictionary2 = new Dictionary<T2, T1>();

        public void Add(T1 v1, T2 v2)
        {
            dictionary1[v1] = v2;
            dictionary2[v2] = v1;
        }

        public T1 this[T2 v2]
        {
            get => dictionary2[v2];
        }

        public T2 this[T1 v1]
        {
            get => dictionary1[v1];
        }

        public void CheckMappings()
        {
        }

        public static EnumMap<T1, T2> GenerateByName()
        {
            var result = new EnumMap<T1, T2>();

            var t1Values = new HashSet<T1>(Enum.GetValues(typeof(T1)).OfType<T1>());
            var t2Values = new HashSet<T2>(Enum.GetValues(typeof(T2)).OfType<T2>());

            foreach (var t1Value in t1Values.ToList())
            {
                var t2Value = (T2)Enum.Parse(typeof(T2), t1Value.ToString()!.Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase), true);

                result.Add(t1Value, t2Value);

                t1Values.Remove(t1Value);
                t2Values.Remove(t2Value);
            }

            if (t1Values.Count > 0 || t2Values.Count > 0)
            {
                var exceptionBuilder = new StringBuilder("The following values are not mapped: ");

                foreach (var t1Value in t1Values)
                {
                    exceptionBuilder.AppendLine($"{typeof(T1)} {t1Value}");
                }

                foreach (var t2Value in t2Values)
                {
                    exceptionBuilder.AppendLine($"{typeof(T2)} {t2Value}");
                }

                throw new InvalidOperationException(exceptionBuilder.ToString());
            }

            return result;
        }
    }
}
