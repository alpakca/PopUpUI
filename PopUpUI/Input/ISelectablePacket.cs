using PopUpUI.Elements;

namespace PopUpUI.Input
{
    public interface ISelectablePacket
    {
        /// <summary>Gets or sets a preference for outside selecting if <see cref="CanBeSelected(Point)"/> is false. Default value is false.</summary>
        public bool EnabledOutsideSelecting { get; set; }
        /// <summary>Gets or sets the mouse buttons that will trigger selecting events. <see cref="MouseButtons"/> is an enumeration with flags.</summary>
        public MouseButtons ButtonsToSelect { get; set; }
        /// <summary>Raised when element is selected.</summary>
        public event PopUpElementHandler Selected;
        /// <summary>Raised when <see cref="CanBeSelected(Point)"/> is false, also outside selecting is enabled.</summary>
        public event PopUpElementHandler OutsideSelected;

        public void RaiseSelected(PopElement sender, PopUpWindowEventArgs e);
        public void RaiseOutsideSelected(PopElement sender, PopUpWindowEventArgs e);
    }
}
