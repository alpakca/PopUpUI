using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Elements
{
    public abstract class PopElement
    {
        protected float _opacity = 1f;
        protected Rectangle _boundaries;

        public string Name { get; set; }
        public Visibility Visibility { get; set; }

        public virtual float Opacity 
        {
            get => _opacity;
            set => _opacity = value; 
        }

        public virtual Rectangle Boundaries 
        {
            get => _boundaries;
            set => _boundaries = value;
        }

        /// <summary>An update method for casual checking, CPU-based.</summary>
        public virtual void Update() { }
        /// <summary>A specialized method for pre-buffer render; rende roperations. Called in draw-thread, GPU-based.</summary>
        public virtual void RenderUpdate() { }
        public virtual IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return Enumerable.Empty<UIDrawInfo>();
        }
    }
}
