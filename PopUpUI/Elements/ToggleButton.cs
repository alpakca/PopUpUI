using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class ToggleButton : Clickable
    {
        private bool isChecked;

        public bool IsChecked => isChecked;
        public ICommander<ToggleButton>? ToggleCommand { get; set; }

        public void Toggle()
        {
            isChecked = !isChecked;
            OnToggle();
        }

        public virtual void SetStatusWithoutCommander(bool setToChecked)
        {
            isChecked = setToChecked;
        }

        protected virtual void OnToggle()
        {
            ToggleCommand?.Execute(this);
        }

        protected override void OnClick()
        {
            Toggle();
            base.OnClick();
        }
    }
}
