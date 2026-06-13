using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;

#nullable enable

namespace PopUpUI.Toolkit
{
    /// <summary>Represents the manager of the PopUpUI, which controls the initiazation, updates and draws of the project.</summary>
    public static class PopUpUIManager
    {
        private static bool isInitialized;

        /// <summary>Raised in each <see cref="Update(GameTime)"/> call.</summary>
        public static event Action<GameTime>? UpdateCall;
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

            ArgumentNullException.ThrowIfNull(game, nameof(game));
            ArgumentNullException.ThrowIfNull(deviceManager, nameof(deviceManager));

            Time.Initialize(game);
            InputManager.Initialize(game.Window);
            DrawHelper.Initialize(game);
            WindowOptions.Initialize(game, deviceManager);
            ContentAssistant.Initialize(game);
            PopWindowManager.Initialize();

            isInitialized = true;
        }

        /// <summary>CPU based update method that should be called in <see cref="Game.Update(GameTime)"/>.</summary>
        public static void Update(GameTime gameTime)
        {
            // always first call Time - to update delta time
            Time.Update(gameTime);
            InputManager.Update();

            PopWindowManager.Update();

            UpdateCall?.Invoke(gameTime);
        }

        /// <summary>An update method to do all drawing before draw of back buffer begins, should be called in <see cref="Game.Draw(GameTime)"/>.</summary>
        /// <param name="gameTime"></param>
        public static void RenderUpdate(GameTime gameTime)
        {
            PopWindowManager.RenderUpdate();

            RenderCall?.Invoke(gameTime);
        }

        /// <summary>Starts the drawing of back buffer, should be called in <see cref="Game.Draw(GameTime)"/>.</summary>
        /// <param name="batch"></param>
        public static void BufferDraw(SpriteBatch batch)
        {
            PopWindowManager.BufferDraw(batch);
        }
    }
}
