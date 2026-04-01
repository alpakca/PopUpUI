using Microsoft.Xna.Framework;

namespace PopUpUI.Input
{
    internal interface IFileReceiver
    {
        public FileReceiverPacket FilePacket { get; }

        public bool CanReceiveFileDrop(Point mousePosition);
        public void OnFileDrop(PopUpWindowEventArgs e, FileDropEventArgs fileArgs);
    }
}
