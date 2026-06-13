using PopUpUI.Elements;

#nullable enable

namespace PopUpUI.Input
{
    public class MouseWheelReceiverPacket
    {
        /// <summary>Raised when mouse wheel value has changed.</summary>
        public event PopUpElementHandler? MouseWheelChanged;

        public virtual void RaiseMouseWheelChanged(PopElement sender, PopWindowMouseEventArgs e)
        {
            MouseWheelChanged?.Invoke(sender, e);
        }
    }
}
