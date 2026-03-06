using PopUpUI.Elements;

namespace PopUpUI.Input
{
    public interface ITextReceiverPacket
    {
        /// <summary>Raised when element got a text input.</summary>
        public event PopUpElementHandler<char> TextReceived;

        public void RaiseTextReceived(PopElement sender, PopUpWindowEventArgs e, char ch);
    }
}
