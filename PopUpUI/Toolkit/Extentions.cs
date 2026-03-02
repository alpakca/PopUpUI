using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace PopUpUI.Toolkit
{
    internal static class DrawExtensions
    {
        public static Texture2D CropTexture(this Texture2D source, Rectangle rect)
        {
            Texture2D tex = DrawHelper.CreateTexture(rect.Width, rect.Height);
            int count = rect.Width * rect.Height;
            Color[] data = new Color[count];
            source.GetData(0, rect, data, 0, count);
            tex.SetData(data);
            return tex;
        }

        public static void CropTexture(this Texture2D source, RenderTarget2D writeOn, Rectangle rect)
        {
            DrawHelper.SetRenderTarget(writeOn);
            DrawHelper.BeginBatch(TextureDrawOptions.Default);

            DrawHelper.Batch.Draw(source, rect.Location.ToVector2(), rect, Color.White);

            DrawHelper.EndBatch();
        }
    }

    internal static class StructureExtensions
    {
        public static Rectangle AddToLocation(this Rectangle rect, Point location)
        {
            return new Rectangle(rect.Location + location, rect.Size);
        }

        public static IEnumerable<int> AllIndexesOf(this string str, string searchstring)
        {
            int minIndex = str.IndexOf(searchstring);
            while (minIndex != -1)
            {
                yield return minIndex;
                minIndex = str.IndexOf(searchstring, minIndex + 1);
            }
        }

        public static Point TimesFloat(this Point point, float fl)
        {
            return (point.ToVector2() * fl).ToPoint();
        }

        public static IEnumerable<(int StartValue, int EndValue)> GetPossibleRanges(this IEnumerable<int> source)
        {
            if (!source.Any()) yield break;

            int[] values = [.. source, source.Last() - 1];

            int startValue = values[0];
            int currentValue = values[0];

            for (int i = 1; i < values.Length; i++)
            {
                if (currentValue + 1 != values[i])
                {
                    yield return (startValue, values[i - 1]);
                    startValue = currentValue = values[i];
                }
                else
                {
                    currentValue += 1;
                }
            }
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
        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void ThrowIfNotBetween<T>(T value, T minValue, T maxValue) where T : INumber<T>
        {
            if (!(minValue <= value && value <= maxValue)) throw new ArgumentOutOfRangeException(value.ToString(), "Value does not fall between range.");
        }
    }
}
