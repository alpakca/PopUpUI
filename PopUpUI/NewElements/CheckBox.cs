using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using System;

namespace PopUpUI.NewElements
{
    internal class CheckBox : PopElement, ISelectable
    {
        protected bool isChecked;

        public bool IsChecked => isChecked;
        public ISelectablePacket SelectablePacket { get; }

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
