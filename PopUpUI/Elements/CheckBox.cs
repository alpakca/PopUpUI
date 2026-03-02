using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;
using System.Collections.Generic;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Elements
{
    /// <summary>Presents a checkbox, that is a button that can change to checked and unchecked as state.</summary>
    public class Checkbox : PopUpElement, ISelectable, IBackgroundSupport
    {
        private bool isChecked;
        private float opacity = 1;
        private Rectangle area;
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
        public override Rectangle Area
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
        /// <summary>Raised when status of checkbox has changed.</summary>
        public event Action<Checkbox, bool> CheckChange;

        /// <summary>Initializes a new instance of <see cref="Checkbox"/> class.</summary>
        public Checkbox()
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
