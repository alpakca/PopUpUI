using PopUpUI.Elements;

namespace PopUpUI.Input
{
    public interface IHoverablePacket
    {
        /// <summary>Raised while element is hovered.</summary>
        public event PopUpElementHandler Hovering;
        /// <summary>Raised when hover status of element has changed, started or ended. Bool gets if its new status is hovered.</summary>
        public event PopUpElementHandler<bool> HoverStateChange;

        public void RaiseHovering(PopElement sender, PopUpWindowEventArgs e);
        public void RaiseHoverStateChange(PopElement sender, PopUpWindowEventArgs e, bool isHovered);
    }
}
