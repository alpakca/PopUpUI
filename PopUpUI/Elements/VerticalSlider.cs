using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class VerticalSlider : PopElement, IHoldable, ISelectable
    {
        protected int yOffset;
        protected bool isHeld;
        protected Point lastMousePosition;

        public bool IsHeld => isHeld;
        public IHoldablePacket HoldPacket { get; set; }
        public ISelectablePacket SelectablePacket { get; set; }

        public int Range { get; }

        public VerticalSlider(int range)
        {
            Range = range;
        }

        public void Slide(int amount)
        {
            yOffset = Math.Clamp(yOffset + amount, 0, Range - _boundaries.Height);

            OnSliding();
        }

        public virtual void OnHolding(PopUpWindowEventArgs e)
        {
            Slide(e.MousePoint.X - lastMousePosition.X);

            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnHoldStateChange(PopUpWindowEventArgs e, bool startedHolding)
        {
            if (startedHolding)
            {
                lastMousePosition = e.MousePoint;
            }

            this.isHeld = startedHolding;
            HoldPacket?.RaiseHoldStateChange(this, e, startedHolding);
        }

        protected virtual void OnSliding()
        {
        }

        public void OnSelect(PopUpWindowEventArgs e)
        {
            if (!IsSliderSelected(e.MousePoint))
            {
                Slide((Range - e.MousePoint.Y) / 2);
            }

            SelectablePacket?.RaiseSelected(this, e);
        }

        public void OnSelectingOutside(PopUpWindowEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        private bool IsSliderSelected(Point mousePosition)
        {
            return _boundaries.X <= mousePosition.X && mousePosition.X < _boundaries.X + _boundaries.Width &&
                   _boundaries.Y + yOffset <= mousePosition.Y && mousePosition.Y < _boundaries.Y + _boundaries.Height + yOffset;
        }

        public bool CanBeHeld(Point mousePosition)
        {
            return IsSliderSelected(mousePosition);
        }

        // lets select slider from its range, causing slider to advance to the mouse position direction
        public bool CanBeSelected(Point mousePosition)
        {
            return _boundaries.X <= mousePosition.X && mousePosition.X < _boundaries.X + Range &&
                   _boundaries.Y <= mousePosition.Y && mousePosition.Y < _boundaries.Y + _boundaries.Height;
        }
    }
}
