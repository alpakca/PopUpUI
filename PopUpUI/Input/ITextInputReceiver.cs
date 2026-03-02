using PopUpUI.Elements;
using System;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for receiving text -not key but character- input from keyboard.</summary>
    public interface ITextInputReceiver
    {
        /// <summary>Indicates if element can take input.</summary>
        public bool IsInTextInputMode { get; }
        /// <summary>Defines a criteria for finishing input mode, so element won't get input any longer.</summary>
        public Predicate<PopUpElement> FinishTextInputModeIf { get; set; }
        /// <summary>Raised when element got a text input.</summary>
        public event PopUpElementHandler<char> TextInputReceived;
        /// <summary>Called when element got a text input.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        /// <param name="ch">The text input.</param>
        public void OnTextInputReceive(PopUpWindowEventArgs e, char ch);
    }
}
