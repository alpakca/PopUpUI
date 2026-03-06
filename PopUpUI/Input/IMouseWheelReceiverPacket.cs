using PopUpUI.Elements;

namespace PopUpUI.Input
{
    public interface IMouseWheelReceiverPacket
    {
        /// <summary>Raised when mouse wheel value has changed.</summary>
        public event PopUpElementHandler MouseWheelChanged;

        public void RaiseMouseWheelChanged(PopElement sender, PopUpWindowEventArgs e);
    }
}
