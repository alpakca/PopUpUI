using Microsoft.Xna.Framework;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for achieving selecting operations.</summary>
    public interface ISelectable
    {
        public SelectablePacket SelectablePacket { get; }

        /// <summary>Checks if element can be selected based on position of mouse.</summary>
        /// <param name="point">The mouse position inside <see cref="PopUpWindow"/> element is in.</param>
        /// <returns>Returns <see langword="true"/> if element can be selected, else <see langword="false"/>.</returns>
        public bool CanBeSelected(Point point);
        /// <summary>Called when element is selected.</summary>
        /// <param name="e">A <see cref="PopWindowMouseEventArgs"/> object that contains event data.</param>
        public void OnSelect(PopWindowMouseEventArgs e);
        /// <summary>Called when <see cref="CanBeSelected(Point)"/> is false, also outside selecting is enabled.</summary>
        /// <param name="e">A <see cref="PopWindowMouseEventArgs"/> object that contains event data.</param>
        public void OnSelectingOutside(PopWindowMouseEventArgs e);
    }
}
