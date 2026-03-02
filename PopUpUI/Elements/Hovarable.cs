using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;

namespace PopUpUI.Elements
{
    /// <summary>Provides a base class for <see cref="IHoverable"/> interface with supporting visualization for hover state change.</summary>
    public class Hoverable : PopUpElement, IHoverable, IBackgroundSupport
    {
        private bool isHovered;
        private float opacity = 1;
        private Rectangle area;
        private readonly AnimationDisplayInfo backgroundUIElement;

        public bool IsHovered => isHovered;
        public override float Opacity
        {
            get => opacity;
            set
            {
                backgroundUIElement.Opacity = value;
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

                area = value;
            }
        }

        public AnimationState BackgroundAnimation
        {
            get => backgroundUIElement.Animation;
            set => backgroundUIElement.Animation = value;
        }

        public Predicate<PopUpElement> DoNotHoverIf { get; set; }

        public event PopUpElementHandler Hovering;
        public event PopUpElementHandler<bool> HoverStateChange;

        /// <summary>Initializes a new instance of <see cref="Hoverable"/> class.</summary>
        public Hoverable()
        {
            backgroundUIElement = new AnimationDisplayInfo();
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            if (BackgroundAnimation != null) yield return backgroundUIElement;
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            if (willPlay) backgroundUIElement.Animation?.Play();
            else backgroundUIElement.Animation?.Stop();
        }

        public bool CanBeHovered(Point point)
        {
            return base.IsInside(point);
        }

        void IHoverable.OnHovering(PopUpWindowEventArgs e)
        {
            Hovering(this, e);
        }

        void IHoverable.OnHoverStateChange(PopUpWindowEventArgs e, bool isHovered)
        {
            this.isHovered = isHovered;
            HoverStateChange(this, e, isHovered);
        }
    }
}
