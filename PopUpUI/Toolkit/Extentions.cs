using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace PopUpUI.Toolkit
{
    internal static class StructureExtensions
    {
        public static Point TimesFloat(this Point point, float fl)
        {
            return (point.ToVector2() * fl).ToPoint();
        }
    }

    internal static class EnumFlagExtensions
    {
        public static bool HasAnyFlagFrom(this Enum enumLeft, Enum enumRight)
        {
            dynamic enumLeftValue = Convert.ChangeType(enumLeft, enumLeft.GetTypeCode());
            dynamic enumRightValue = Convert.ChangeType(enumRight, enumRight.GetTypeCode());

            return (enumLeftValue & enumRightValue) != 0;
        }

        public static IEnumerable<T> GetUniqueFlags<T>(this T flags) where T : Enum
        {
            ulong flag = 1;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<T>())
            {
                ulong bits = Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                if (flag == bits && flags.HasFlag(value))
                {
                    yield return value;
                }
            }
        }
    }

    internal static class ThrowHelper
    {
        public static void ThrowIfNotBetween<T>(T value, T minValue, T maxValue) where T : INumber<T>
        {
            if (!(minValue <= value && value <= maxValue)) throw new ArgumentOutOfRangeException(value.ToString(), "Value does not fall between range.");
        }
    }
}
