using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class HorizontalSlider : PopElement, IHoldable, ISelectable
    {
        private int xOffset;
        private bool isHeld;
        private int heldOffsetInWidth;

        protected int ScrollOffset => xOffset;

        public bool IsHeld => isHeld;
        public HoldablePacket HoldPacket { get; set; }
        public SelectablePacket SelectablePacket { get; set; }

        /// <summary>Gets the range of the slider. This range is different from Boundaries.Width, 
        /// as Boundaries.Width refers to width of sliding object.</summary>
        public int Range { get; }
        public float ScrollPercentage => (float)xOffset / (Range - Boundaries.Width) * 100;

        public HorizontalSlider(int range)
        {
            Range = range;
        }

        public void Slide(int amount)
        {
            xOffset = Math.Clamp(xOffset + amount, 0, Range - Boundaries.Width);

            OnSliding();
        }

        public void SlideTo(int offset)
        {
            xOffset = Math.Clamp(offset, 0, Range - Boundaries.Width);

            OnSliding();
        }

        public virtual void OnHolding(PopUpWindowEventArgs e)
        {
            SlideTo(e.MousePoint.X - Boundaries.X - heldOffsetInWidth);

            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnHoldStateChange(PopUpWindowEventArgs e, bool startedHolding)
        {
            if (startedHolding)
            {
                heldOffsetInWidth = e.MousePoint.X - Boundaries.X - xOffset;
            }

            isHeld = startedHolding;
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
            if (e.MousePoint.X - Boundaries.X < xOffset)
            {
                Slide((e.MousePoint.X - Boundaries.X - xOffset) / 2 - Boundaries.Width);
            }
            // move slider to right
            else
            {
                Slide((e.MousePoint.X - xOffset + Boundaries.Width) / 2);
            }
        }

        public virtual void OnSelectingOutside(PopUpWindowEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        protected bool IsSliderSelected(Point mousePosition)
        {
            return Boundaries.X + xOffset <= mousePosition.X && mousePosition.X < Boundaries.X + Boundaries.Width + xOffset &&
                   Boundaries.Y <= mousePosition.Y && mousePosition.Y < Boundaries.Y + Boundaries.Height;
        }

        public bool CanBeHeld(Point mousePosition)
        {
            return IsSliderSelected(mousePosition);
        }

        // lets select slider from its range, causing slider to advance to the mouse position direction
        public bool CanBeSelected(Point mousePosition)
        {
            return Boundaries.X <= mousePosition.X && mousePosition.X < Boundaries.X + Range &&
                   Boundaries.Y <= mousePosition.Y && mousePosition.Y < Boundaries.Y + Boundaries.Height;
        }
    }
}
