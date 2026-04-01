using PopUpUI.Elements;

#nullable enable

namespace PopUpUI.Input
{
    public class HoverablePacket
    {
        /// <summary>Raised while element is hovered.</summary>
        public event PopUpElementHandler? Hovering;
        /// <summary>Raised when hover status of element has changed, started or ended. Bool gets if its new status is hovered.</summary>
        public event PopUpElementHandler<bool>? HoverStateChange;

        public virtual void RaiseHovering(PopElement sender, PopUpWindowEventArgs e)
        {
            Hovering?.Invoke(sender, e);
        }

        public virtual void RaiseHoverStateChange(PopElement sender, PopUpWindowEventArgs e, bool startedHovering)
        {
            HoverStateChange?.Invoke(sender, e, startedHovering);
        }
    }
}
