using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;

namespace PopUpUI.Toolkit
{
    /// <summary>Contains methods to draw through <see cref="TextureDrawOptions"/> and create textures.</summary>
    public static class DrawHelper
    {
        private static bool IsInitialized = false;
        private static GraphicsDevice GraphicsDevice;
        private static SpriteBatch DrawBatch;

        /// <summary>The <see cref="Microsoft.Xna.Framework.Graphics.GraphicsDevice"/> of the project.</summary>
        public static GraphicsDevice Device => GraphicsDevice;
        /// <summary>Batch to use for only drawing. To start and end a batch use BeginBatch() and EndBatch() methods.</summary>
        public static SpriteBatch Batch => DrawBatch;

        internal static void Initialize(Game game)
        {
            if (IsInitialized) return;

            GraphicsDevice = game.GraphicsDevice;
            GraphicsDevice.DiscardColor = Color.Transparent;

            DrawBatch = new SpriteBatch(GraphicsDevice);
            
            IsInitialized = true;
        }

        /// <summary>Creates a <see cref="Texture2D"/> instance with given size.</summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <returns>The created <see cref="Texture2D"/> instance.</returns>
        public static RenderTarget2D CreateRenderTarget(int width, int height) => new RenderTarget2D(GraphicsDevice, width, height);
        /// <summary>Creates a <see cref="RenderTarget2D"/> instance with given size.</summary>
        /// <param name="width">Width of the target.</param>
        /// <param name="height">Height of the target.</param>
        /// <returns>The created <see cref="RenderTarget2D"/> instance.</returns>
        public static Texture2D CreateTexture(int width, int height) => new Texture2D(GraphicsDevice, width, height);

        /// <summary>Sets the <paramref name="target"/> to <see cref="Microsoft.Xna.Framework.Graphics.GraphicsDevice"/>.</summary>
        /// <param name="target"></param>
        public static void SetRenderTarget(RenderTarget2D target)
        {
            GraphicsDevice.SetRenderTarget(target);
        }

        /// <summary>Creates a <see cref="Texture2D"/> from file in <paramref name="path"/>.</summary>
        /// <param name="path">Path of the file to load <see cref="Texture2D"/>.</param>
        /// <returns>The loaded <see cref="Texture2D"/>.</returns>
        public static Texture2D FromFile(string path)
        {
            return Texture2D.FromFile(GraphicsDevice, path);
        }

        /// <summary>Begins the <see cref="DrawBatch"/> from <paramref name="options"/>.</summary>
        /// <param name="options">Draw options to use in <see cref="SpriteBatch"/>.</param>
        public static void BeginBatch(TextureDrawOptions options)
        {
            DrawBatch.Begin(options.SortMode, options.BlendState, options.SamplerState, 
                            options.DepthStencilState, options.RasterizerState, options.Effect, options.TransformMatrix);
        }

        /// <summary>Ends the <see cref="DrawBatch"/>.</summary>
        public static void EndBatch()
        {
            DrawBatch.End();
        }
    }
}
