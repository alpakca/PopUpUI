using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class ToggleButton : Clickable
    {
        private bool isChecked;

        public bool IsChecked => isChecked;

        public event Action<ToggleButton>? Toggled;

        public void Toggle()
        {
            isChecked = !isChecked;
            OnToggle();
        }

        protected virtual void OnToggle()
        {
            Toggled?.Invoke(this);
        }

        protected override void OnClick()
        {
            Toggle();
            base.OnClick();
        }
    }
}
