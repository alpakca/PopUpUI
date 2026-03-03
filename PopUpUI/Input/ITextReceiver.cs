using PopUpUI.Elements;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for receiving text -not key but character- input from keyboard.</summary>
    public interface ITextReceiver
    {
        public ITextReceiverPacket TextReceiverPacket { get; }

        /// <summary>Called when element got a text input.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        /// <param name="ch">The text input.</param>
        public void OnTextInputReceive(PopUpWindowEventArgs e, char ch);
    }
}
