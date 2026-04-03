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

        public void OnSelect(PopUpWindowEventArgs e)
        {
            isChecked = !isChecked;
            OnCheckChange();

            SelectablePacket?.RaiseSelected(this, e);
        }

        public void OnSelectingOutside(PopUpWindowEventArgs e)
        {
            SelectablePacket?.RaiseOutsideSelected(this, e);
        }

        public bool CanBeSelected(Point point)
        {
            return base.InsideBoundaries(point);
        }
    }
}
