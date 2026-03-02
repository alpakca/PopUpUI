namespace PopUpUI.Text
{
    /// <summary>Orientation options of <see cref="TextRenderer"/>.</summary>
    public enum TextOrientation
    {
        /// <summary>Text is left aligned, with a one space in between words.</summary>
        Left,
        /// <summary>Text is middle aligned, it have equal spaces between start and end of the word rows, with words have one space between words.</summary>
        Middle,
        /// <summary>Text is right aligned, with a one space between words.</summary>
        Right,
        /// <summary>Text doesn't have space at the start or end of the line, instead extra spaces are in between words.</summary>
        FixedSize
    }
}
