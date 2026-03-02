using Microsoft.Xna.Framework;

namespace PopUpUI.Elements.Spatial
{
    /// <summary>Provides as a base class for <see cref="Surface.PopUpWindow"/> elements that does not have a rectangular area. This is an abstract class.</summary>
    public abstract class SpatialPopUpElement : PopUpElement
    {
        public override Rectangle Area 
        {
            get => SpatialArea.Bounds; 
            set => SpatialArea = value; 
        }

        /// <summary>
        /// Gets or sets a spatial area, that can be non-rectangular and uses <see cref="SpatialArea.Bounds"/> property 
        /// to affects boundaries of <see cref="Surface.UIElement"/>s this element contains.
        /// </summary>
        public abstract SpatialArea SpatialArea { get; set; }

        public override bool IsInside(Point point)
        {
            return SpatialArea.Contains(point);
        }
    }
}
