using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using PopUpUI.Elements;

#nullable enable

namespace PopUpUI.Containers
{
    /// <summary>Represents a group of <see cref="PopUpElement"/>s and supports operations to be applied in the group.</summary>
    [DebuggerDisplay("Count = {Count,nq}{Name.Length > 0 ? \", Name: \" + Name : System.String.Empty,nq}")]
    public class ElementGroup : ZIndexCollection<PopUpElement>
    {
        private Point lastMovedPosition;

        /// <summary>Gets or sets a name for the group.</summary>
        public string Name { get; set; } = "";

        /// <summary>Raised when opacity of the group have changed. Returns the set opacity.</summary>
        public event Action<ElementGroup, float>? OpacityChanged;
        /// <summary>Raised when visibility of the group have changed. Returns the set visibility.</summary>
        public event Action<ElementGroup, Visibility>? VisibilityChanged;
        /// <summary>Raised when an offset have applied to group. Returns the offset amount.</summary>
        public event Action<ElementGroup, Point>? OffsetApplied;

        /// <summary>Initializes a new instance of <see cref="ElementGroup"/> class without any elements.</summary>
        public ElementGroup() : base()
        {
        }

        /// <summary>Initializes a new instance of <see cref="ElementGroup"/> class with a comparer for Z-Indexes.</summary>
        /// <param name="zIndexComparer">Comparer for the Z-Indexes.</param>
        public ElementGroup(IComparer<int>? zIndexComparer) : base(zIndexComparer)
        {
        }

        /// <summary>>Initializes a new instance of <see cref="ElementGroup"/> class with elements copied from <paramref name="collection"/>.</summary>
        /// <param name="collection">Elements to copy from.</param>
        public ElementGroup(IEnumerable<PopUpElement> collection) : base(collection)
        {
        }

        /// <summary>Initializes a new instance of <see cref="ElementGroup"/> class 
        /// with elements copied from <paramref name="collection"/> and a comparer for Z-Indexes.</summary>
        /// <param name="collection">Collection to copy elements from.</param>
        /// <param name="zIndexComparer">Comparer for the Z-Indexes.</param>
        public ElementGroup(IEnumerable<PopUpElement> collection, IComparer<int>? zIndexComparer) : base(collection, zIndexComparer)
        {
        }

        #region Group Operations
        /// <summary>Sets the visibility of each element.</summary>
        /// <param name="visibility">Visibility to set.</param>
        public void SetVisibility(Visibility visibility)
        {
            foreach (var element in sortedElements.Values)
            {
                element.Visibility = visibility;
            }

            VisibilityChanged?.Invoke(this, visibility);
        }

        /// <summary>Sets the opacity of each element.</summary>
        /// <param name="opacity">Opacity to set.</param>
        public void SetOpacity(float opacity)
        {
            foreach (var element in sortedElements.Values)
            {
                element.Opacity = opacity;
            }

            OpacityChanged?.Invoke(this, opacity);
        }

        /// <summary>Applies a multiplier to opacities of each element.</summary>
        /// <param name="multiplier">Multiplier to apply.</param>
        public void ApplyOpacityMultiplier(float multiplier)
        {
            foreach (var element in sortedElements.Values)
            {
                element.Opacity *= multiplier;
            }
        }

        /// <summary>Offsets the locations of each element by given <paramref name="amount"/>.</summary>
        /// <param name="amount">Amount of offset to apply.</param>
        public void Offset(Point amount)
        {
            foreach (var element in sortedElements.Values)
            {
                element.Area = new Rectangle(element.Area.Location + amount, element.Area.Size);
            }

            OffsetApplied?.Invoke(this, amount);
        }

        /// <summary>Moves elements as a group by taking base as <see cref="Point.Zero"/>.</summary>
        /// <param name="location"></param>
        public void MoveTo(Point location)
        {
            if (lastMovedPosition == location) return;

            Offset(location - lastMovedPosition);

            lastMovedPosition = location;
        }
        #endregion

        #region Search Methods
        /// <summary>Searches for the first element that matches the conditions of given predicate.</summary>
        /// <param name="match">Predicate to match with an element.</param>
        /// <returns>If found returns the found element that matches the conditions of given predicate; otherwise <see langword="null"/>.</returns>
        public PopUpElement? Find(Predicate<PopUpElement> match)
        {
            return sortedElements.FindValue(match);
        }

        /// <summary>Searches for the first element with name <paramref name="name"/>.</summary>
        /// <param name="name">Name of the element to look for.</param>
        /// <returns>If found returns the found element with name <paramref name="name"/>; otherwise <see langword="null"/>.</returns>
        public PopUpElement? FindByName(string name)
        {
            return sortedElements.FindValue(element => element.Name == name);
        }

        /// <summary>Searches for the last element that matches the conditions of given predicate.</summary>
        /// <param name="match">Predicate to match with an element.</param>
        /// <returns>If found returns the found element that matches the conditions of given predicate; otherwise <see langword="null"/>.</returns>
        public PopUpElement? FindLast(Predicate<PopUpElement> match)
        {
            return sortedElements.FindLastValue(match);
        }

        /// <summary>Searches for the last element with name <paramref name="name"/>.</summary>
        /// <param name="name">Name of the element to look for.</param>
        /// <returns>If found returns the found element with name <paramref name="name"/>; otherwise <see langword="null"/>.</returns>
        public PopUpElement? FindByNameLast(string name)
        {
            return sortedElements.FindLastValue(element => element.Name == name);
        }
        #endregion
    }
}
