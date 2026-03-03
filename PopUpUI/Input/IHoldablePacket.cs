using PopUpUI.Elements;
using PopUpUI.Input;
using System;

namespace PopUpUI.Input
{
    public interface IHoldablePacket
    {
        /// <summary>Gets the current hold status of element.</summary>
        public bool IsHeld { get; }
        public MouseButtons ButtonsToHold { get; set; }
        /// <summary>Raised while element is held.</summary>
        public event PopUpElementHandler Held;
        /// <summary>Raised when hold status of element has changed, started or ended. Bool gets if its new status is held.</summary>
        public event PopUpElementHandler<bool> HoldStateChange;

        public void RaiseHeld(PopUpElement sender, PopUpWindowEventArgs e);
        public void RaiseHoldStateChange(PopUpElement sender, PopUpWindowEventArgs e, bool isHeld);
    }
}
