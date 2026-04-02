using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace PopUpUI.Elements
{
    internal class CheckBox : PopElement, ISelectable
    {
        protected bool isChecked;

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
            return _boundaries.Contains(point);
        }
    }
}
