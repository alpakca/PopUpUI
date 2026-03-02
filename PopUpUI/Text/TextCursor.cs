using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Threading.Tasks;

namespace PopUpUI.Text
{
    /// <summary>Represents a text cursor which supports cursor operations through <see cref="TextBlock"/> and can be customized.</summary>
    public class TextCursor : IDisposable
    {
        private int width = 2;
        private int leftAlign = 0;
        private int currentCharacterPosition;
        private int cooldownBetweenBlinks = 350;
        private Point cursorLocation;
        private Color color = Color.WhiteSmoke;
        private Texture2D cursorTexture;

        private readonly TextureDisplayInfo cursorUIElement;

        private int countdown;
        private bool isActive;
        private bool changesToApply = true;

        /// <summary>Gets or sets the width of the cursor. Default value is 2. To apply the changes, use <see cref="ApplyChanges"/> method.</summary>
        public int Width
        {
            get => width;
            set
            {
                width = value;
                changesToApply = true;
            }
        }

        /// <summary>Gets or sets the left-align of the cursor. Default value is 0. To apply the changes, use <see cref="ApplyChanges"/> method.</summary>
        public int LeftAlign
        {
            get => leftAlign;
            set
            {
                leftAlign = value;
                changesToApply = true;
            }
        }

        /// <summary>Gets or sets the time between blinks of the cursor, in milliseconds. Default value is 350.</summary>
        public int CooldownBetweenBlinks
        {
            get => cooldownBetweenBlinks;
            set
            {
                cooldownBetweenBlinks = value;
            }
        }

        /// <summary>Gets or sets the color of the cursor. Default value is <see cref="Color.WhiteSmoke"/>. To apply the changes, use <see cref="ApplyChanges"/> method.</summary>
        public Color Color
        {
            get => color;
            set
            {
                color = value;
                changesToApply = true;
            }
        }

        /// <summary>Gets the current position of the cursor. For instance, if value is 2; cursor is between indexes 1 and 2.</summary>
        public int CurrentCharacterPosition => currentCharacterPosition;
        /// <summary>Gets the current location of the cursor relative to texture of <see cref="TextBlock"/>.</summary>
        public Point CursorLocation => cursorLocation;

        /// <summary>Gets the <see cref="Text.TextBlock"/> this cursor belongs to.</summary>
        public TextBlock TextBlock { get; }

        /// <summary>Creates a new instance of <see cref="TextCursor"/> class.</summary>
        /// <param name="textBlock">The <see cref="Text.TextBlock"/> this cursor will belong to.</param>
        /// <param name="cursorUIElement">The <see cref="UIElement"/> cursor will update depending on its status. For instance, to change its opacity and location.</param>
        public TextCursor(TextBlock textBlock, TextureDisplayInfo cursorUIElement)
        {
            TextBlock = textBlock;
            this.cursorUIElement = cursorUIElement;

            CreateCursorTexture();
        }

        ~TextCursor()
        {
            Dispose();
        }

        public void Dispose()
        {
            cursorTexture?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>Sets the cursor to given character <paramref name="position"/>.</summary>
        /// <param name="position"></param>
        public void SetAtCharacterPosition(int position)
        {
            position = Math.Clamp(position, 0, TextBlock.Renderer.Text.Length);
            currentCharacterPosition = position;

            Point newCursorLocation = TextBlock.Renderer.GetCursorPosition(position, TextBlock.Size).ToPoint() + new Point(leftAlign, 0);

            countdown = cooldownBetweenBlinks;

            cursorUIElement.Location += newCursorLocation - cursorLocation;
            cursorUIElement.Opacity = 1;

            cursorLocation = newCursorLocation;
        }

        /// <summary>Sets the cursor depending on a point relative to texture of <see cref="TextBlock"/>.</summary>
        /// <param name="point">The point to put cursor on.</param>
        public void SetAtPoint(Point point)
        {
            SetAtCharacterPosition(TextBlock.Renderer.GetCursorPosition(point, TextBlock.Size));
        }

        /// <summary>Advances the character position of cursor by given <paramref name="count"/>.</summary>
        /// <param name="count">Amount of character position to advance.</param>
        public void AdvanceCharacterPositionBy(int count)
        {
            SetAtCharacterPosition(CurrentCharacterPosition + count);
        }

        /// <summary>Indicates if cursor is at the beginning of text of <see cref="TextBlock"/>.</summary>
        /// <returns></returns>
        public bool IsAtTheBeginning()
        {
            return currentCharacterPosition == 0;
        }

        /// <summary>Activates the cursor to start blinking, if not already.</summary>
        public void Activate()
        {
            if (isActive) return;

            cursorUIElement.Opacity = 1;

            isActive = true;
            countdown = 0;
            StartBlinkLoop();
        }

        /// <summary>Deactivates the cursor.</summary>
        public void Deactivate()
        {
            cursorUIElement.Opacity = 0;
            isActive = false;
        }

        /// <summary>Applies changes that made to cursor.</summary>
        public void ApplyChanges()
        {
            if (!changesToApply) return;

            changesToApply = false;
            CreateCursorTexture();
        }

        private async void StartBlinkLoop()
        {
            while (countdown-- > 0)
            {
                if (!isActive) return;

                await Task.Delay(1);
            }

            cursorUIElement.Opacity = cursorUIElement.Opacity != 1 ? 1 : 0;
            countdown = cooldownBetweenBlinks;

            StartBlinkLoop();
        }

        private void CreateCursorTexture()
        {
            cursorTexture?.Dispose();
            cursorTexture = DrawHelper.CreateTexture(width, TextBlock.Renderer.Font.LineSpacing);

            Color[] textureData = new Color[width * TextBlock.Renderer.Font.LineSpacing];
            Array.Fill(textureData, color);

            cursorTexture.SetData(textureData);

            cursorUIElement.Texture = cursorTexture;
            cursorUIElement.Size = new Point(width, TextBlock.Renderer.Font.LineSpacing);
        }
    }
}
