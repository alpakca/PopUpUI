using Microsoft.Xna.Framework;

namespace PopUpUI.Input
{
    internal interface IFileReceiver
    {
        public IFileReceiverPacket FilePacket { get; }

        public void OnFileDrop(PopUpWindowEventArgs e, FileDropEventArgs fileArgs);
    }
}
