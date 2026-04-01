using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;

#nullable enable

namespace PopUpUI.Input
{
    public class KeyReceiverPacket
    {
        /// <summary>Raised when element got a key down input.</summary>
        public event PopUpElementHandler<Keys>? KeyDown;
        public event PopUpElementHandler<Keys>? KeyUp;

        public virtual void RaiseKeyDown(PopElement sender, PopUpWindowEventArgs e, Keys key)
        {
            KeyDown?.Invoke(sender, e, key);
        }

        public virtual void RaiseKeyUp(PopElement sender, PopUpWindowEventArgs e, Keys key)
        {
            KeyUp?.Invoke(sender, e, key);
        }
    }
}
