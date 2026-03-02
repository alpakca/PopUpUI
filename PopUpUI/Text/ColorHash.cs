using Microsoft.Xna.Framework;
using System;

namespace PopUpUI.Text
{
    /// <summary>Presents the hashes and their corresponding color.</summary>
    public readonly struct ColorHash
    {
        /// <summary>The hash that notifies the start of coloring progress.</summary>
        public readonly string StartHash { get; }
        /// <summary>The hash that notifies coloring progress reached end.</summary>
        public readonly string EndHash { get; }
        /// <summary>The color that hash is linked to.</summary>
        public readonly Color Color { get; }

        /// <summary>Initializes a new instance of <see cref="ColorHash"/> struct. <paramref name="startHash"/> can't be same as <paramref name="endHash"/>.</summary>
        /// <param name="startHash">The hash that notifies the start of coloring progress. Can't be same as <paramref name="endHash"/>.</param>
        /// <param name="endHash">The hash that notifies coloring progress reached end. Can't be same as <paramref name="startHash"/>.</param>
        /// <param name="color">The color that hash is linked to.</param>
        /// <exception cref="ArgumentException"></exception>
        public ColorHash(string startHash, string endHash, Color color)
        {
            ArgumentException.ThrowIfNullOrEmpty(startHash);
            ArgumentException.ThrowIfNullOrEmpty(endHash);
            if (startHash == endHash) throw new ArgumentException("startHash and endHash cannot be same");

            StartHash = startHash;
            EndHash = endHash;
            Color = color;
        }
    }
}
