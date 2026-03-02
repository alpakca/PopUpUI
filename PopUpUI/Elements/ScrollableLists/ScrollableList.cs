using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using PopUpUI.Surface;
using PopUpUI.Containers;
using PopUpUI.Surface.Animation;

#nullable enable

namespace PopUpUI.Elements
{
    /// <summary>Presents a list that their nodes can be dynamically changed, placed anywhere and can be scrolled to see the nodes.</summary>
    public partial class ScrollableList : PopUpElement, IBackgroundSupport, IDisposable
    {
        private bool scrollStarted;
        private bool autoScrollDistanceCalculationOn = true;
        private Point lastHeldPointOnScrolling;

        private readonly TextureDrawOptions drawOptions;
        private readonly ListOperations operations;
        private readonly TextureDisplayInfo areaUIElement;
        private readonly AnimationDisplayInfo backgroundUIElement;

        private bool willBeUpdated;
        private float opacity = 1;
        private double scrolledDistance = 0;
        private double maxScrollDistance = double.NaN;
        private Rectangle area;

        private bool ScrollsVertically => ScrollDirection == MovementDirection.Vertical;

        /// <summary>Gets or sets the speed of scrolling through <see cref="IMouseWheelReceiver"/>. Default value is 24.</summary>
        public int ScrollSpeed { get; set; } = 24;
        /// <summary>Gets the distance that have scrolled.</summary>
        public double ScrolledDistance => scrolledDistance;
        /// <summary>Gets the direction list can be scrolled to.</summary>
        public MovementDirection ScrollDirection { get; }
        /// <summary>Gets or sets the option for updating the list's visual.</summary>
        public ScrollableListUpdateOptions UpdateOption { get; set; } = ScrollableListUpdateOptions.Default;
        /// <summary>Nodes of the list.</summary>
        public PopUpElementCollection Nodes { get; }

        /// <summary>Gets or sets the maximum distance to be scrolled. 
        /// If it is set to <see cref="double.NaN"/>, it will be set to the rightmost position of its nodes. Default value is <see cref="double.NaN"/>.</summary>
        public double MaxScrollDistance
        {
            get => maxScrollDistance;
            set
            {
                if (double.IsNaN(value))
                {
                    autoScrollDistanceCalculationOn = true;
                    CalculateMaxScrollDistanceIfNeeded();
                }
                else
                {
                    autoScrollDistanceCalculationOn = false;
                    maxScrollDistance = value;
                }

                Reload();
            }
        }

        public override float Opacity
        {
            get => opacity;
            set
            {
                areaUIElement.Opacity = value;
                backgroundUIElement.Opacity = value;

                opacity = value;
                willBeUpdated = true;
            }
        }
        public override Rectangle Area
        {
            get => area;
            set
            {
                areaUIElement.Location = value.Location;

                backgroundUIElement.Boundaries = value;

                area.Location = value.Location;
                willBeUpdated = true;
            }
        }

        public AnimationState BackgroundAnimation
        {
            get => backgroundUIElement.Animation;
            set => backgroundUIElement.Animation = value;
        }

        /// <summary>Raised when list is scrolled. Includes the new offset of the list.</summary>
        public event Action<ScrollableList, double> Scrolled = delegate { };

        /// <summary>Initializes a new instance of <see cref="ScrollableList"/> class with given values.</summary>
        /// <param name="scrollDirection">The direction this list going to scroll to.</param>
        /// <param name="visibleArea">The visible area list will show.</param>
        /// <param name="options">Options to control drawing behaviour.</param>
        /// <exception cref="ArgumentException"></exception>
        public ScrollableList(MovementDirection scrollDirection, Point visibleArea, TextureDrawOptions? options)
        {
            if (scrollDirection is < (MovementDirection)1 or > (MovementDirection)2)
                throw new ArgumentException($"ScrollableList only accepts {MovementDirection.Horizontal} and {MovementDirection.Vertical} as valid MovementDirection.");

            ScrollDirection = scrollDirection;
            area = new Rectangle(new Point(), visibleArea);
            areaUIElement = new TextureDisplayInfo();
            backgroundUIElement = new AnimationDisplayInfo();

            ButtonsToHold = MouseButtons.Any;
            ButtonsToSelect = MouseButtons.Any;

            drawOptions = options ?? TextureDrawOptions.Default;
            operations = new ListOperations(this);

            Nodes = new PopUpElementCollection();
            Nodes.ChangeOfCollection += ListChangedCheck;
        }

        /// <summary>Initializes a new instance of <see cref="ScrollableList"/> class with given values.</summary>
        /// <param name="scrollDirection">The direction this list going to scroll to.</param>
        /// <param name="visibleArea">The visible area list will show.</param>
        /// <exception cref="ArgumentException"></exception>
        public ScrollableList(MovementDirection scrollDirection, Point visibleArea) : this(scrollDirection, visibleArea, null)
        {
        }

        ~ScrollableList()
        {
            Dispose();
        }

        public void Dispose()
        {
            operations.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>Scrolls to given offset.</summary>
        /// <param name="value">Scroll offset.</param>
        public void Scroll(double value)
        {
            scrolledDistance = Math.Clamp(value, 0, MaxScrollDistance);
            Scrolled(this, scrolledDistance);

            CheckForUpdateCondition(ScrollableListUpdateOptions.Scrolled);
        }

        /// <summary>Changes the area list shows.</summary>
        /// <param name="newVisibleArea">The new area list will show.</param>
        public void ChangeVisibleArea(Point newVisibleArea)
        {
            if (area.Size == newVisibleArea) return;

            area.Size = newVisibleArea;
            operations.VisibleAreaChanged();

            Reload();
        }

        /// <summary>Reloads the list. Can be used to apply any changes made in bounds of any node.</summary>
        public void Reload()
        {
            CalculateMaxScrollDistanceIfNeeded();

            scrolledDistance = Math.Clamp(scrolledDistance, 0, MaxScrollDistance);

            willBeUpdated = true;
        }

        public override void UpdateElements()
        {
            operations.UpdateNodesAndTexture();
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            if (BackgroundAnimation != null) yield return backgroundUIElement;
            yield return areaUIElement;
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            if (willPlay) backgroundUIElement.Animation?.Play();
            else backgroundUIElement.Animation?.Stop();
        }

        private void CalculateMaxScrollDistanceIfNeeded()
        {
            if (!autoScrollDistanceCalculationOn) return;

            double newMax = 0;

            if (ScrollsVertically)
            {
                foreach (var node in Nodes)
                {
                    if (node.Visibility == Visibility.Hidden) continue;

                    newMax = Math.Clamp(node.Area.Bottom - area.Height, newMax, double.MaxValue);
                }
            }
            else
            {
                foreach (var node in Nodes)
                {
                    if (node.Visibility == Visibility.Hidden) continue;

                    newMax = Math.Clamp(node.Area.Right - area.Width, newMax, double.MaxValue);
                }
            }

            maxScrollDistance = newMax;
        }

        private void CheckForUpdateCondition(ScrollableListUpdateOptions operation)
        {
            if (UpdateOption.HasFlag(operation)) willBeUpdated = true;
        }

        private void ListChangedCheck(PopUpElementCollection sender, CollectionChangeAction action)
        {
            CalculateMaxScrollDistanceIfNeeded();
            CheckForUpdateCondition(ScrollableListUpdateOptions.NodeListChanged);
        }
    }
}
