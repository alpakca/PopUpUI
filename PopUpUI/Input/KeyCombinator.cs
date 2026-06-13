using Microsoft.Xna.Framework.Input;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace PopUpUI.Input
{
    internal class KeyCombinator
    {
        private Combination? readyCombination;
        private readonly SortedSet<Combination> combinations = new SortedSet<Combination>();

        public KeyCombinator()
        {
            combinations = new SortedSet<Combination>();
        }

        public void AddOneTime(Keys[] keys, string name)
        {
            AddFirstDelayedMultiTime(keys, name, 0, 0);
        }

        public void AddDelayedMultiTime(Keys[] keys, string name, float delaySpan)
        {
            AddFirstDelayedMultiTime(keys, name, delaySpan, delaySpan);
        }

        public void AddFirstDelayedMultiTime(Keys[] keys, string name, float firstDelaySpan, float consecutiveDelaySpan)
        {
            keys.Sort();
            combinations.Add(new Combination(keys, name, firstDelaySpan, consecutiveDelaySpan));
        }

        public string GetUsedCombination()
        {
            if (readyCombination != null)
            {
                return readyCombination.Name;
            }

            return "";
        }

        public void Update()
        {
            readyCombination = null;

            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            pressedKeys.Sort();

            foreach (var combination in combinations)
            {
                if (combination.Update(pressedKeys))
                {
                    readyCombination = combination;
                }
            }
        }

        private class Combination : IComparable<Combination>
        {
            private bool isActivated;
            private float remainingDelay;

            public Keys[] Keys { get; }
            public string Name { get; }
            public float FirstDelay { get; }
            public float ConsecutiveDelay { get; }

            public Combination(Keys[] keys, string name, float firstDelay, float consecutiveDelay)
            {
                Keys = keys;
                Name = name;
                FirstDelay = firstDelay;
                ConsecutiveDelay = consecutiveDelay;

                Reset();
            }

            public void Reset()
            {
                isActivated = false;
                remainingDelay = 0;
            }

            public bool Update(Keys[] pressed)
            {
                if (!Enumerable.SequenceEqual(pressed, Keys))
                {
                    Reset();
                    return false;
                }

                if (!isActivated)
                {
                    remainingDelay = FirstDelay;
                    isActivated = true;

                    return true;
                }

                remainingDelay -= Time.DeltaTime;
                if (remainingDelay <= 0)
                {
                    remainingDelay = ConsecutiveDelay;
                    return true;
                }

                return false;
            }

            public int CompareTo(Combination? other)
            {
                return this.Keys.Length.CompareTo(other!.Keys.Length);
            }
        }
    }
}
