using Microsoft.Xna.Framework.Input;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.Input
{
    public class KeyboardCombination : IComparable<KeyboardCombination>
    {
        private readonly TimeSpan _originalFirstDelay;
        private readonly TimeSpan _originalConsecutiveDelay;

        private bool isInFirstDelay;
        private TimeSpan remainingDelay;

        public Keys[] Keys { get; }
        public Action FirstTriggerAction { get; set; }
        public Action ConsecutiveTriggerAction { get; set; }

        private KeyboardCombination(Keys[] keys, TimeSpan firstDelay, TimeSpan consecutiveDelays)
        {
            Keys = keys;
            Array.Sort(keys);

            _originalFirstDelay = firstDelay;
            _originalConsecutiveDelay = consecutiveDelays;

            isInFirstDelay = true;
            remainingDelay = firstDelay;
        }

        public int CompareTo(KeyboardCombination other)
        {
            return -this.Keys.Length.CompareTo(other.Keys.Length);
        }

        public bool Update()
        {
            remainingDelay -= TimeSpan.FromMilliseconds(Time.DeltaTime);
            if (remainingDelay.Ticks <= 0)
            {
                if (isInFirstDelay)
                {
                    isInFirstDelay = false;
                    FirstTriggerAction?.Invoke();
                }
                else
                {
                    ConsecutiveTriggerAction?.Invoke();
                }

                remainingDelay = _originalConsecutiveDelay;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            isInFirstDelay = true;
            remainingDelay = _originalFirstDelay;
        }

        public static KeyboardCombination GetSingleUse(Keys[] keys)
        {
            return new KeyboardCombination(keys, TimeSpan.Zero, TimeSpan.MaxValue);
        }

        public static KeyboardCombination GetFirstInstantMultiDelayed(Keys[] keys, TimeSpan delay)
        {
            return new KeyboardCombination(keys, TimeSpan.Zero, delay);
        }

        public static KeyboardCombination GetDelayed(Keys[] keys, TimeSpan firstDelay, TimeSpan consecutiveDelays)
        {
            return new KeyboardCombination(keys, firstDelay, consecutiveDelays);
        }
    }
}
