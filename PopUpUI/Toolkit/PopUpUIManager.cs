using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable enable

namespace PopUpUI.Toolkit
{
    /// <summary>Presents the manager of the PopUpUI, which controls the initiazation, updates and draws of the project.</summary>
    public static class PopUpUIManager
    {
        private static bool isInitialized;

        /// <summary>Raised in each <see cref="FrameUpdate(GameTime)"/> call.</summary>
        public static event Action<GameTime>? FrameCall;
        /// <summary>Raised in each <see cref="RenderUpdate(GameTime)"/> call, it is the call of render before buffer draw starts.</summary>
        public static event Action<GameTime>? RenderCall;

        /// <summary>
        /// Initializes the PopUpUI with given parameters. 
        /// It is not possible to use PopUpUI without calling this method.
        /// </summary>
        /// <param name="game">A <see cref="Game"/> object to make visualization possible.</param>
        /// <param name="deviceManager">A <see cref="GraphicsDeviceManager"/> object for settings of main window.</param>
        public static void Initialize(Game game, GraphicsDeviceManager deviceManager)
        {
            if (isInitialized) return;

            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(deviceManager);

            Time.Initialize(game);
            InputManager.Initialize(game);
            DrawHelper.Initialize(game);
            WindowOptions.Initialize(game, deviceManager);
            ContentAssistant.Initialize(game);
            PopWindowManager.Initialize();

            isInitialized = true;
        }

        public static void Update(GameTime gameTime)
        {
            // always first call Time - to update delta time
            Time.Update(gameTime);
            InputManager.Update();

            PopWindowManager.Update();

            FrameCall?.Invoke(gameTime);
        }

        public static void RenderUpdate(GameTime gameTime)
        {
            PopWindowManager.RenderUpdate();

            RenderCall?.Invoke(gameTime);
        }

        public static void BufferDraw(SpriteBatch batch)
        {
            PopWindowManager.BufferDraw(batch);
        }
    }
}
