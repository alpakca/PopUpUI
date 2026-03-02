using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PopUpUI.Toolkit;

namespace PopUpUI.Elements.Spatial
{
    /// <summary>Represents an readonly-area that can represent any shape or position.</summary>
    [DebuggerDisplay("Bounds: {Bounds.ToString(),nq}")]
    public class SpatialArea : IEquatable<SpatialArea>, IEnumerable<Point>
    {
        public static readonly SpatialArea Empty = Rectangle.Empty;
        private static readonly Func<LineRange, (int, int)> defaultLineRangeConverter = range => (range.Start, range.End);

        private readonly bool WasDeclearedAsRectangle;
        // presents the positions that are in rows, rows are in ascending order
        private readonly ILookup<int, LineRange> RowRanges;

        /// <summary>The smallest X position of the area.</summary>
        public int X => Bounds.X;
        /// <summary>The smallest Y position of the area.</summary>
        public int Y => Bounds.Y;
        /// <summary>The biggest width of the area.</summary>
        public int Width => Bounds.Width;
        /// <summary>The biggest height of the area.</summary>
        public int Height => Bounds.Height;

        /// <summary>Gets the y-coordinate of the top edge of the <see cref="Bounds"/>.</summary>
        public int Top => Bounds.Top;
        /// <summary>Gets the y-coordinate that is the sum of the Y and Height property values of the <see cref="Bounds"/>.</summary>
        public int Bottom => Bounds.Bottom;
        /// <summary>Gets the x-coordinate of the left edge of the <see cref="Bounds"/>.</summary>
        public int Left => Bounds.Left;
        /// <summary>Gets the x-coordinate that is the sum of the X and Width property values of the <see cref="Bounds"/>.</summary>
        public int Right => Bounds.Right;

        /// <summary>The location of the <see cref="Bounds"/>.</summary>
        public Point Location => Bounds.Location;
        /// <summary>The size of the <see cref="Bounds"/>.</summary>
        public Point Size => Bounds.Size;
        /// <summary>Boundaries of the smallest rectangle containing all positions.</summary>
        public Rectangle Bounds { get; }

        /// <summary>Initializes a new area as a given rectangle.</summary>
        /// <param name="rectangle">The rectangle to use as the area.</param>
        public SpatialArea(Rectangle rectangle)
        {
            var rowRanges = new Dictionary<int, LineRange>();
            for (int r = rectangle.Top; r < rectangle.Bottom; r++)
            {
                rowRanges.Add(r, new LineRange { Start = rectangle.Left, End = rectangle.Right - 1 });
            }
            
            RowRanges = rowRanges.ToLookup(pair => pair.Key, pair => pair.Value);

            WasDeclearedAsRectangle = true;
            Bounds = rectangle;
        }

        /// <summary>Initializes a new area with given positions.</summary>
        /// <param name="positions">The positions will in the area.</param>
        /// <param name="fixedBounds">A fixed bounds for the area, that will appear in <see cref="Bounds"/> instead of calculated from <paramref name="positions"/>.</param>
        public SpatialArea(IEnumerable<Point> positions, Rectangle? fixedBounds)
        {
            // Row start ends
            var positionsByRow = positions.ToLookup(pos => pos.Y);
            var rowKeys = positionsByRow.Select(gr => gr.Key).Order();
            var rowRanges = new Dictionary<int, IEnumerable<LineRange>>();

            foreach (var key in rowKeys)
            {
                rowRanges.Add(key, positionsByRow[key].Select(pos => pos.X)
                                                      .Order()
                                                      .GetPossibleRanges()
                                                      .Select(tuple => new LineRange { Start = tuple.StartValue, End = tuple.EndValue }));
            }

            RowRanges = rowRanges.SelectMany(p => p.Value.Select(range => (p.Key, range))).ToLookup(pair => pair.Key, pair => pair.range);

            WasDeclearedAsRectangle = false;
            Bounds = fixedBounds ?? CreateBounds();
        }

        /// <summary>Initializes a new area with given positions.</summary>
        /// <param name="positions">The positions will in the area.</param>
        public SpatialArea(IEnumerable<Point> positions) : this(positions, null)
        {
        }

        /// <summary>Initializes a new area with a yielder and a fixed bounds for the area.</summary>
        /// <param name="positionYielder">A yielder to get positions of the area.</param>
        /// <param name="fixedBounds">
        /// A fixed bounds for the area, that will appear in <see cref="Bounds"/> instead of calculated from <paramref name="positionYielder"/>.
        /// </param>
        public SpatialArea(Func<IEnumerable<Point>> positionYielder, Rectangle? fixedBounds) : this(positionYielder(), fixedBounds)
        {
        }

        /// <summary>Initializes a new area with a yielder.</summary>
        /// <param name="positionYielder">A yielder to get positions of the area.</param>
        public SpatialArea(Func<IEnumerable<Point>> positionYielder) : this(positionYielder, null)
        {
        }
        
        /// <summary>Checks if given point is in the area.</summary>
        /// <param name="point">The point to check if in the area.</param>
        /// <returns>Returns <see langword="true"/> if point is in the area, else <see langword="false"/>.</returns>
        public bool Contains(Point point)
        {
            if (WasDeclearedAsRectangle) return Bounds.Contains(point);

            return RowRanges[point.Y].Any(range => range.Start <= point.X && point.X <= range.End);
        }

        /// <summary>Checks if given point is in the area.</summary>
        /// <param name="point">The point to check if in the area.</param>
        /// <param name="result"><see langword="true"/> if point is in the area, else <see langword="false"/>.</param>
        public void Contains(Point point, out bool result)
        {
            if (WasDeclearedAsRectangle)
            {
                result = Bounds.Contains(point);
                return;
            }

            result = RowRanges[point.Y].Any(range => range.Start <= point.X && point.X <= range.End);
        }

        /// <summary>Creates a new area with given offset.</summary>
        /// <param name="offset">Offset to apply this area.</param>
        /// <returns>Returns a new area which is created by offsetting this area's positions.</returns>
        public SpatialArea WithOffset(Point offset)
        {
            if (WasDeclearedAsRectangle) return new SpatialArea(Bounds.AddToLocation(offset));

            return FromLineRanges(RowRanges, lineIsRow: true, null, offset);
        }

        /// <summary>Creates a new area which is expanded to given size.</summary>
        /// <param name="newSize">The new size of the area.</param>
        /// <returns>Returns a new area which is created by expanding this area to <paramref name="newSize"/>.</returns>
        public SpatialArea ExpandTo(Point newSize)
        {
            if (WasDeclearedAsRectangle) return new SpatialArea(Bounds with { Width = newSize.X, Height = newSize.Y });

            // round it to up if decimal part is >= 5
            static int Round(float value)
            {
                return value % 1 >= 0.5f ? (int)value + 1 : (int)value;
            }

            SpatialArea instance = this;

            if (newSize.X != instance.Width)
            {
                float ratio = (float)newSize.X / instance.Width;

                instance = FromLineRanges(instance.RowRanges, lineIsRow: true, range => (Round(range.Start * ratio), Round((range.End + 1) * ratio) - 1));
            }

            if (newSize.Y != instance.Height)
            {
                float ratio = (float)newSize.Y / instance.Height;

                instance = FromLineRanges(instance.GetColumnRanges(), lineIsRow: false, range => (Round(range.Start * ratio), Round((range.End + 1) * ratio) - 1));
            }

            return instance;
        }

        private Rectangle CreateBounds()
        {
            int rectangleLeft = int.MaxValue;
            int rectangleRight = int.MinValue;

            // looking for Rectangle.Left and Rectangle.Right
            foreach (var key in RowRanges.Select(gr => gr.Key))
            {
                rectangleLeft = int.Min(rectangleLeft, RowRanges[key].First().Start);
                rectangleRight = int.Max(rectangleRight, RowRanges[key].Last().End);
            }

            int rectangleTop = RowRanges.First().Key;
            int rectangleBottom = RowRanges.Last().Key;

            return new Rectangle()
            {
                Location = new Point(rectangleLeft, rectangleTop),
                Size = new Point(rectangleRight - rectangleLeft + 1, rectangleBottom - rectangleTop + 1)
            };
        }

        private ILookup<int, LineRange> GetColumnRanges()
        {
            var rowKeys = RowRanges.Select(gr => gr.Key);
            var columnRanges = new Dictionary<int, IEnumerable<LineRange>>();

            // iterating through columns
            for (int i = Bounds.Left; i < Bounds.Right; i++)
            {
                List<int> rowsContaininPosition = new();
                foreach (var row in rowKeys)
                {
                    if (RowRanges[row].Any(range => range.Start <= i && i <= range.End)) rowsContaininPosition.Add(row);
                }

                columnRanges.Add(i, rowsContaininPosition.GetPossibleRanges().Select(tuple => new LineRange { Start = tuple.StartValue, End = tuple.EndValue }));
            }

            return columnRanges.SelectMany(p => p.Value.Select(range => (p.Key, range))).ToLookup(pair => pair.Key, pair => pair.range);
        }

        private static SpatialArea FromLineRanges(ILookup<int, LineRange> lineRanges, bool lineIsRow,
                                                  Func<LineRange, (int, int)> lineRangeConverter = null,
                                                  Point offset = default)
        {
            lineRangeConverter ??= defaultLineRangeConverter;
            
            IEnumerable<Point> gatherer()
            {
                foreach (var line in lineRanges.Select(gr => gr.Key))
                {
                    foreach (var range in lineRanges[line])
                    {
                        var (startColumn, endColumn) = lineRangeConverter(range);
                        for (int c = startColumn; c <= endColumn; c++)
                        {
                            yield return (lineIsRow ? new Point(c, line) : new Point(line, c)) + offset;
                        }
                    }
                }
            }

            return new SpatialArea(gatherer);
        }

        public static implicit operator SpatialArea(Rectangle rect)
        {
            return new SpatialArea(rect);
        }

        public static bool operator ==(SpatialArea left, SpatialArea right)
        {
            if (left is null || right is null) return false;

            if (left.WasDeclearedAsRectangle && right.WasDeclearedAsRectangle) return left.Bounds == right.Bounds;

            return left.RowRanges.SelectMany(gr => left.RowRanges[gr.Key].Select(range => (gr.Key, range)))
                                 .SequenceEqual(right.RowRanges.SelectMany(gr => right.RowRanges[gr.Key].Select(range => (gr.Key, range))));
        }

        public static bool operator !=(SpatialArea left, SpatialArea right)
        {
            return !(left == right);
        }

        public bool Equals(SpatialArea other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is SpatialArea area && Equals(area);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WasDeclearedAsRectangle, RowRanges);
        }
        
        /// <summary>Gets the positions of this area.</summary>
        /// <returns>Returns the positions of this area.</returns>
        public IEnumerator<Point> GetEnumerator()
        {
            foreach (var row in RowRanges.Select(gr => gr.Key))
            {
                foreach (var (Start, End) in RowRanges[row])
                {
                    for (int i = Start; i <= End; i++) yield return new Point(i, row);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        private readonly struct LineRange
        {
            public int Start { get; init; }
            public int End { get; init; }

            public void Deconstruct(out int start, out int end)
            {
                start = Start;
                end = End;
            }
        }
    }
}
