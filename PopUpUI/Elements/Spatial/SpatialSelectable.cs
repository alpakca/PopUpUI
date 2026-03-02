using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;
using System;
using System.Collections.Generic;

namespace PopUpUI.Elements.Spatial
{
    /// <summary>Provides a base class for <see cref="ISelectable"/> interface which have an optional visual and can capture selecting using <see cref="Spatial.SpatialArea"/>.</summary>
    public class SpatialSelectable : SpatialPopUpElement, ISelectable, IBackgroundSupport
    {
        private float opacity = 1;
        private SpatialArea area;
        private readonly AnimationDisplayInfo backgroundUIElement;

        public bool EnabledOutsideSelecting { get; set; }
        public MouseButtons ButtonsToSelect { get; set; }
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

        public Predicate<PopUpElement> DoNotSelectIf { get; set; }

        public event PopUpElementHandler Selected;
        public event PopUpElementHandler OutsideSelected;

        /// <summary>Initializes a new instance of <see cref="Selectable"/> class.</summary>
        public SpatialSelectable()
        {
            backgroundUIElement = new AnimationDisplayInfo();
        }

        public bool CanBeSelected(Point point)
        {
            return base.IsInside(point);
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

        void ISelectable.OnSelect(PopUpWindowEventArgs e)
        {
            Selected(this, e);
        }

        void ISelectable.OnSelectingOutside(PopUpWindowEventArgs e)
        {
            OutsideSelected(this, e);
        }
    }
}
