using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class VerticalSlider : PopElement, IHoldable, ISelectable
    {
        private int yOffset;
        private bool isHeld;
        private int heldOffsetInHeight;

        protected int ScrollOffset => yOffset;

        public bool IsHeld => isHeld;
        public HoldablePacket HoldPacket { get; set; }
        public SelectablePacket SelectablePacket { get; set; }
        public ICommander<VerticalSlider>? SlideCommand { get; set; }

        public int Range 
        { 
            get; 
            set
            {
                field = value;
                Slide(0);
            }
        }
        public float ScrollPercentage => (float)yOffset / (Range - Boundaries.Height) * 100;

        public void Slide(int amount)
        {
            yOffset = Math.Clamp(yOffset + amount, 0, Range - Boundaries.Height);

            OnSliding();
        }

        public void SlideTo(int offset)
        {
            yOffset = Math.Clamp(offset, 0, Range - Boundaries.Height);

            OnSliding();
        }

        public virtual void OnHolding(Input.PopWindowMouseEventArgs e)
        {
            SlideTo(e.MousePosition.Y - Boundaries.Y - heldOffsetInHeight);

            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnHoldStateChange(Input.PopWindowMouseEventArgs e, bool startedHolding)
        {
            if (startedHolding)
            {
                heldOffsetInHeight = e.MousePosition.Y - Boundaries.Y - yOffset;
            }

            this.isHeld = startedHolding;
            HoldPacket?.RaiseHoldStateChange(this, e, startedHolding);
        }

        protected virtual void OnSliding()
        {
            SlideCommand?.Execute(this);
        }

        public void OnSelect(Input.PopWindowMouseEventArgs e)
        {
            if (IsSliderSelected(e.MousePosition))
            {
                SelectablePacket?.RaiseSelected(this, e);
                return;
            }

            // move slider to up
            if (e.MousePosition.Y - Boundaries.Y < yOffset)
            {
                Slide((e.MousePosition.Y - Boundaries.Y - yOffset) / 2 - Boundaries.Height);
            }
            // move slider to down
            else
            {
                Slide((e.MousePosition.Y - yOffset + Boundaries.Height) / 2);
            }
        }

        public void OnSelectingOutside(Input.PopWindowMouseEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        private bool IsSliderSelected(Point mousePosition)
        {
            return Boundaries.X <= mousePosition.X && mousePosition.X < Boundaries.X + Boundaries.Width &&
                   Boundaries.Y + yOffset <= mousePosition.Y && mousePosition.Y < Boundaries.Y + Boundaries.Height + yOffset;
        }

        public bool CanBeHeld(Point mousePosition)
        {
            return IsSliderSelected(mousePosition);
        }

        // lets select slider from its range, causing slider to advance to the mouse position direction
        public bool CanBeSelected(Point mousePosition)
        {
            return Boundaries.X <= mousePosition.X && mousePosition.X < Boundaries.X + Boundaries.Width &&
                   Boundaries.Y <= mousePosition.Y && mousePosition.Y < Boundaries.Y + Range;
        }
    }
}
