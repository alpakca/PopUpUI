using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Input;
using PopUpUI.Text;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;
using System.Linq;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Elements
{
    /// <summary>Presents an object that can receive key inputs from keyboard.</summary>
    public class Inputable : PopUpElement, ISelectable, IKeyInputReceiver, ITextInputReceiver, IBackgroundSupport
    {
        private bool isInInputMode;
        private bool autoActivateCursor = true;
        private float opacity = 1;
        private Point textOffset;
        private Rectangle area;

        private readonly AnimationDisplayInfo backgroundUIElement;
        private readonly TextureDisplayInfo textBlockUIElement;
        private readonly TextureDisplayInfo cursorUIElement;

        bool IKeyInputReceiver.IsInKeyInputMode => isInInputMode;
        bool ITextInputReceiver.IsInTextInputMode => isInInputMode;

        /// <summary>Gets the types of key inputs from keyboard element is accepting. This property uses flags.</summary>
        public InputType InputType { get; }
        /// <summary>Gets <see cref="char"/>s that can be inputted regardless of <see cref="InputType"/>.</summary>
        public IEnumerable<char> EnabledCharsForInput { get; }
        public IEnumerable<char> RestrictedCharsForInput { get; }
        /// <summary>Gets the <see cref="Text.TextBlock"/> this element contains.</summary>
        public TextBlock TextBlock { get; }
        /// <summary>Gets the <see cref="TextCursor"/> this element contains. </summary>
        public TextCursor Cursor { get; }

        /// <summary>Gets or sets a preference for automaticaly switching to default mode. Default value is <see langword="true"/>.</summary>
        /// <remarks>Default mode appears when input mode is set to false with inputable having no text in it. It shows <see cref="DefaultContent"/> with 0.75f opacity.</remarks>
        public bool EnableSwitchingToDefaultMode { get; set; } = true;
        /// <summary>Gets or sets a preference for letter inputs to be case-sensitive with caps lock. Default value is true.</summary>
        public bool IsCaseSensitive { get; set; } = true;
        /// <summary>Gets or sets a preference for letter inputs to use upper case if <see cref="IsCaseSensitive"/> is false. Default value is true.</summary>
        public bool UseUpperCaseForCaseInsensitive { get; set; } = true;
        public bool EnabledOutsideSelecting { get; set; } = true;
        public MouseButtons ButtonsToSelect { get; set; }

        /// <summary>Indicates whether inputable is currently taking inputs.</summary>
        public bool InInputMode => isInInputMode;

        /// <summary>Indicates the desire to automatically activate <see cref="TextCursor"/> while inputable can receive inputs.</summary>
        public bool AutoActivateCursor
        {
            get => autoActivateCursor;
            set
            {
                if (isInInputMode)
                {
                    if (value == true) Cursor.Activate();
                    else Cursor.Deactivate();
                }

                autoActivateCursor = value;
            }
        }

        public override float Opacity
        {
            get => opacity;
            set
            {
                backgroundUIElement.Opacity = value;
                textBlockUIElement.Opacity = value;

                opacity = value;
            }
        }
        public override Rectangle Area
        {
            get => area;
            set
            {
                backgroundUIElement.Location = value.Location;
                backgroundUIElement.Size = value.Size;

                textBlockUIElement.Location = value.Location;
               
                cursorUIElement.Location += value.Location - area.Location;

                area = value;
            }
        }

        public Point TextOffset
        {
            get => textOffset;
            set
            {
                textBlockUIElement.Location -= textOffset;
                textBlockUIElement.Location += value;

                cursorUIElement.Location -= textOffset;
                cursorUIElement.Location += value;

                textOffset = value;
            }
        }

        public AnimationState BackgroundAnimation
        {
            get => backgroundUIElement.Animation;
            set => backgroundUIElement.Animation = value;
        }

        public Predicate<PopUpElement> DoNotSelectIf { get; set; }
        public Predicate<PopUpElement> FinishKeyInputModeIf { get; set; }
        public Predicate<PopUpElement> FinishTextInputModeIf { get; set; }

        public event PopUpElementHandler Selected;
        public event PopUpElementHandler OutsideSelected;
        public event PopUpElementHandler<Keys> KeyInputReceived;
        /// <summary>Raised only if inputted text was eligable with <see cref="InputType"/>.</summary>
        public event PopUpElementHandler<char> TextInputReceived;

        /// <summary>Raised when the input of the inputable has changed, a char was added or removed.</summary>
        public event Action<Inputable, string> InputChanged = delegate { };
        /// <summary>Raised when the mode of the element has changed. Bool indicates if element has entered input mode.</summary>
        public event Action<Inputable, bool> InputModeChanged = delegate { };

        /// <summary>Initializes a new instance of <see cref="Inputable"/> class witch given TextBlock instance, input type and enabled chars for input.</summary>
        /// <param name="textBlock">A <see cref="Text.TextBlock"/> used for displaying input.</param>
        /// <param name="inputType">Input type this inputable will be accepting.</param>
        /// <param name="enabledCharsForInput">Chars that are enabled to input regardless of type of <paramref name="inputType"/>.</param>
        public Inputable(TextBlock textBlock, InputType inputType, IEnumerable<char> enabledCharsForInput, IEnumerable<char> restrictedCharsForInput)
        {
            ArgumentNullException.ThrowIfNull(textBlock);

            backgroundUIElement = new AnimationDisplayInfo();
            textBlockUIElement = new TextureDisplayInfo { Texture = textBlock.GetTextTexture() };
            cursorUIElement = new TextureDisplayInfo { Opacity = 0 };

            InputType = inputType;
            EnabledCharsForInput = enabledCharsForInput;
            RestrictedCharsForInput = restrictedCharsForInput;
            TextBlock = textBlock;

            Cursor = new TextCursor(textBlock, cursorUIElement);
            Cursor.Deactivate();
        }

        /// <summary>Initializes a new instance of <see cref="Inputable"/> class witch given <see cref="TextBlock"/> instance and input type.</summary>
        /// <param name="text">A <see cref="TextBlock"/> used for displaying input.</param>
        /// <param name="inputType">Input type this inputable will be accepting.</param>
        public Inputable(TextBlock text, InputType inputType) : this(text, inputType, [], [])
        {
        }

        /// <summary>Initializes a new instance of <see cref="Inputable"/> class witch given <see cref="TextBlock"/> instance and enabled chars for input.</summary>
        /// <param name="text">A <see cref="TextBlock"/> used for displaying input.</param>
        /// <param name="enabledCharsForInput">Chars that are enabled to input.</param>
        public Inputable(TextBlock text, IEnumerable<char> enabledCharsForInput) : this(text, InputType.None, enabledCharsForInput, [])
        { 
        }

        public Inputable(TextBlock text, IEnumerable<char> enabledCharsForInput, IEnumerable<char> restrictedCharsForInput)
            : this(text, InputType.None, enabledCharsForInput, restrictedCharsForInput)
        {
        }

        public override void UpdateElements()
        {
            if (TextBlock.HasChanged()) textBlockUIElement.Texture = TextBlock.GetTextTexture();
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            if (BackgroundAnimation != null) yield return backgroundUIElement;
            yield return textBlockUIElement;

            if (AutoActivateCursor) yield return cursorUIElement;
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            if (willPlay) backgroundUIElement.Animation?.Play();
            else backgroundUIElement.Animation?.Stop();
        }

        public bool CanBeSelected(Point point)
        {
            return base.IsInside(point);
        }

        void ISelectable.OnSelect(PopUpWindowEventArgs e)
        {
            ChangeInputMode(true);
            AlignCursor(e.MousePoint);

            Selected(this, e);
        }

        void ISelectable.OnSelectingOutside(PopUpWindowEventArgs e)
        {
            ChangeInputMode(false);

            OutsideSelected(this, e);
        }

        void IKeyInputReceiver.OnKeyInputReceive(PopUpWindowEventArgs e, Keys key)
        {
            if (FinishKeyInputModeIf(this) == true) // end the input mode
            {
                ChangeInputMode(false);
            }

            switch (key)
            {
                case Keys.Back:
                    DeleteFromContent();

                    break;
                case Keys.Enter:
                    ChangeInputMode(false);

                    break;
                case Keys.Right:
                    Cursor.AdvanceCharacterPositionBy(+1);

                    break;
                case Keys.Left:
                    Cursor.AdvanceCharacterPositionBy(-1);

                    break;
                case Keys.Up:
                    Cursor.SetAtCharacterPosition(TextBlock.Text.Length);

                    break;
                case Keys.Down:
                    Cursor.SetAtCharacterPosition(0);

                    break;
            }

            KeyInputReceived(this, e, key);
        }

        void ITextInputReceiver.OnTextInputReceive(PopUpWindowEventArgs e, char ch)
        {
            if (FinishTextInputModeIf(this))
            {
                ChangeInputMode(false);
                return;
            }

            if (!EligableForInput(ch)) return;

            if (!IsCaseSensitive) ch = char.ToUpper(ch);

            AddToContent(ch);
            TextInputReceived(this, e, ch);
        }

        private bool EligableForInput(char ch) 
        {
            return TextBlock.Font.Characters.Contains(ch) &&
                   !RestrictedCharsForInput.Contains(ch) &&
                   (
                        EnabledCharsForInput.Contains(ch) ||
                        (InputType.HasFlag(InputType.SpecialCharacter) && !char.IsLetterOrDigit(ch)) ||
                        (InputType.HasFlag(InputType.Digit) && char.IsDigit(ch)) ||
                        (InputType.HasFlag(InputType.Letter) && char.IsLetter(ch))
                   );
        }
        /// <summary>Changes the mode of the element.</summary>
        /// <param name="willInMode">New mode of the element.</param>
        public void ChangeInputMode(bool willInMode)
        {
            if (isInInputMode == willInMode) return;

            isInInputMode = willInMode;

            if (AutoActivateCursor)
            {
                if (willInMode) Cursor.Activate();
                else Cursor.Deactivate();
            }

            InputModeChanged(this, willInMode);
        }

        private void AlignCursor(Point mousePoint)
        {
            if (AutoActivateCursor)
            {
                Point positionInText = mousePoint - textOffset - this.area.Location;

                Cursor.SetAtPoint(positionInText);
            }
        }
        
        /// <summary>Adds <paramref name="ch"/> to end of current input if cursor is not used; else inserts <paramref name="ch"/> to character position of cursor.</summary>
        /// <param name="ch">Char to add to input.</param>
        public void AddToContent(char ch)
        {
            if (AutoActivateCursor)
            {
                TextBlock.Text = TextBlock.Text.Insert(Cursor.CurrentCharacterPosition, ch.ToString());
                Cursor.AdvanceCharacterPositionBy(+1);
            }
            else
            {
                TextBlock.Text += ch;
            }

            InputChanged(this, TextBlock.Text);
        }

        /// <summary>Removes the end char from input if cursor is not used; else removes the char at the index one less the character position of cursor.</summary>
        public void DeleteFromContent()
        {
            if (AutoActivateCursor)
            {
                if (Cursor.IsAtTheBeginning()) return;

                TextBlock.Text = TextBlock.Text.Remove(Cursor.CurrentCharacterPosition - 1, 1);
                Cursor.AdvanceCharacterPositionBy(-1);
            }
            else
            {
                if (TextBlock.Text.Length == 0) return;

                TextBlock.Text = TextBlock.Text[..^1];
            }

            InputChanged(this, TextBlock.Text);
        }
    }

    [Flags]
    public enum InputType
    {
        None = 0,
        Letter = 1,
        Digit = 2,
        SpecialCharacter = 4,

        LetterAndDigit = Letter | Digit,
        Any = Letter | Digit | SpecialCharacter
    }
}
