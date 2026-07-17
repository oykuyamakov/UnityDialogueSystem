using System;
using System.Collections.Generic;

namespace Cadi.Scripts.Utility.Extensions
{
    public static class EnumExtensions 
    {
        public static IEnumerable<T> GetUniqueFlags<T>(this T flags)
            where T : Enum    // New constraint for C# 7.3
        {
            foreach (Enum value in Enum.GetValues(flags.GetType()))
                if (flags.HasFlag(value))
                    yield return (T)value;
        }
    }
}
