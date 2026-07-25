using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class Clickable : PopElement, IHoldable, IHoverable, ISelectable
    {
        private bool isHeld;
        private bool isHovered;
        private bool wasSelected;
        protected bool _enabled = true;

        protected bool WasSelected => wasSelected;

        public bool IsHeld => isHeld;
        public bool IsHovered => isHovered;
        public HoldablePacket HoldPacket { get; set; }
        public HoverablePacket HoverablePacket { get; set; }
        public SelectablePacket SelectablePacket { get; set; }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                EnableStatusChanged();
            }
        }
        public ICommander<Clickable>? ClickCommand { get; set; }

        public void Press()
        {
            wasSelected = false;
            OnClick();
        }

        protected virtual void EnableStatusChanged()
        {
        }

        protected virtual void OnClick()
        {
            ClickCommand?.Execute(this);
        }

        public virtual void OnHoldStateChange(PopWindowMouseEventArgs e, bool startedHolding)
        {
            if (!startedHolding && wasSelected && CanBeSelected(e.MousePosition))
            {
                Press();
            }

            this.isHeld = startedHolding;
            HoldPacket?.RaiseHoldStateChange(this, e, startedHolding);
        }

        public virtual void OnHolding(PopWindowMouseEventArgs e)
        {
            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnHoverStateChange(PopWindowMouseEventArgs e, bool startedHovering)
        {
            this.isHovered = startedHovering;
            HoverablePacket?.RaiseHoverStateChange(this, e, startedHovering);
        }

        public void OnHovering(PopWindowMouseEventArgs e)
        {
            HoverablePacket?.RaiseHovering(this, e);
        }

        public virtual void OnSelect(PopWindowMouseEventArgs e)
        {
            if (!Enabled)
                return;

            wasSelected = true;
            SelectablePacket?.RaiseSelected(this, e);
        }

        public virtual void OnSelectingOutside(PopWindowMouseEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        public bool CanBeHeld(Point point)
        {
            return Boundaries.Contains(point);
        }

        public bool CanBeHovered(Point point)
        {
            return Boundaries.Contains(point);
        }

        public bool CanBeSelected(Point point)
        {
            return Boundaries.Contains(point);
        }
    }
}
