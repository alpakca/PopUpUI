using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;

namespace PopUpUI.Input
{
    public interface IKeyReceiverPacket
    {
        /// <summary>Raised when element got a key down input.</summary>
        public event PopUpElementHandler<Keys> KeyDown;
        public event PopUpElementHandler<Keys> KeyUp;

        public void RaiseKeyDown(PopElement sender, PopUpWindowEventArgs e, Keys key); 
        public void RaiseKeyUp(PopElement sender, PopUpWindowEventArgs e, Keys key);
    }
}
