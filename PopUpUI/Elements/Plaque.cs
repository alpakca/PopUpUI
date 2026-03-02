using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Text;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Elements
{
    /// <summary>Presents an object that can display either <see cref="AnimationState"/> or <see cref="TextBlock"/>.</summary>
    public class Plaque : PopUpElement
    {
        private float opacity = 1;
        private Rectangle area;
        private readonly TextBlock _displayingTextBlock;
        private readonly IUIDisplayInfo uiElement;

        /// <summary>Indicates that this Plaque is displaying texture.</summary>
        public bool IsDisplayingTexture { get; }
        /// <summary>Gets or sets the <see cref="AnimationState"/> this Plaque is displaying.</summary>
        public AnimationState DisplayingAnimation
        {
            get
            {
                ThrowIfTypeDoesNotMatch(textureAction: true);

                return (uiElement as AnimationDisplayInfo).Animation;
            }
            set
            {
                ThrowIfTypeDoesNotMatch(textureAction: true);

                (uiElement as AnimationDisplayInfo).Animation = value;
            }
        }
        /// <summary>Gets the <see cref="TextBlock"/> this Plaque is displaying.</summary>
        public TextBlock DisplayingTextBlock
        {
            get
            {
                ThrowIfTypeDoesNotMatch(textureAction: false);

                return _displayingTextBlock;
            }
        }
        /// <summary>Gets the texture from <see cref="DisplayingAnimation"/> if it displays texture; else it gets from <see cref="DisplayingTextBlock"/>.</summary>
        public Texture2D Texture
        {
            get
            {
                if (IsDisplayingTexture) return DisplayingAnimation;
                else return DisplayingTextBlock.GetTextTexture();
            }
        }

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

        /// <summary>Initializes a new instance of <see cref="Plaque"/> class with given <see cref="AnimationState"/>.</summary>
        /// <param name="animationState">A <see cref="Animator"/> that this Plaque will display.</param>
        public Plaque(AnimationState animationState)
        {
            ArgumentNullException.ThrowIfNull(animationState);

            uiElement = new AnimationDisplayInfo { Animation = animationState };
            IsDisplayingTexture = true;
        }

        /// <summary>Initializes a new instance of <see cref="Plaque"/> class with given <see cref="TextBlock"/>.</summary>
        /// <param name="text">A <see cref="TextBlock"/> that this Plaque will display.</param>
        public Plaque(TextBlock text)
        {
            ArgumentNullException.ThrowIfNull(text);

            _displayingTextBlock = text;
            uiElement = new TextureDisplayInfo { Texture = _displayingTextBlock.GetTextTexture() };
            IsDisplayingTexture = false;
        }

        public override void UpdateElements()
        {
            if (!IsDisplayingTexture)
            {
                if (_displayingTextBlock.HasChanged()) (uiElement as TextureDisplayInfo).Texture = _displayingTextBlock.GetTextTexture();
            }
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            return [uiElement];
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            if (willPlay) (uiElement as AnimationDisplayInfo)?.Animation.Play();
            else (uiElement as AnimationDisplayInfo)?.Animation.Stop();
        }

        private void ThrowIfTypeDoesNotMatch(bool textureAction)
        {
            if (!IsDisplayingTexture && textureAction) throw new ArgumentException("This Plaque does not display texture.");

            if (IsDisplayingTexture && !textureAction) throw new ArgumentException("This Plaque does not display text.");
        }
    }
}
