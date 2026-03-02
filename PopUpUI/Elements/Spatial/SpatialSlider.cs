using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;

namespace PopUpUI.Elements.Spatial
{
    /// <summary>Represents a slider which can be slided through its range using <see cref="Spatial.SpatialArea"/>.</summary>
    internal class SpatialSlider : SpatialPopUpElement, IHoldable, IBackgroundSupport
    {
        private bool isHeld;
        private float opacity = 1;
        private Point heldOffsetFromSliderBounds;
        private SpatialArea area;
        private readonly AnimationDisplayInfo sliderUIElement;

        private int RangeWithoutSize => Range - (SlidingVertically ? SpatialArea.Height : SpatialArea.Width);
        private bool SlidingVertically => MovementDirection == MovementDirection.Vertical;

        public bool IsHeld => isHeld;
        public MouseButtons ButtonsToHold { get; set; }
        public Predicate<PopUpElement> DoNotHoldIf { get; set; }

        public override float Opacity
        {
            get => opacity;
            set
            {
                sliderUIElement.Opacity = value;

                opacity = value;
            }
        }
        public override SpatialArea SpatialArea
        {
            get => area;
            set
            {
                sliderUIElement.Location = value.Location;
                sliderUIElement.Size = value.Size;

                area = value;
            }
        }

        public int Range { get; }
        public MovementDirection MovementDirection { get; }

        public float SlidedPercentage
        {
            get
            {
                return 100 * (SlidingVertically ? (sliderUIElement.Location.Y - area.Y) / (float)RangeWithoutSize
                                                : (sliderUIElement.Location.X - area.X) / (float)RangeWithoutSize);
            }
            set
            {
                int offset = (int)(RangeWithoutSize * value / 100);

                sliderUIElement.Location = SlidingVertically ? new Point(sliderUIElement.Location.X, area.Y + offset)
                                                             : new Point(area.X + offset, sliderUIElement.Location.Y);
            }
        }

        public AnimationState BackgroundAnimation
        {
            get => sliderUIElement.Animation;
            set => sliderUIElement.Animation = value;
        }

        public event PopUpElementHandler Held;
        public event PopUpElementHandler<bool> HoldStateChange;

        public SpatialSlider(MovementDirection movementDirection, int range)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(range, 1);
            ThrowHelper.ThrowIfNotBetween((int)movementDirection, 1, 2);

            Range = range;
            MovementDirection = movementDirection;

            sliderUIElement = new AnimationDisplayInfo();
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            if (BackgroundAnimation != null) yield return sliderUIElement;
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            if (willPlay) sliderUIElement.Animation?.Play();
            else sliderUIElement.Animation?.Stop();
        }

        public bool CanBeHeld(Point point)
        {
            return sliderUIElement.Boundaries.Contains(point);
        }

        void IHoldable.HoldStatusUpdate(PopUpWindowEventArgs e, bool isHeld)
        {
            if (isHeld && !this.isHeld)
            {
                HoldStateChange(this, e, true);

                heldOffsetFromSliderBounds = e.MousePoint - sliderUIElement.Location;
            }
            else if (!isHeld && this.isHeld)
            {
                HoldStateChange(this, e, false);
            }

            this.isHeld = isHeld;

            if (isHeld)
            {
                Point change = e.MousePoint - sliderUIElement.Location - heldOffsetFromSliderBounds;
                SetSlider(sliderUIElement.Location + change);

                Held(this, e);
            }
        }

        private void SetSlider(Point location)
        {
            if (SlidingVertically)
            {
                sliderUIElement.Location = new Point(sliderUIElement.Location.X, Math.Clamp(location.Y, area.Y, area.Y + Range - SpatialArea.Height));
            }
            else
            {
                sliderUIElement.Location = new Point(Math.Clamp(location.X, area.X, area.X + Range - SpatialArea.Width), sliderUIElement.Location.Y);
            }
        }
    }
}
