

namespace PopUpUI.Text
{
    public interface ITextPiece
    {
        public int VisibleCharLength { get; }

        public float TextWidth();
        public float TextWidth(int startIndex);
        public float TextWidth(int startIndex, int length);
    }
}
