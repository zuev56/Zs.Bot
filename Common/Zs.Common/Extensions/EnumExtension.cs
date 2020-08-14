using System;
using System.Collections.Generic;
using System.Linq;

namespace Zs.Common.Extensions
{
    public static class EnumExtentions
    {
        /// <summary> Получение константы из строкового значения </summary>
        public static TEnum SafeParse<TEnum>(this string stringValue)
            where TEnum : struct, IConvertible
        {
            if (Enum.TryParse(stringValue, true, out TEnum result))
                return result;
            else
            {
                var allEnumValues = Enum.GetValues(typeof(TEnum));
                foreach (var value in allEnumValues)
                    if ((int)value == -1)
                        return (TEnum)Enum.Parse(typeof(TEnum), value.ToString(), ignoreCase: true);

                throw new InvalidOperationException($"Тип перечисления {nameof(TEnum)} не содержит стандартного значения Undefined = -1");
            }
        }

        /// <summary> Получение массива отдельных флагов из сборного флага </summary>
        public static List<TEnum> ToSingleFlagList<TEnum>(this Enum flag)
            where TEnum : struct, IConvertible
        {
            return Enum.GetValues(typeof(TEnum))
                       .Cast<Enum>()
                       .Where(flag.HasFlag)
                       .Cast<TEnum>()
                       .Where(f => f.ToString() != flag.ToString()
                                && ToSingleFlagList<TEnum>((Enum)(object)f).Count == 0) //((Enum)(object)f & flag) != 0)
                       .ToList();
        }
    }
}
