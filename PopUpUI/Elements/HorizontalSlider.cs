using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class HorizontalSlider : PopElement, IHoldable, ISelectable
    {
        protected int xOffset;
        protected bool isHeld;

        private int heldOffsetInWidth;
        protected Point lastMousePosition;

        public bool IsHeld => isHeld;
        public HoldablePacket HoldPacket { get; set; }
        public SelectablePacket SelectablePacket { get; set; }

        /// <summary>Gets the range of the slider. This range is different from Boundaries.Width, 
        /// as Boundaries.Width refers to width of sliding object.</summary>
        public int Range { get; }

        public HorizontalSlider(int range)
        {
            Range = range;
        }

        public void Slide(int amount)
        {
            xOffset = Math.Clamp(xOffset + amount, 0, Range - _boundaries.Width);

            OnSliding();
        }

        public void SlideTo(int offset)
        {
            xOffset = Math.Clamp(offset, 0, Range - _boundaries.Width);

            OnSliding();
        }

        public virtual void OnHolding(PopUpWindowEventArgs e)
        {
            SlideTo(e.MousePoint.X - _boundaries.X - heldOffsetInWidth);
            lastMousePosition = e.MousePoint;

            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnHoldStateChange(PopUpWindowEventArgs e, bool startedHolding)
        {
            if (startedHolding)
            {
                lastMousePosition = e.MousePoint;
                heldOffsetInWidth = e.MousePoint.X - _boundaries.X - xOffset;
            }

            this.isHeld = startedHolding;
            HoldPacket?.RaiseHoldStateChange(this, e, startedHolding);
        }

        protected virtual void OnSliding()
        {
        }

        public virtual void OnSelect(PopUpWindowEventArgs e)
        {
            if (IsSliderSelected(e.MousePoint))
            {
                SelectablePacket?.RaiseSelected(this, e);
                return;
            }

            // move slider to left
            if (e.MousePoint.X - _boundaries.X < xOffset)
            {
                Slide((e.MousePoint.X - _boundaries.X - xOffset) / 2 - _boundaries.Width);
            }
            // move slider to right
            else
            {
                Slide((e.MousePoint.X - xOffset + _boundaries.Width) / 2);
            }
        }

        public virtual void OnSelectingOutside(PopUpWindowEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        protected bool IsSliderSelected(Point mousePosition)
        {
            return _boundaries.X + xOffset <= mousePosition.X && mousePosition.X < _boundaries.X + _boundaries.Width + xOffset &&
                   _boundaries.Y <= mousePosition.Y && mousePosition.Y < _boundaries.Y + _boundaries.Height;
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
