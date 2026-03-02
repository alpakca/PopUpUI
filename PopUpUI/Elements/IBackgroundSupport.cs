using System;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Elements
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for generalizing background support for those elements who support background.</summary>
    public interface IBackgroundSupport
    {
        /// <summary>Gets a handler that changes background of given element.</summary>
        public static Action<PopUpElement, AnimationState> BackgroundChanger { get; } = 
            (element, newBackground) => 
            {
                if (element is IBackgroundSupport support) support.BackgroundAnimation = newBackground; 
            };

        /// <summary>Gets or sets the background of the element.</summary>
        public AnimationState BackgroundAnimation { get; set; }
    }
}
