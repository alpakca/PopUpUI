using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using System;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for achieving selecting operations.</summary>
    public interface ISelectable
    {
        /// <summary>Gets or sets a preference for outside selecting if <see cref="CanBeSelected(Point)"/> is false. Default value is false.</summary>
        public bool EnabledOutsideSelecting { get; set; }
        /// <summary>Gets or sets the mouse buttons that will trigger selecting events. <see cref="MouseButtons"/> is an enumeration with flags.</summary>
        public MouseButtons ButtonsToSelect { get; set; }
        /// <summary>Defines a criteria for selecting based on element itself.</summary>
        public Predicate<PopUpElement> DoNotSelectIf { get; set; }
        /// <summary>Raised when element is selected.</summary>
        public event PopUpElementHandler Selected;
        /// <summary>Raised when <see cref="CanBeSelected(Point)"/> is false, also outside selecting is enabled.</summary>
        public event PopUpElementHandler OutsideSelected;
        /// <summary>Checks if element can be selected based on position of mouse.</summary>
        /// <param name="point">The mouse position inside <see cref="PopUpWindow"/> element is in.</param>
        /// <returns>Returns <see langword="true"/> if element can be selected, else <see langword="false"/>.</returns>
        public bool CanBeSelected(Point point);
        /// <summary>Called when element is selected.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        public void OnSelect(PopUpWindowEventArgs e);
        /// <summary>Called when <see cref="CanBeSelected(Point)"/> is false, also outside selecting is enabled.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        public void OnSelectingOutside(PopUpWindowEventArgs e);
    }
}
