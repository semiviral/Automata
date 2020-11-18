using System;
using System.Collections.Generic;
using System.Linq;

namespace Automata.Engine.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<TEnum> GetValues<TEnum>() where TEnum : Enum => Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum @enum) where TEnum : Enum
        {
            foreach (Enum? value in Enum.GetValues(typeof(TEnum)))
            {
                if (value is not null && @enum.HasFlag(value))
                {
                    yield return (TEnum)value;
                }
            }
        }
    }
}
