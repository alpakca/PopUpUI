using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for receiving key input from keyboard.</summary>
    public interface IKeyReceiver
    {
        /// <summary>Indicates if element can take input.</summary>
        public bool ReceivesKeyInput { get; }
        public IKeyReceiverPacket KeyReceiverPacket { get; }

        /// <summary>Called when element got a key input.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        /// <param name="key">The key input.</param>
        public void OnKeyDown(PopUpWindowEventArgs e, Keys key);
        public void OnKeyUp(PopUpWindowEventArgs e, Keys key);
    }
}
