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

        public void SetCheckStatus(bool setToChecked)
        {
            if (setToChecked != isChecked)
            {
                Toggle();
            }
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
