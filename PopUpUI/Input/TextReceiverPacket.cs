using PopUpUI.Elements;

#nullable enable

namespace PopUpUI.Input
{
    public class TextReceiverPacket
    {
        /// <summary>Raised when element got a text input.</summary>
        public event PopUpElementHandler<char>? TextReceived;

        public virtual void RaiseTextReceived(PopElement sender, PopWindowMouseEventArgs e, char ch)
        {
            TextReceived?.Invoke(sender, e, ch);
        }
    }
}
