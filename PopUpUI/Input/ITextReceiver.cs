namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for receiving text -not key but character- input from keyboard.</summary>
    public interface ITextReceiver
    {
        /// <summary>Indicates if element can take input.</summary>
        public bool ReceivesTextInput { get; }
        public TextReceiverPacket TextReceiverPacket { get; }

        /// <summary>Called when element got a text input.</summary>
        /// <param name="e">A <see cref="PopWindowMouseEventArgs"/> object that contains event data.</param>
        /// <param name="ch">The text input.</param>
        public void OnTextInputReceive(PopWindowMouseEventArgs e, char ch);
    }
}
