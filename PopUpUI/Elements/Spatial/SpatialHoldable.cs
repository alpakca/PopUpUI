using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;

namespace PopUpUI.Elements.Spatial
{
    /// <summary>Provides a base class for <see cref="IHoldable"/> interface with supporting visualization for hold state change using <see cref="Spatial.SpatialArea"/>.</summary>
    public class SpatialHoldable : SpatialPopUpElement, IHoldable, IBackgroundSupport
    {
        private bool isHeld;
        private float opacity = 1;
        private SpatialArea area;
        private readonly AnimationDisplayInfo backgroundUIElement;

        public bool IsHeld => isHeld;
        public MouseButtons ButtonsToHold { get; set; }

        public override float Opacity
        {
            get => opacity;
            set
            {
                backgroundUIElement.Opacity = value;
                opacity = value;
            }
        }
        public override SpatialArea SpatialArea
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

        public Predicate<PopUpElement> DoNotHoldIf { get; set; }

        public event PopUpElementHandler Held;
        public event PopUpElementHandler<bool> HoldStateChange;

        /// <summary>Initializes a new instance of <see cref="SpatialHoldable"/> class.</summary>
        public SpatialHoldable()
        {
            backgroundUIElement = new AnimationDisplayInfo();
        }

        public bool CanBeHeld(Point point)
        {
            return base.IsInside(point);
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

        void IHoldable.HoldStatusUpdate(PopUpWindowEventArgs e, bool isHeld)
        {
            if (!this.isHeld && isHeld)
            {
                HoldStateChange(this, e, true);
            }
            else if (this.isHeld && !isHeld)
            {
                HoldStateChange(this, e, false);
            }

            this.isHeld = isHeld;
            if (isHeld) Held(this, e);
        }
    }
}
