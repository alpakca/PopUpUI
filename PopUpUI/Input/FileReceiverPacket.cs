using Microsoft.Xna.Framework;
using PopUpUI.Elements;

#nullable enable

namespace PopUpUI.Input
{
    public class FileReceiverPacket
    {
        public event PopUpElementHandler<FileDropEventArgs>? FileDropped;

        public virtual void RaiseFileDropped(PopElement sender, PopUpWindowEventArgs e, FileDropEventArgs fileArgs)
        {
            FileDropped?.Invoke(sender, e, fileArgs);
        }
    }
}
