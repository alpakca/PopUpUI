using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PopUpUI.Surface
{
    /// <summary>Presents the options to use while beginning batch <see cref="SpriteBatch"/>.</summary>
    public class TextureDrawOptions
    {
        private static readonly TextureDrawOptions defaultDrawOptions = new TextureDrawOptions { BlendState = BlendState.NonPremultiplied, _isReadonly = true };
        /// <summary>Gets a readonly default options for drawing texture.</summary>
        public static TextureDrawOptions Default => defaultDrawOptions;

        private bool _isReadonly = false;

        private SpriteSortMode _sortMode;
        private BlendState _blendState;
        private SamplerState _samplerState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;
        private Effect _effect;
        private Matrix? _transformMatrix;

        /// <summary>SpriteSortMode to use while drawing.</summary>
        public SpriteSortMode SortMode
        {
            get => _sortMode;
            set
            {
                ValidateReadonly();
                _sortMode = value;
            }
        }

        /// <summary>BlendState to use while drawing.</summary>
        public BlendState BlendState
        {
            get => _blendState;
            set
            {
                ValidateReadonly();
                _blendState = value;
            }
        }

        /// <summary>SamplerState to use while drawing.</summary>
        public SamplerState SamplerState
        {
            get => _samplerState;
            set
            {
                ValidateReadonly();
                _samplerState = value;
            }
        }

        /// <summary>DepthStencilState to use while drawing.</summary>
        public DepthStencilState DepthStencilState
        {
            get => _depthStencilState;
            set
            {
                ValidateReadonly();
                _depthStencilState = value;
            }
        }

        /// <summary>RasterizerState to use while drawing.</summary>
        public RasterizerState RasterizerState
        {
            get => _rasterizerState;
            set
            {
                ValidateReadonly();
                _rasterizerState = value;
            }
        }

        /// <summary>Effect to use while drawing.</summary>
        public Effect Effect
        {
            get => _effect;
            set
            {
                ValidateReadonly();
                _effect = value;
            }
        }

        /// <summary><see cref="Matrix"/> to use while drawing.</summary>
        public Matrix? TransformMatrix
        {
            get => _transformMatrix;
            set
            {
                ValidateReadonly();
                _transformMatrix = value;
            }
        }

        /// <summary>Initializes options with default properties.</summary>
        public TextureDrawOptions()
        {
            SortMode = SpriteSortMode.Deferred;
            TransformMatrix = null;
        }

        /// <summary>Initializes options with copying from <paramref name="options"/>.</summary>
        /// <param name="options">Options to copy from.</param>
        public TextureDrawOptions(TextureDrawOptions options)
        {
            _blendState = options.BlendState;
            _samplerState = options.SamplerState;
            _depthStencilState = options.DepthStencilState;
            _rasterizerState = options.RasterizerState;
            _effect = options.Effect;
            _transformMatrix = options.TransformMatrix;
        }

        /// <summary>Makes options readonly to be immutable.</summary>
        public void MakeReadonly()
        {
            _isReadonly = true;
        }

        private void ValidateReadonly()
        {
            if (_isReadonly)
            {
                throw new ArgumentException("Can't modify a readonly TextureDrawOptions instance.");
            }
        }
    }
}
