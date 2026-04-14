using Microsoft.Xna.Framework;

namespace PopUpUI.Text.Rich
{
    /// <summary>Defines a mechanism for transforming and presenting various texts.</summary>
    public interface IProcesserHash
    {
        /// <summary>Gets a bool indicating this hash's texts will have <see cref="Symbol"/> wrapped around them.</summary>
        public bool WrapsText { get; }
        /// <summary>Gets the symbol indicating the hash.</summary>
        public string Symbol { get; }

        /// <summary>Gets the width of given string.</summary>
        /// <param name="str">The string to get width of.</param>
        /// <returns>Returns the width of given string.</returns>
        public float GetWidth(string str);
        /// <summary>Gets the number of visible characters inside given string.</summary>
        /// <param name="str">The stirng to get number of visible characters.</param>
        /// <returns>Returns the number of visible characters inside given string.</returns>
        public int VisibleCharLength(string str);
        /// <summary>Draws given <paramref name="text"/> at given <paramref name="position"/> and using <paramref name="opacity"/>.</summary>
        /// <param name="text">The text to render on target of <see cref="PopUpUI.Toolkit.DrawHelper"/>.</param>
        /// <param name="position">The position of text to draw.</param>
        /// <param name="opacity">The opacity will be applied to rendered text.</param>
        public void Draw(string text, Vector2 position, float opacity);
    }
}
