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

        public ScrollablePopWindow() : base()
        {
        }

        public ScrollablePopWindow(BatchDrawOptions? drawOptions) : base(drawOptions)
        {
        }

        public override Point TransformMousePoint(Point mousePoint)
        {
            return mousePoint - ScrolledOffset;
        }

        protected internal override void RenderUpdate()
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

        protected internal override void OnMouseVerticalScrollChange(PopWindowMouseEventArgs e, int change)
        {
            IMouseWheelReceiver? mouseWheelReceiver = Elements.FindWithType<IMouseWheelReceiver>(element => element.CanReceiveWheelChangeInput(e.MousePosition));

            if (mouseWheelReceiver != null)
            {
                mouseWheelReceiver.OnMouseVerticalScrollChangeReceive(e, change);
            }
            else if (EnableScrolling && ScrollsVertically)
            {
                ScrolledOffset += new Point(0, (int)(change / 6 * ScrollSpeed));
                ScrolledOffset = new Point(ScrolledOffset.X, Math.Min(ScrolledOffset.Y, 0));
            }
        }

        protected internal override void OnMouseHorizontalScrollChange(PopWindowMouseEventArgs e, int change)
        {
            IMouseWheelReceiver? mouseWheelReceiver = Elements.FindWithType<IMouseWheelReceiver>(element => element.CanReceiveWheelChangeInput(e.MousePosition));

            if (mouseWheelReceiver != null)
            {
                mouseWheelReceiver.OnMouseVerticalScrollChangeReceive(e, change);
            }
            else if (EnableScrolling)
            {
                ScrolledOffset += new Point((int)(change / 6 * ScrollSpeed), 0);
                ScrolledOffset = new Point(Math.Min(ScrolledOffset.X, 0), ScrolledOffset.Y);
            }
        }
    }
}
