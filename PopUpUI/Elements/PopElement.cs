using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Elements
{
    public abstract class PopElement
    {
        public string Name { get; set; } = string.Empty;
        public Visibility Visibility { get; set; } = Visibility.Visible;

        public virtual float Opacity { get; set; } = 1f;
        public virtual Rectangle Boundaries { get; set; }

        /// <summary>An update method for casual checking, CPU-based.</summary>
        public virtual void Update() { }
        /// <summary>A specialized method for pre-buffer render; render operations. Called in draw-thread, GPU-based.</summary>
        public virtual void RenderUpdate() { }
        public virtual IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return Enumerable.Empty<UIDrawInfo>();
        }

        public bool InsideBoundaries(Point point)
        {
            return Boundaries.Contains(point);
        }
    }
}
