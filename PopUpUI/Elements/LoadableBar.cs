using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using PopUpUI.Toolkit;
using PopUpUI.Surface.Animation;
using PopUpUI.Surface;

namespace PopUpUI.Elements
{
    /// <summary>Presents a rectangular shaped bar whose fullness can be changed dynamically.</summary>
    public class LoadableBar : PopUpElement, IDisposable
    {
        private float currentFullness = 0;
        private float opacity = 1;
        private Rectangle area;

        private readonly RenderTarget2D currentBarTexture;
        private readonly AnimationState FilledBarTexture;
        private readonly TextureDisplayInfo uiElement;

        /// <summary>Direction of the bar where its going to be increased to.</summary>
        public BarIncreaseDirection IncreaseDirection { get; }
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

        /// <summary>Initializes a new instance of <see cref="LoadableBar"/> class with the full bar texture and increase direction.</summary>
        /// <param name="filledBarTexture">The <see cref="AnimationState"/> that is used for bar's visual.</param>
        /// <param name="increaseDirection">The direction for increasing bar fullness.</param>
        public LoadableBar(AnimationState filledBarTexture, BarIncreaseDirection increaseDirection)
        {
            ArgumentNullException.ThrowIfNull(filledBarTexture);

            FilledBarTexture = filledBarTexture;
            IncreaseDirection = increaseDirection;

            uiElement = new TextureDisplayInfo();

            currentBarTexture = DrawHelper.CreateRenderTarget(filledBarTexture.Width, filledBarTexture.Height);
        }
        
        ~LoadableBar() => Dispose();

        public void Dispose()
        {
            currentBarTexture.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>Updates the bar fullness depending on <paramref name="percentage"/>.</summary>
        /// <param name="percentage">New fullness of the bar, expected value is in between 0 and 100.</param>
        public void UpdateBarFullness(float percentage)
        {
            int newBarWidth = (int)MathHelper.Clamp(percentage * FilledBarTexture.Width / 100, 0, FilledBarTexture.Width);
            int newBarHeight = (int)MathHelper.Clamp(percentage * FilledBarTexture.Height / 100, 0, FilledBarTexture.Height);

            ((Texture2D)FilledBarTexture).CropTexture(writeOn: currentBarTexture, rect: IncreaseDirection switch
            {
                BarIncreaseDirection.DownToUp => new Rectangle(0, FilledBarTexture.Height - newBarHeight, FilledBarTexture.Width, newBarHeight),
                BarIncreaseDirection.UpToDown => new Rectangle(0, 0, FilledBarTexture.Width, newBarHeight),
                BarIncreaseDirection.RightToLeft => new Rectangle(FilledBarTexture.Width - newBarWidth, 0, newBarWidth, FilledBarTexture.Height),
                BarIncreaseDirection.LeftToRight => new Rectangle(0, 0, newBarWidth, FilledBarTexture.Height),
                _ => throw new Exception()
            });

            // to let UI element know Texture of the element has changed
            uiElement.Texture = currentBarTexture;
            currentFullness = percentage;
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            return [uiElement];
        }

        public override void UpdateElements()
        {
            FilledBarTexture.Update();
            if (!FilledBarTexture.IsSingleTexture) UpdateBarFullness(currentFullness);
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            if (willPlay) FilledBarTexture.Play();
            else FilledBarTexture.Stop();
        }
    }

    /// <summary>Presents the increase direction of bar for <see cref="LoadableBar"/>.</summary>
    public enum BarIncreaseDirection
    {
        DownToUp,
        UpToDown,
        RightToLeft,
        LeftToRight,
    }
}
