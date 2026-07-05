using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class Clickable : PopElement, IHoldable, ISelectable
    {
        private bool isHeld;
        private bool wasSelected;

        protected bool WasSelected => wasSelected;

        public bool IsHeld => isHeld;
        public HoldablePacket HoldPacket { get; set; }
        public SelectablePacket SelectablePacket { get; set; }

        public ICommander<Clickable>? Command { get; set; }
        public event Action<Clickable>? Clicked;

        public void Press()
        {
            wasSelected = false;
            OnClick();
        }

        protected virtual void OnClick()
        {
            Command?.Execute(this);
            Clicked?.Invoke(this);
        }

        public virtual void OnHoldStateChange(Input.PopWindowMouseEventArgs e, bool startedHolding)
        {
            if (!startedHolding && wasSelected && CanBeSelected(e.MousePosition))
            {
                Press();
            }

            this.isHeld = startedHolding;
            HoldPacket?.RaiseHoldStateChange(this, e, startedHolding);
        }

        public virtual void OnHolding(Input.PopWindowMouseEventArgs e)
        {
            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnSelect(Input.PopWindowMouseEventArgs e)
        {
            wasSelected = true;
            SelectablePacket?.RaiseSelected(this, e);
        }

        public virtual void OnSelectingOutside(Input.PopWindowMouseEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        public bool CanBeHeld(Point point)
        {
            return Boundaries.Contains(point);
        }

        public bool CanBeSelected(Point point)
        {
            return Boundaries.Contains(point);
        }
    }
}
