using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PopUpUI.Input
{
    public class KeyboardShortcutManager : PopElement
    {
        private KeyboardCombination _lastTriggered;
        private readonly SortedSet<KeyboardCombination> _combinations;

        public KeyboardCombination LastTriggered => _lastTriggered;

        public KeyboardShortcutManager()
        {
            _combinations = new SortedSet<KeyboardCombination>();
        }

        public void AddCombinations(params ReadOnlySpan<KeyboardCombination> combinations)
        {
            foreach (var combination in combinations) _combinations.Add(combination);
        }

        public override void Update()
        {
            _lastTriggered = null;

            Keys[] pressed = Keyboard.GetState().GetPressedKeys();
            Array.Sort(pressed);

            bool resetAll = false;
            foreach (var combination in _combinations)
            {
                if (resetAll)
                {
                    combination.Reset();
                    continue;
                }

                if (Enumerable.SequenceEqual(pressed, combination.Keys))
                {
                    if (combination.Update())
                    {
                        _lastTriggered = combination;
                    }

                    resetAll = true;
                }
                else
                {
                    combination.Reset();
                }
            }
        }
    }
}
