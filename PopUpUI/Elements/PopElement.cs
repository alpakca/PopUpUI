using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Elements
{
    public abstract class PopElement
    {
        public string Name { get; set; }
        public Visibility Visibility { get; set; }

        public virtual float Opacity { get; set; } = 1f;
        public virtual Rectangle Boundaries { get; set; }

        public virtual void FrameUpdate() { }
        public virtual void RenderUpdate() { }
        public virtual IEnumerable<IUIDrawInfo> GetDrawInfos()
        {
            return Enumerable.Empty<IUIDrawInfo>();
        }
    }
}
