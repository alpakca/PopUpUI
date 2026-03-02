using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Text;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using PopUpUI.Surface.Animation;

#nullable enable

namespace PopUpUI.Elements
{
    /// <summary>Presents a text that is scrolled to see all its contents over time.</summary>
    public class ScrollingText : PopUpElement, IBackgroundSupport, IDisposable
    {
        private float offset = 0;
        private bool canBeScrolled;
        private float opacity = 1;
        private float _remainingReachingTime;
        private Rectangle area;
        private RenderTarget2D elementTexture;

        private readonly int SpaceBetweenTexts;
        private readonly TextBlock _textBlock;
        private readonly TextureDrawOptions drawOptions;

        private readonly AnimationDisplayInfo backgroundUIElement;
        private readonly TextureDisplayInfo textUIElement;

        /// <summary>Gets or sets the speed of the scroll, that is how many pixels scrolled within each second. Default value is 100 pixels per second.</summary>
        public float ScrollSpeed { get; set; } = 100;
        /// <summary>Gets or sets if the text will be scrolled to left or right. Default value is true.</summary>
        public bool ScrollsToLeft { get; set; } = true;
        /// <summary>Gets or sets the time that will be waited until scrolling continues, after text reached the start position, in milliseconds. Default is zero.</summary>
        public float TimeAfterReachingEnd { get; set; } = 0;
        /// <summary>Gets the <see cref="Text.TextBlock"/> this element contains.</summary>
        public TextBlock TextBlock => _textBlock;

        public override float Opacity
        {
            get => opacity;
            set
            {
                backgroundUIElement.Opacity = value;

                textUIElement.Opacity = value;
                opacity = value;
            }
        }
        public override Rectangle Area
        {
            get => area;
            set
            {
                backgroundUIElement.Location = value.Location;
                backgroundUIElement.Size = value.Size;

                textUIElement.Location = value.Location;
                area = value;
            }
        }

        public AnimationState BackgroundAnimation
        {
            get => backgroundUIElement.Animation;
            set => backgroundUIElement.Animation = value;
        }

        /// <summary>Initializes a new instance of <see cref="ScrollingText"/> class with given values.</summary>
        /// <param name="text">The TextBlock that will be used to get text from.</param>
        /// <param name="spaceBetweenTexts">The space length between texts while scrolling.</param>
        /// <param name="visibleArea">The visible area that text will be shown.</param>
        /// <param name="options">Options to control drawing behaviour.</param>
        public ScrollingText(TextBlock text, int spaceBetweenTexts, Point visibleArea, TextureDrawOptions? options)
        {
            ArgumentNullException.ThrowIfNull(text);

            _textBlock = text;
            area = new Rectangle(new Point(), visibleArea);
            SpaceBetweenTexts = spaceBetweenTexts;
            drawOptions = options ?? TextureDrawOptions.Default;

            elementTexture = DrawHelper.CreateRenderTarget(visibleArea.X, visibleArea.Y);

            backgroundUIElement = new AnimationDisplayInfo();
            textUIElement = new TextureDisplayInfo { Texture = elementTexture };
        }

        /// <summary>Initializes a new instance of <see cref="ScrollingText"/> class with given values.</summary>
        /// <param name="text">The TextBlock that will be used to get text from.</param>
        /// <param name="spaceBetweenTexts">The space length between texts while scrolling.</param>
        /// <param name="visibleArea">The visible area that text will be shown.</param>
        public ScrollingText(TextBlock text, int spaceBetweenTexts, Point visibleArea) : this(text, spaceBetweenTexts, visibleArea, null)
        {
        }

        ~ScrollingText()
        {
            Dispose();
        }

        public void Dispose()
        {
            elementTexture.Dispose();
            GC.SuppressFinalize(this);
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            if (BackgroundAnimation != null) yield return backgroundUIElement;
            yield return textUIElement;
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            if (willPlay) backgroundUIElement.Animation?.Play();
            else backgroundUIElement.Animation?.Stop();
        }

        public override void UpdateElements()
        {
            if (_remainingReachingTime > 0)
            {
                _remainingReachingTime -= Time.DeltaTime * 1000;
                return;
            }

            if (_textBlock.HasChanged())
            {
                canBeScrolled = _textBlock.Size.X > area.Width;

                if (!canBeScrolled) PinTheText();
            }

            if (canBeScrolled)
            {
                if (ScrollsToLeft) Scroll(offset - ScrollSpeed * Time.DeltaTime);
                else Scroll(offset + ScrollSpeed * Time.DeltaTime);
            }
        }

        /// <summary>Changes the area that can be seen and checks if text can be scrolled with the new area.</summary>
        /// <param name="newArea">The new area that will be shown.</param>
        public void ChangeVisibleArea(Point newArea)
        {
            if (area.Size == newArea) return;

            elementTexture.Dispose();

            elementTexture = DrawHelper.CreateRenderTarget(newArea.X, newArea.Y);
            textUIElement.Texture = elementTexture;

            area.Size = newArea;

            canBeScrolled = _textBlock.Size.X > area.Width;
            if (!canBeScrolled) PinTheText();
        }

        /// <summary>Scrolls to given offset with updating <see cref="offset"/>.</summary>
        /// <param name="newOffset">The new offset.</param>
        private void Scroll(float newOffset)
        {
            offset = newOffset;
            Texture2D texture = _textBlock.GetTextTexture();

            DrawHelper.SetRenderTarget(elementTexture);
            DrawHelper.BeginBatch(drawOptions);

            DrawHelper.Batch.Draw(texture, new Vector2(offset, 0), Color.White);

            if (ScrollsToLeft) DrawHelper.Batch.Draw(texture, new Vector2(offset + texture.Width + SpaceBetweenTexts, 0), Color.White);
            else DrawHelper.Batch.Draw(texture, new Vector2(offset - texture.Width - SpaceBetweenTexts, 0), Color.White);

            DrawHelper.EndBatch();

            textUIElement.Texture = elementTexture;

            if (ScrollsToLeft && -offset >= texture.Width + SpaceBetweenTexts)
            {
                offset += texture.Width + SpaceBetweenTexts;
                _remainingReachingTime = TimeAfterReachingEnd;
            }
            else if (!ScrollsToLeft && offset >= area.Width + SpaceBetweenTexts)
            {
                offset -= texture.Width + SpaceBetweenTexts;
                _remainingReachingTime = TimeAfterReachingEnd;
            }
        }

        /// <summary>Pins the text to top-left corner if its texture's length is shorter than bounds.Width.</summary>
        private void PinTheText()
        {
            Texture2D textTexture = _textBlock.GetTextTexture();

            DrawHelper.SetRenderTarget(elementTexture);
            DrawHelper.BeginBatch(drawOptions);

            DrawHelper.Batch.Draw(textTexture, new Vector2(0, 0), Color.White);

            DrawHelper.EndBatch();
        }
    }
}
