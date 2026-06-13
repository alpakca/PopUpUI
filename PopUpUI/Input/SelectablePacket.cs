using PopUpUI.Elements;

#nullable enable

namespace PopUpUI.Input
{
    public class SelectablePacket
    {
        /// <summary>Gets or sets a preference for outside selecting if <see cref="CanBeSelected(Point)"/> is false. Default value is false.</summary>
        public bool EnabledOutsideSelecting { get; set; } = false;
        /// <summary>Gets or sets the mouse buttons that will trigger selecting events. <see cref="MouseButtons"/> is an enumeration with flags.</summary>
        public MouseButtons ButtonsToSelect { get; set; } = MouseButtons.Left;
        /// <summary>Raised when element is selected.</summary>
        public event PopUpElementHandler? Selected;
        /// <summary>Raised when <see cref="CanBeSelected(Point)"/> is false, also outside selecting is enabled.</summary>
        public event PopUpElementHandler? OutsideSelected;

        public virtual void RaiseSelected(PopElement sender, PopWindowMouseEventArgs e)
        {
            Selected?.Invoke(sender, e);
        }

        public virtual void RaiseOutsideSelected(PopElement sender, PopWindowMouseEventArgs e)
        {
            OutsideSelected?.Invoke(sender, e);
        }
    }
}
