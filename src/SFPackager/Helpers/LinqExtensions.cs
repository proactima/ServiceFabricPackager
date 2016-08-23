using System;
using System.Collections.Generic;

namespace SFPackager.Helpers
{
    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var element in input)
            {
                action(element);
            }
        }
    }
}