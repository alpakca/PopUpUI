using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Toolkit;
using System;

#nullable enable

namespace PopUpUI.Surface
{
    public class ScrollablePopWindow : PopWindow
    {
        public bool EnableScrolling { get; set; } = false;
        public bool ScrollsVertically { get; set; } = true;
        public float ScrollSpeed { get; set; } = 1f;
        public Point ScrolledOffset { get; set; }

        public ScrollablePopWindow(Point size) : base(size)
        {
        }

        public ScrollablePopWindow(Point size, BatchDrawOptions? drawOptions) : base(size, drawOptions)
        {
        }

        public override Point TransformMousePoint(Point mousePoint)
        {
            return mousePoint - ScrolledOffset;
        }

        internal override void RenderUpdate()
        {
            foreach (var element in Elements)
            {
                element.RenderUpdate();
            }

            DrawHelper.SetBeginBatch(WindowTexture, DrawOptions);

            foreach (var element in Elements)
            {
                if (element.Visibility != Visibility.Visible)
                {
                    continue;
                }

                foreach (var drawInfo in element.GetDrawInfos())
                {
                    Rectangle destination = new Rectangle(ScrolledOffset + drawInfo.Location, drawInfo.Size);

                    // only render the texture if it can be seen
                    if (IsDestinationVisible(destination))
                    {
                        DrawHelper.Batch.Draw(drawInfo.GetTexture(), destination, drawInfo.Opacity * Color.White);

                    }
                }
            }

            DrawHelper.EndBatch();
        }

        internal override void OnMouseWheelChange(PopUpWindowEventArgs e)
        {
            IMouseWheelReceiver? mouseWheelReceiver = Elements.FindWithType<IMouseWheelReceiver>(element => element.CanReceiveWheelChangeInput(e.MousePoint));

            if (mouseWheelReceiver != null)
            {
                mouseWheelReceiver.OnMouseWheelChangeReceive(e);
            }
            else if (EnableScrolling)
            {
                ScrolledOffset += ScrollsVertically ? new Point(0, (int)(e.MouseWheelChange / 6 * ScrollSpeed))
                                                    : new Point((int)(e.MouseWheelChange / 6 * ScrollSpeed), 0);

                ScrolledOffset = new Point(Math.Min(ScrolledOffset.X, 0), Math.Min(ScrolledOffset.Y, 0));
            }
        }
    }
}
