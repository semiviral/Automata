#region

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#endregion

namespace Automata.Engine.Extensions
{
    public static class EnumExtensions
    {
        public static T Next<T>(this T src) where T : Enum
        {
            T[] arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf(arr, src) + 1;
            return arr.Length == j ? arr[0] : arr[j];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TEnum> GetEnumsList<TEnum>() where TEnum : Enum => (TEnum[])Enum.GetValues(typeof(TEnum));
    }
}
