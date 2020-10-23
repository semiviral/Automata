#region

using System;
using System.Collections.Generic;

#endregion


namespace Automata.Engine.Extensions
{
    public static class EnumExtensions
    {
        public static T Next<T>(this T src) where T : Enum
        {
            T[] arr = (T[])Enum.GetValues(typeof(T));
            int index = Array.IndexOf(arr, src) + 1;
            return arr.Length == index ? arr[0] : arr[index];
        }

        public static IEnumerable<TEnum> GetEnumsList<TEnum>() where TEnum : Enum => (TEnum[])Enum.GetValues(typeof(TEnum));
    }
}
