using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    /// <summary>Represents a piece of text that has its own drawing and parsing behaviour.</summary>
    public interface ITextPiece
    {
        /// <summary>Gets the number of visible characters this piece contains.</summary>
        public int VisibleCharLength { get; }

        /// <summary>Gets the width of the piece.</summary>
        /// <returns>The width of the piece.</returns>
        public float TextWidth();
        /// <summary>Gets the width of piece starting from given index.</summary>
        /// <param name="startIndex">Index the width measurement will start.</param>
        /// <returns>The width of the piece starting from <paramref name="startIndex"/>.</returns>
        public float TextWidth(int startIndex);
        /// <summary>Gets the width of piece starting from given index and have given length.</summary>
        /// <param name="startIndex">Index the width measurement will start.</param>
        /// <param name="length">Length of piece to measure.</param>
        /// <returns>The width of the piece starting from <paramref name="startIndex"/> and have given <paramref name="length"/>.</returns>
        public float TextWidth(int startIndex, int length);
        /// <summary>Draws the text piece with given <paramref name="position"/>, <paramref name="color"/> and <paramref name="opacity"/>.</summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="opacity"></param>
        public void Draw(Vector2 position, Color color, float opacity);
    }
}
