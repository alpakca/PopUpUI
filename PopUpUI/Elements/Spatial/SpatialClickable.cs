using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;
using System.Collections.Generic;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Elements.Spatial
{
    /// <summary>Presents a clickable object with <see cref="Spatial.SpatialArea"/>, that is a button can be held, hovered and selected.</summary>
    public class SpatialClickable : SpatialPopUpElement, IHoldable, IHoverable, ISelectable, IBackgroundSupport
    {
        private bool isHeld;
        private bool isHovered;
        private bool currentlySelected;
        private float opacity = 1;
        private SpatialArea area;
        private readonly AnimationDisplayInfo backgroundUIElement;

        public bool IsHeld => isHeld;
        public bool IsHovered => isHovered;
        public bool EnabledOutsideSelecting { get; set; }
        public MouseButtons ButtonsToHold { get; set; }
        public MouseButtons ButtonsToSelect { get; set; }

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
        public Predicate<PopUpElement> DoNotHoverIf { get; set; }
        public Predicate<PopUpElement> DoNotSelectIf { get; set; }

        public event PopUpElementHandler Held;
        public event PopUpElementHandler<bool> HoldStateChange;
        public event PopUpElementHandler<bool> HoverStateChange;
        public event PopUpElementHandler Hovering;
        public event PopUpElementHandler Selected;
        public event PopUpElementHandler OutsideSelected;

        /// <summary>Initializes a new instance of <see cref="SpatialClickable"/> class.</summary>
        public SpatialClickable()
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

        public bool CanBeHeld(Point point) => base.IsInside(point);
        public bool CanBeHovered(Point point) => base.IsInside(point);
        public bool CanBeSelected(Point point) => base.IsInside(point);

        void IHoldable.HoldStatusUpdate(PopUpWindowEventArgs e, bool isHeld)
        {
            // check for selecting clickable
            if (currentlySelected && this.isHeld && !isHeld && CanBeSelected(e.MousePoint))
            {
                Selected(this, e);
                currentlySelected = false;
            }

            if (!this.isHeld && isHeld)
            {
                HoldStateChange(this, e, true);
            }
            else if (this.isHeld && !isHeld)
            {
                HoldStateChange(this, e, false);
            }

            // variable update
            this.isHeld = isHeld;

            // regular update for visual & event
            if (isHeld)
            {
                Held(this, e);
            }
        }

        void IHoverable.OnHoverStateChange(PopUpWindowEventArgs e, bool isHovered)
        {
            this.isHovered = isHovered;
            HoverStateChange(this, e, isHovered);
        }

        void IHoverable.OnHovering(PopUpWindowEventArgs e)
        {
            Hovering(this, e);
        }

        void ISelectable.OnSelect(PopUpWindowEventArgs e)
        {
            currentlySelected = true;
        }

        void ISelectable.OnSelectingOutside(PopUpWindowEventArgs e)
        {
            OutsideSelected(this, e);
        }
    }
}
