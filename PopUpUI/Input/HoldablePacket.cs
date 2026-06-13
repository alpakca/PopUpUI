using PopUpUI.Elements;

#nullable enable

namespace PopUpUI.Input
{
    public class HoldablePacket
    {
        public MouseButtons ButtonsToHold { get; set; } = MouseButtons.Left;
        /// <summary>Raised while element is held.</summary>
        public event PopUpElementHandler? Held;
        /// <summary>Raised when hold status of element has changed, started or ended. Bool gets if its new status is held.</summary>
        public event PopUpElementHandler<bool>? HoldStateChange;

        public virtual void RaiseHeld(PopElement sender, PopWindowMouseEventArgs e)
        {
            Held?.Invoke(sender, e);
        }

        public virtual void RaiseHoldStateChange(PopElement sender, PopWindowMouseEventArgs e, bool startedHolding)
        {
            HoldStateChange?.Invoke(sender, e, startedHolding);
        }
    }
}
