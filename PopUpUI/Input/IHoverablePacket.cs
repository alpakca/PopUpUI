using PopUpUI.Elements;
using System;

namespace PopUpUI.Input
{
    public interface IHoverablePacket
    {
        /// <summary>Gets the current hover status of element.</summary>
        public bool IsHovered { get; }
        /// <summary>Raised while element is hovered.</summary>
        public event PopUpElementHandler Hovering;
        /// <summary>Raised when hover status of element has changed, started or ended. Bool gets if its new status is hovered.</summary>
        public event PopUpElementHandler<bool> HoverStateChange;

        public void RaiseHovering(PopUpElement sender, PopUpWindowEventArgs e);
        public void RaiseHoverStateChange(PopUpElement sender, PopUpWindowEventArgs e, bool isHovered);
    }
}
