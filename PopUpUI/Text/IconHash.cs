using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PopUpUI.Text
{
    /// <summary>Presents a hash and its correspoding texture as an icon.</summary>
    public readonly struct IconHash
    {
        /// <summar>The hash presenting the icon.</summary>
        public readonly string Hash { get; }
        /// <summary>The size of the icon in the text.</summary>
        public readonly Point Size { get; }
        /// <summary>The offset of the icon from top-right corner of the last character written in the text before icon.</summary>
        public readonly Point Offset { get; }
        /// <summary>The icon that hash is linked to.</summary>
        public readonly Texture2D Icon { get; }

        /// <summary>Initializes a new instance of <see cref="IconHash"/> struct.</summary>
        /// <param name="hash">The hash presenting the icon.</param>
        /// <param name="size">Size of the icon.</param>
        /// <param name="offset">Offset of the icon.</param>
        /// <param name="icon">The icon that is linked to hash.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public IconHash(string hash, Point size, Point offset, Texture2D icon)
        {
            ArgumentException.ThrowIfNullOrEmpty(hash);
            ArgumentNullException.ThrowIfNull(icon);

            Hash = hash;
            Size = size;
            Offset = offset;
            Icon = icon;
        }
    }
}
