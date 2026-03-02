using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;
using System;
using System.Collections.Generic;

namespace PopUpUI.Elements.Spatial
{
    /// <summary>Presents a mobile object, that is an object that can be dragged through screen using <see cref="Spatial.SpatialArea"/>.</summary>
    public class SpatialMobileObject : SpatialPopUpElement, IHoldable, IBackgroundSupport
    {
        private bool isHeld;
        private bool movementStarted;
        private float opacity = 1;
        private Point HeldPoint;
        private SpatialArea area;
        private readonly AnimationDisplayInfo backgroundUIElement;

        public bool IsHeld => isHeld;
        public MouseButtons ButtonsToHold { get; set; }
        /// <summary>The direction mobile object can move to. Default value is <see cref="MoveDirection.Any"/>.</summary>
        public MovementDirection MovementDirection { get; }

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
        /// <summary>Gets or sets a boundry for mobile object to move to. Default value lets object to move anywhere.</summary>
        public Predicate<Point> MovementBoundary { get; set; }

        public event PopUpElementHandler Held;
        public event PopUpElementHandler<bool> HoldStateChange;
        /// <summary>Raised when movement of the object has started. Point returns element's first held position.</summary>
        public event Action<SpatialMobileObject, Point> MovementStarted;
        /// <summary>Raised when object is moving. Point returns the change of element's position.</summary>
        public event Action<SpatialMobileObject, Point> Moving;
        /// <summary>Raised when movement of the object has ended. Point returns element's last held position.</summary>
        public event Action<SpatialMobileObject, Point> MovementEnded;

        /// <summary>Initializes a new instance of <see cref="SpatialMobileObject"/> class given <paramref name="movementDirection"/>.</summary>
        /// <param name="movementDirection">The direction mobile object can move to.</param>
        public SpatialMobileObject(MovementDirection movementDirection)
        {
            MovementDirection = movementDirection;
            MovementBoundary = p => true;

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

        private void SetToNewPoint(Point point)
        {
            if (!isHeld || !CanMove(point)) return;

            if (!movementStarted)
            {
                movementStarted = true;
                HeldPoint = point;
                MovementStarted?.Invoke(this, HeldPoint);
                return;
            }

            int widthChange = point.X - HeldPoint.X;
            int heightChange = point.Y - HeldPoint.Y;
            HeldPoint = point;

            area = area.WithOffset(MovementDirection switch
            {
                MovementDirection.Any => new Point(SpatialArea.X + widthChange, SpatialArea.Y + heightChange),
                MovementDirection.Vertical => new Point(SpatialArea.X, SpatialArea.Y + heightChange),
                MovementDirection.Horizontal => new Point(SpatialArea.X + widthChange, SpatialArea.Y),
                _ => throw new Exception()
            } - this.area.Location);

            Moving?.Invoke(this, new Point(widthChange, heightChange));
            backgroundUIElement.Location = area.Location;
        }

        private bool CanMove(Point p)
        {
            return MovementBoundary(p);
        }

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
            if (!isHeld && movementStarted) // movement has ended
            {
                movementStarted = false;
                MovementEnded?.Invoke(this, HeldPoint);
            }

            if (isHeld)
            {
                SetToNewPoint(e.MousePoint);
                Held(this, e);
            }
        }

        public bool CanBeHeld(Point point)
        {
            return base.IsInside(point);
        }
    }
}
