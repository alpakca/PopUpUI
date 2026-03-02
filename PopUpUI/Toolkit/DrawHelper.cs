using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;

namespace PopUpUI.Toolkit
{
    /// <summary>Contains methods to draw through <see cref="TextureDrawOptions"/> and create textures.</summary>
    public static class DrawHelper
    {
        private static bool IsInitialized = false;

        private static Texture2D singlePixel;
        private static SpriteBatch drawBatch;
        private static GraphicsDevice graphicsDevice;

        /// <summary>The <see cref="GraphicsDevice"/> of the project.</summary>
        public static GraphicsDevice Device => graphicsDevice;
        /// <summary>Batch to use for only drawing. To start and end a batch use BeginBatch() and EndBatch() methods.</summary>
        public static SpriteBatch Batch => drawBatch;

        internal static void Initialize(Game game)
        {
            if (IsInitialized) return;

            graphicsDevice = game.GraphicsDevice;
            GraphicsDevice.DiscardColor = Color.Transparent;

            singlePixel = new Texture2D(graphicsDevice, 1, 1);
            drawBatch = new SpriteBatch(graphicsDevice);
            
            IsInitialized = true;
        }

        /// <summary>Creates a <see cref="Texture2D"/> instance with given size.</summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <returns>The created <see cref="Texture2D"/> instance.</returns>
        public static RenderTarget2D CreateRenderTarget(int width, int height)
        {
            return new RenderTarget2D(graphicsDevice, width, height);
        }

        /// <summary>Creates a <see cref="RenderTarget2D"/> instance with given size.</summary>
        /// <param name="width">Width of the target.</param>
        /// <param name="height">Height of the target.</param>
        /// <returns>The created <see cref="RenderTarget2D"/> instance.</returns>
        public static Texture2D CreateTexture(int width, int height)
        {
            return new Texture2D(graphicsDevice, width, height);
        }

        /// <summary>Creates a <see cref="Texture2D"/> from file in <paramref name="path"/>.</summary>
        /// <param name="path">Path of the file to load <see cref="Texture2D"/>.</param>
        /// <returns>The loaded <see cref="Texture2D"/>.</returns>
        public static Texture2D FromFile(string path)
        {
            return Texture2D.FromFile(graphicsDevice, path);
        }
        
        public static void SetBeginBatch(RenderTarget2D target)
        {
            graphicsDevice.SetRenderTarget(target);
            drawBatch.Begin();
        }

        public static void SetBeginBatch(RenderTarget2D target, TextureDrawOptions options)
        {
            graphicsDevice.SetRenderTarget(target);
            drawBatch.Begin(options.SortMode, options.BlendState, options.SamplerState,
                            options.DepthStencilState, options.RasterizerState, options.Effect, options.TransformMatrix);
        }

        /// <summary>Ends the <see cref="drawBatch"/>.</summary>
        public static void EndBatch()
        {
            drawBatch.End();
        }

        public static void DrawBackground(Rectangle destinationArea, Color color)
        {
            drawBatch.Draw(singlePixel, destinationArea, color);
        }
    }
}
