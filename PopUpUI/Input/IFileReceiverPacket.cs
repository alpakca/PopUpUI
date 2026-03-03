using Microsoft.Xna.Framework;
using PopUpUI.Elements;

namespace PopUpUI.Input
{
    internal interface IFileReceiverPacket
    {
        public bool ReceivesFileDrop { get; }

        public event PopUpElementHandler<FileDropEventArgs> FileDropped;

        public void RaiseFileDropped(PopElement sender, PopUpWindowEventArgs e, FileDropEventArgs fileArgs);
    }
}
