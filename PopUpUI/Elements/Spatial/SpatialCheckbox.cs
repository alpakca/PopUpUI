using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;
using System.Collections.Generic;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Elements.Spatial
{
    /// <summary>Presents a checkbox with <see cref="Spatial.SpatialArea"/>, that is a button that can change to checked and unchecked as state.</summary>
    public class SpatialCheckbox : SpatialPopUpElement, ISelectable, IBackgroundSupport
    {
        private bool isChecked;
        private float opacity = 1f;
        private SpatialArea area = SpatialArea.Empty;
        private readonly AnimationDisplayInfo backgroundUIElement;

        /// <summary>Indicates the current state of checkbox.</summary>
        public bool IsChecked => isChecked;
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
                backgroundUIElement.Boundaries = value.Bounds;
                area = value;
            }
        }

        public Predicate<PopUpElement> DoNotSelectIf { get; set; }
        public AnimationState BackgroundAnimation
        {
            get => backgroundUIElement.Animation;
            set => backgroundUIElement.Animation = value;
        }

        public event PopUpElementHandler Selected;
        public event PopUpElementHandler OutsideSelected;
        /// <summary>Raised when status of checkbox has changed.</summary>
        public event Action<SpatialCheckbox, bool> CheckChange;

        /// <summary>Initializes a new instance of <see cref="SpatialCheckbox"/> class.</summary>
        public SpatialCheckbox()
        {
            backgroundUIElement = new AnimationDisplayInfo();
        }

        /// <summary>Sets the check state of the checkbox.</summary>
        /// <param name="isChecked">New state of the checkbox.</param>
        public void SetCheckState(bool isChecked)
        {
            this.isChecked = isChecked;

            CheckChange?.Invoke(this, isChecked);
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

        public bool CanBeSelected(Point point)
        {
            return base.IsInside(point);
        }

        void ISelectable.OnSelect(PopUpWindowEventArgs e)
        {
            bool newValue = !isChecked;
            SetCheckState(newValue);

            Selected(this, e);
        }

        void ISelectable.OnSelectingOutside(PopUpWindowEventArgs e)
        {
            OutsideSelected(this, e);
        }
    }
}
