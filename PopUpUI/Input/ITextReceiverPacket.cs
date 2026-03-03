using PopUpUI.Elements;
using PopUpUI.Input;
using System;

namespace PopUpUI.Input
{
    public interface ITextReceiverPacket
    {
        /// <summary>Indicates if element can take input.</summary>
        public bool ReceivesTextInput { get; }
        /// <summary>Raised when element got a text input.</summary>
        public event PopUpElementHandler<char> TextReceived;

        public void RaiseTextReceived(PopUpElement sender, PopUpWindowEventArgs e, char ch);
    }
}
