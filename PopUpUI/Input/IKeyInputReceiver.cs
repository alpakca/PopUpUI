using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using System;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for receiving key input from keyboard.</summary>
    public interface IKeyInputReceiver
    {
        /// <summary>Indicates if element can take input.</summary>
        public bool IsInKeyInputMode { get; }
        /// <summary>Defines a criteria for finishing input mode, so element won't get input any longer.</summary>
        public Predicate<PopUpElement> FinishKeyInputModeIf { get; set; }
        /// <summary>Raised when element got a key input.</summary>
        public event PopUpElementHandler<Keys> KeyInputReceived;
        /// <summary>Called when element got a key input.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        /// <param name="key">The key input.</param>
        public void OnKeyInputReceive(PopUpWindowEventArgs e, Keys key);
    }
}
