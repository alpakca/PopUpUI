using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    public class CheckBox : PopElement, ISelectable
    {
        private bool isChecked;

        public bool IsChecked => isChecked;
        public SelectablePacket SelectablePacket { get; set; }

        public event Action<CheckBox> CheckChange;

        protected virtual void OnCheckChange()
        {
            CheckChange?.Invoke(this);
        }

        public void OnSelect(Input.PopWindowMouseEventArgs e)
        {
            SwitchStatus();

            SelectablePacket?.RaiseSelected(this, e);
        }

        public void OnSelectingOutside(Input.PopWindowMouseEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        public bool CanBeSelected(Point point)
        {
            return base.InsideBoundaries(point);
        }

        public void SwitchStatus()
        {
            isChecked = !isChecked;
            OnCheckChange();
        }
    }
}
