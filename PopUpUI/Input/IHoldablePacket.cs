using PopUpUI.Elements;

namespace PopUpUI.Input
{
    public interface IHoldablePacket
    {
        public MouseButtons ButtonsToHold { get; set; }
        /// <summary>Raised while element is held.</summary>
        public event PopUpElementHandler Held;
        /// <summary>Raised when hold status of element has changed, started or ended. Bool gets if its new status is held.</summary>
        public event PopUpElementHandler<bool> HoldStateChange;

        public void RaiseHeld(PopElement sender, PopUpWindowEventArgs e);
        public void RaiseHoldStateChange(PopElement sender, PopUpWindowEventArgs e, bool startedHolding);
    }
}
