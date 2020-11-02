#region

using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IEnumerable<TEnum> GetValues<TEnum>() where TEnum : Enum => Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum @enum) where TEnum : Enum
        {
            foreach (Enum? value in Enum.GetValues(typeof(TEnum)))
            {
                if (value is not null && @enum.HasFlag(value)) yield return (TEnum)value;
            }
        }
    }
}
