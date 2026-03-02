using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Elements
{
    /// <summary>Presents a circular shaped bar whose fullness can be changed dynamically.</summary>
    public class CircularLoadableBar : PopUpElement, IDisposable
    {
        private int currentAngleFullness = 0;
        private float opacity = 1;
        private Rectangle area;

        private readonly float Radius;
        private readonly Color[] ColorDataForTexture;
        private readonly RenderTarget2D currentBarTexture;
        private readonly AnimationState FilledBarTexture;
        private readonly TextureDisplayInfo uiElement;

        /// <summary>Direction of bar wher eit is going to be decreased to.</summary>
        public CircularBarDecreaseDirection DecreaseDirection { get; }
        public override float Opacity
        {
            get => opacity;
            set
            {
                uiElement.Opacity = value;
                opacity = value;
            }
        }
        public override Rectangle Area
        {
            get => area;
            set
            {
                uiElement.Location = value.Location;
                uiElement.Size = value.Size;
                area = value;
            }
        }

        /// <summary>Initializes a new instance of <see cref="LoadableBar"/> class with the full bar texture and decrease direction.</summary>
        /// <param name="filledBarTexture">The <see cref="AnimationState"/> that is used for bar's visual.</param>
        /// <param name="decreaseDirection">The direction for decreasing bar fullness.</param>
        public CircularLoadableBar(AnimationState filledBarTexture, CircularBarDecreaseDirection decreaseDirection)
        {
            ArgumentNullException.ThrowIfNull(filledBarTexture);
            if (filledBarTexture.Width != filledBarTexture.Height) throw new ArgumentException("Texture must be square.");

            FilledBarTexture = filledBarTexture;
            DecreaseDirection = decreaseDirection;

            Radius = filledBarTexture.Width / 2f;
            currentBarTexture = DrawHelper.CreateRenderTarget(filledBarTexture.Width, filledBarTexture.Height);

            uiElement = new TextureDisplayInfo();

            ColorDataForTexture = new Color[filledBarTexture.Width * filledBarTexture.Height];
        }

        ~CircularLoadableBar()
        {
            Dispose();
        }

        public void Dispose()
        {
            currentBarTexture.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Updates the bar fullness depending on <paramref name="angle"/>. 
        /// Areas covered by the <paramref name="angle"/> will not be visible and <paramref name="angle"/> starts from top-middle point of the texture.
        /// </summary>
        /// <param name="angle">New fullness of the bar, expected value is in between 0 and 360.</param>
        public void UpdateBarFullness(int angle)
        {
            // let angle be between 0 and 360
            if (angle != 360)
            {
                while (angle < 0) angle += 360;
                angle %= 360;
            }

            IEnumerable<Point> VisiblePixels = GetTransparentPixelPositions(angle);

            ((Texture2D)FilledBarTexture).GetData(ColorDataForTexture);

            int oneSideLength = (int)(Radius * 2);
            if (DecreaseDirection == CircularBarDecreaseDirection.CounterClockwise)
            {
                foreach (var point in VisiblePixels)
                {
                    Point mirrorPoint = new Point((int)(2 * Radius - point.X), point.Y);

                    if (!(0 <= mirrorPoint.X && mirrorPoint.X < oneSideLength &&
                          0 <= mirrorPoint.Y && mirrorPoint.Y < oneSideLength)) continue;

                    ColorDataForTexture[mirrorPoint.Y * oneSideLength + mirrorPoint.X] = Color.Transparent;
                }
            }
            else
            {
                foreach (var point in VisiblePixels)
                {
                    if (!(0 <= point.X && point.X < oneSideLength &&
                          0 <= point.Y && point.Y < oneSideLength)) continue;

                    ColorDataForTexture[point.Y * oneSideLength + point.X] = Color.Transparent;
                }
            }

            currentBarTexture.SetData(ColorDataForTexture);

            // to let UI element know Texture of the element has changed
            uiElement.Texture = currentBarTexture;
            currentAngleFullness = angle;
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            return [uiElement];
        }

        public override void UpdateElements()
        {
            if (!FilledBarTexture.IsSingleTexture) UpdateBarFullness(currentAngleFullness);
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            if (willPlay) FilledBarTexture.Play();
            else FilledBarTexture.Stop();
        }

        /// <summary>Gets the pixel positions that will be transparent depending on angle.</summary>
        /// <returns>Returns the positions of pixels that will be transparent.</returns>
        private IEnumerable<Point> GetTransparentPixelPositions(int angle)
        {
            double a;
            double max = Radius;

            if (angle == 0) yield break;
            else if (angle == 180)
            {
                foreach (var pos in GetQuarterRectangles(2)) yield return pos;
                yield break;
            }
            else if (angle == 360)
            {
                foreach (var pos in GetQuarterRectangles(4)) yield return pos;
                yield break;
            }

            switch (angle)
            {
                case <= 45 and > 0: // 1B

                    a = Radius * (1 - Math.Tan(angle * Math.PI / 180));

                    // triangle area
                    for (float x = Radius; x < 2 * Radius - a; x++)
                    {
                        for (int y = 0; y < max; y++)
                        {
                            yield return new Point((int)x, y);
                        }
                        max -= Radius / (Radius - a);
                    }

                    break;
                case <= 90 and >= 45: // 1A

                    a = Radius * (1 - 1 / Math.Tan(angle * Math.PI / 180));

                    // triangle area
                    for (float x = Radius; x < 2 * Radius; x++)
                    {
                        for (double y = a; y < max; y++)
                        {
                            yield return new Point((int)x, (int)y);
                        }
                        max -= (Radius - a) / Radius;
                    }

                    // rectangle area
                    for (float x = Radius; x <= 2 * Radius; x++)
                    {
                        for (int y = 0; y < a; y++)
                        {
                            yield return new Point((int)x, y);
                        }
                    }

                    break;
                case <= 135 and >= 90: // 2B

                    a = Radius * (1 + 1 / Math.Tan(angle * Math.PI / 180));

                    // triangle area
                    for (float x = Radius; x <= 2 * Radius; x++)
                    {
                        for (float y = Radius; y < max; y++)
                        {
                            yield return new Point((int)x, (int)y);
                        }
                        max += (Radius - a) / Radius;
                    }

                    foreach (var p in GetQuarterRectangles(1)) yield return p;

                    break;
                case < 180 and >= 135: // 2A

                    a = Radius * (1 + Math.Tan(angle * Math.PI / 180));

                    // triangle area
                    for (float x = Radius; x < 2 * Radius - a; x++)
                    {
                        for (float y = Radius; y < max; y++)
                        {
                            yield return new Point((int)x, (int)y);
                        }
                        max += Radius / (Radius - a);
                    }

                    // rectangle area
                    for (float x = Radius * 2; x > 2 * Radius - a; x--)
                    {
                        for (float y = Radius; y <= Radius * 2; y++)
                        {
                            yield return new Point((int)x, (int)y);
                        }
                    }

                    foreach (var p in GetQuarterRectangles(1)) yield return p;

                    break;
                case <= 225 and > 180: // 3B

                    // triangle area
                    IEnumerable<Point> Points_1B = GetTransparentPixelPositions(angle - 180);
                    foreach (var p in Points_1B)
                    {
                        float xDiff = Radius - p.X;
                        float yDiff = Radius - p.Y;

                        yield return new Point((int)(Radius + xDiff), (int)(Radius + yDiff));
                    }

                    foreach (var p in GetQuarterRectangles(2)) yield return p;

                    break;
                case <= 270 and >= 225: // 3A

                    // triangle area
                    IEnumerable<Point> Points_1A = GetTransparentPixelPositions(angle - 180);
                    foreach (var p in Points_1A)
                    {
                        float xDiff = Radius - p.X;
                        float yDiff = Radius - p.Y;

                        yield return new Point((int)(Radius + xDiff), (int)(Radius + yDiff));
                    }

                    foreach (var p in GetQuarterRectangles(2)) yield return p;

                    break;
                case <= 315 and >= 270: // 4B

                    // triangle area
                    IEnumerable<Point> Points_2B = GetTransparentPixelPositions(angle - 180);
                    foreach (var p in Points_2B)
                    {
                        float xDiff = Radius - p.X;
                        float yDiff = Radius - p.Y;

                        yield return new Point((int)(Radius + xDiff), (int)(Radius + yDiff));
                    }

                    foreach (var p in GetQuarterRectangles(3)) yield return p;

                    break;
                case < 360 and >= 315: // 4A

                    // triangle area
                    IEnumerable<Point> Points_2A = GetTransparentPixelPositions(angle - 180);
                    foreach (var p in Points_2A)
                    {
                        float xDiff = Radius - p.X;
                        float yDiff = Radius - p.Y;

                        yield return new Point((int)(Radius + xDiff), (int)(Radius + yDiff));
                    }

                    foreach (var p in GetQuarterRectangles(3)) yield return p;

                    break;

            }
        }

        /// <summary>
        /// Gets the positions from the given <paramref name="region"/>. Texture have been divided by 4 different regions.<br/>
        /// 1 -> top-left part of the texture. <br/>
        /// 2 -> bottom-right part of the texture and region 1. <br/>
        /// 3 -> bottom-left part of the texture and region 2. <br/>
        /// 4 -> top-right part of the texture and region 3.
        /// </summary>
        /// <param name="region">The wanted region.</param>
        /// <returns>Returns the points got from wanted region.</returns>
        private IEnumerable<Point> GetQuarterRectangles(int region)
        {
            switch (region)
            {
                case 1:

                    for (float x = Radius; x <= Radius * 2; x++)
                        for (int y = 0; y < Radius; y++) 
                            yield return new Point((int)x, y);

                    break;
                case 2:

                    for (float x = Radius; x <= Radius * 2; x++)
                        for (float y = Radius; y < Radius * 2; y++)
                            yield return new Point((int)x, (int)y);

                    goto case 1;
                case 3:

                    for (int x = 0; x < Radius; x++)
                        for (float y = Radius; y < Radius * 2; y++)
                            yield return new Point(x, (int)y);

                    goto case 2;
                case 4:

                    for (int x = 0; x < Radius; x++)
                        for (int y = 0; y < Radius; y++)
                            yield return new Point(x, y);
               
                    goto case 3;
            }
        }
    }

    /// <summary>Presents the decrease direction of bar for <see cref="CircularLoadableBar"/>.</summary>
    public enum CircularBarDecreaseDirection
    {
        Clockwise,
        CounterClockwise
    }
}
