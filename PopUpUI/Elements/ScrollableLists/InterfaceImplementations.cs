using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public partial class ScrollableList : IHoldable, IHoverable, ISelectable, IMouseWheelReceiver
    {
        private bool isHeld;
        private bool isHovered;

        public bool IsHeld => isHeld;
        public bool IsHovered => isHovered;

        public bool EnabledOutsideSelecting { get; set; }
        public MouseButtons ButtonsToSelect { get; set; }
        public MouseButtons ButtonsToHold { get; set; }

        public Predicate<PopUpElement> DoNotHoldIf { get; set; }
        public Predicate<PopUpElement> DoNotHoverIf { get; set; }
        public Predicate<PopUpElement> DoNotSelectIf { get; set; }
        public Predicate<PopUpElement> DoNotReceiveWheelChangeIf { get; set; }

        public event PopUpElementHandler Held;
        public event PopUpElementHandler<bool> HoldStateChange;
        public event PopUpElementHandler Hovering;
        public event PopUpElementHandler<bool> HoverStateChange;
        public event PopUpElementHandler Selected;
        public event PopUpElementHandler OutsideSelected;
        public event PopUpElementHandler MouseWheelChanged;

        public bool CanBeHeld(Point point) => base.IsInside(point);
        public bool CanBeHovered(Point point) => base.IsInside(point);
        public bool CanBeSelected(Point point) => base.IsInside(point);
        public bool CanReceiveWheelChangeInput(Point point) => base.IsInside(point);

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

            bool anyElementHeld = operations.Held(e);
            if (anyElementHeld) return;

            if (!isHeld && scrollStarted) // movement has ended
            {
                scrollStarted = false;
            }

            if (isHeld)
            {
                if (!scrollStarted)
                {
                    lastHeldPointOnScrolling = e.MousePoint;
                    scrollStarted = true;
                    return;
                }

                int offset = ScrollsVertically ? e.MousePoint.Y - lastHeldPointOnScrolling.Y : e.MousePoint.X - lastHeldPointOnScrolling.X;
                lastHeldPointOnScrolling = e.MousePoint;

                Scroll(scrolledDistance - offset);

                Held(this, e);
            }
        }

        void IHoverable.OnHovering(PopUpWindowEventArgs e)
        {
            operations.Hovering(e);
            Hovering(this, e);
        }

        void IHoverable.OnHoverStateChange(PopUpWindowEventArgs e, bool isHovered)
        {
            this.isHovered = isHovered;

            operations.HoverStateChange(e, isHovered);
            HoverStateChange(this, e, isHovered);
        }

        void ISelectable.OnSelect(PopUpWindowEventArgs e)
        {
            operations.Selected(e);
            operations.CheckForOutsideSelected(e);
            Selected(this, e);
        }

        void ISelectable.OnSelectingOutside(PopUpWindowEventArgs e)
        {
            OutsideSelected(this, e);
        }

        void IMouseWheelReceiver.OnMouseWheelChangeReceive(PopUpWindowEventArgs e)
        {
            Scroll(ScrollSpeed * 100d / -e.MouseWheelChange + scrolledDistance);

            MouseWheelChanged(this, e);
        }
    }
}
