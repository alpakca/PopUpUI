using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class Movable : PopElement, IHoldable
    {
        private bool isHeld;
        private Point lastMousePosition;

        public bool IsHeld => isHeld;
        public MovementDirection MovementDirection { get; }

        public HoldablePacket HoldPacket { get; set; }
        public ICommander<Movable>? PositionChangeCommand { get; set; }

        public Movable(MovementDirection movementDirection)
        {
            MovementDirection = movementDirection;
        }

        public virtual void OnHolding(Input.PopWindowMouseEventArgs e)
        {
            UpdatePosition(e.MousePosition);

            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnHoldStateChange(Input.PopWindowMouseEventArgs e, bool startedHolding)
        {
            if (startedHolding)
            {
                lastMousePosition = e.MousePosition;
            }

            isHeld = startedHolding;
            HoldPacket?.RaiseHoldStateChange(this, e, startedHolding);
        }

        protected virtual void OnPositionChange()
        {
            PositionChangeCommand?.Execute(this);
        }

        public bool CanBeHeld(Point point)
        {
            return base.InsideBoundaries(point);
        }

        private void UpdatePosition(Point mousePosition)
        {
            int widthChange = mousePosition.X - lastMousePosition.X;
            int heightChange = mousePosition.Y - lastMousePosition.Y;

            Point newLocation = Boundaries.Location + MovementDirection switch
            {
                MovementDirection.Any => new Point(widthChange, heightChange),
                MovementDirection.Horizontal => new Point(widthChange, 0),
                MovementDirection.Vertical => new Point(0, heightChange),

                _ => throw new Exception()
            };

            lastMousePosition = mousePosition;
            Boundaries = new Rectangle(newLocation, Boundaries.Size);

            if (newLocation != Point.Zero)
            {
                OnPositionChange();
            }
        }
    }
}
