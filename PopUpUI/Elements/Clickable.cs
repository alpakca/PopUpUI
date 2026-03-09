using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using System;

namespace PopUpUI.NewElements
{
    internal class Clickable : PopElement, IHoldable, ISelectable
    {
        protected bool isHeld;
        protected bool wasSelected;

        public bool IsHeld => isHeld;
        public IHoldablePacket HoldPacket { get; set; }
        public ISelectablePacket SelectablePacket { get; set; }

        public event Action<Clickable> Clicked;

        protected virtual void OnClick()
        {
            Clicked?.Invoke(this);
        }

        public virtual void OnHoldStateChange(PopUpWindowEventArgs e, bool startedHolding)
        {
            if (!startedHolding && wasSelected && CanBeSelected(e.MousePoint))
            {
                wasSelected = false;
                OnClick();
            }

            this.isHeld = startedHolding;
            HoldPacket?.RaiseHoldStateChange(this, e, startedHolding);
        }

        public virtual void OnHolding(PopUpWindowEventArgs e)
        {
            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnSelect(PopUpWindowEventArgs e)
        {
            wasSelected = true;
            SelectablePacket?.RaiseSelected(this, e);
        }

        public virtual void OnSelectingOutside(PopUpWindowEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        public bool CanBeHeld(Point point)
        {
            return _boundaries.Contains(point);
        }

        public bool CanBeSelected(Point point)
        {
            return _boundaries.Contains(point);
        }
    }
}
