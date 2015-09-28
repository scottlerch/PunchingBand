using System;
using System.Collections.Generic;

namespace PunchingBand.Utilities
{
    public static class IListExtensions
    {
        public static void AddCamelCaseEnum<T>(this IList<string> list)
        {
            foreach (var value in (T[])Enum.GetValues(typeof(T)))
            {
                list.Add(value.ToString().SplitCamelCase());
            }
        }
    }
}
