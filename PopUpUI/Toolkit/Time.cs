using Microsoft.Xna.Framework;
using System;

namespace PopUpUI.Toolkit
{
    /// <summary>Contains properties to help time management of the project.</summary>
    public static class Time
    {
        private static float deltaTime;
        private static bool isInitialized = false;
        private static TimeSpan elapsedGameTime;
        private static Game game;

        /// <summary>Gets the time elapsed between the last update and the one preceding it, in milliseconds.</summary>
        public static float DeltaTime => deltaTime;
        /// <summary>Gets or sets a fps limit for the project. To be worked with set fps, <see cref="UseFixedTimeStep"/> should be set to true.</summary>
        public static int TargetFrameRate
        {
            get => FPS;
            set => game.TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / value);
        }
        /// <summary>Gets the count of frame per second.</summary>
        public static int FPS => (int)(1000f / (1000 * deltaTime));
        /// <summary>Gets or sets if project will use fixed frame per second. Default value is true.</summary>
        public static bool UseFixedTimeStep
        {
            get => game.IsFixedTimeStep;
            set => game.IsFixedTimeStep = value;
        }
        /// <summary>Gets the total, the time since project started, time have passed.</summary>
        public static TimeSpan ElapsedGameTime => elapsedGameTime;

        internal static void Initialize(Game game)
        {
            if (isInitialized) return;

            Time.game = game;
            UseFixedTimeStep = true;

            isInitialized = true;
        }

        internal static void FrameUpdate(GameTime gameTime)
        {
            deltaTime = gameTime.ElapsedGameTime.Milliseconds;
            elapsedGameTime = gameTime.TotalGameTime;
        }
    }
}
