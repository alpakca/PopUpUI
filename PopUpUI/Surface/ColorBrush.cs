using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using PopUpUI.Toolkit;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Surface
{
    /// <summary>Represents a <see cref="Microsoft.Xna.Framework.Color"/> that can be used as <see cref="AnimationState"/>.</summary>
    public class ColorBrush : IDisposable
    {
        private readonly AnimationState colorTexture;

        /// <summary>Gets the <see cref="Microsoft.Xna.Framework.Color"/> this brush contains.</summary>
        public Color Color { get; }

        /// <summary>Initializes a new instance of <see cref="ColorBrush"/> class with given <paramref name="color"/>.</summary>
        /// <param name="color">The color of the brush.</param>
        public ColorBrush(Color color)
        {
            Texture2D texture = DrawHelper.CreateTexture(1, 1);
            texture.SetData([color]);
            colorTexture = new AnimationState(texture);

            Color = color;
        }

        public void Dispose()
        {
            colorTexture.DisposeFrames();
            GC.SuppressFinalize(this);
        }

        public static implicit operator AnimationState(ColorBrush brush)
        {
            return brush.colorTexture;
        }
    }
}
