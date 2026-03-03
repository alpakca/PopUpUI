using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using System;

namespace PopUpUI.Input
{
    public interface IKeyReceiverPacket
    {
        /// <summary>Indicates if element can take input.</summary>
        public bool ReceivesKeyInput { get; }
        /// <summary>Raised when element got a key down input.</summary>
        public event PopUpElementHandler<Keys> KeyDown;
        public event PopUpElementHandler<Keys> KeyUp;

        public void RaiseKeyDown(PopUpElement sender, PopUpWindowEventArgs e, Keys key); 
        public void RaiseKeyUp(PopUpElement sender, PopUpWindowEventArgs e, Keys key);
    }
}
