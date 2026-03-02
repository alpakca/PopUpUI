using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;
using System;
using System.Collections.Generic;

#nullable enable

namespace PopUpUI.Toolkit
{
    /// <summary>Presents the manager of the PopUpUI, which controls the initiazation, updates and draws of the project.</summary>
    public static class PopUpUIManager
    {
        private static readonly List<WeakReference<Animator>> _animators = new List<WeakReference<Animator>>();
        private static readonly List<WeakReference<AnimationState>> _animationStates = new List<WeakReference<AnimationState>>();

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

            InputManager.Initialize(game);
            DrawHelper.Initialize(game);
            Time.Initialize(game);
            WindowOptions.Initialize(game, deviceManager);
            ContentAssistant.Initialize(game);
            PopUpWindow.Initialize();

            isInitialized = true;
        }
        /*
        /// <summary>The update method for the PopUpUI and elements of <see cref="PopUpWindow"/>s.</summary>
        /// <param name="gameTime">A <see cref="GameTime"/> object presenting application time.</param>
        public static void Update(GameTime gameTime)
        {
            InputManager.Update();
            Time.Update(gameTime);

            UpdateAnimationStates();
            UpdateAnimators();
            ResetAnimationStates();

            PopUpWindow.UpdateWindows();

            FrameUpdate?.Invoke(gameTime);
        }*/

        public static void FrameUpdate(GameTime gameTime)
        {
            // always first call Time - to update delta time
            Time.FrameUpdate(gameTime);
            InputManager.FrameUpdate();

            FrameCall?.Invoke(gameTime);
        }

        public static void RenderUpdate(GameTime gameTime)
        {
            RenderCall?.Invoke(gameTime);
        }

        public static void BufferDraw(SpriteBatch batch)
        {
            
        }
        /*
        /// <summary>The draw method for the PopUpUI and <see cref="PopUpWindow"/>s.</summary>
        /// <param name="batch">The <see cref="SpriteBatch"/> to use while drawing <see cref="PopUpWindow"/>s to screen.</param>
        public static void Draw(SpriteBatch batch)
        {
            WindowOptions.InternalDraw(batch);
            PopUpWindow.DrawWindows(batch);

            DrawUpdate?.Invoke(batch);
        }*/

        internal static void RegisterAnimator(Animator animator)
        {
            _animators.Add(new WeakReference<Animator>(animator));
        }

        internal static void RegisterAnimationState(AnimationState state)
        {
            _animationStates.Add(new WeakReference<AnimationState>(state));
        }

        internal static void UpdateAnimators()
        {
            for (int i = _animators.Count - 1; i >= 0; i--)
            {
                if (_animators[i].TryGetTarget(out Animator? animator))
                {
                    animator.Update();
                }
                else
                {
                    _animators.RemoveAt(i);
                }
            }
        }

        internal static void UpdateAnimationStates()
        {
            for (int i = _animationStates.Count - 1; i >= 0; i--)
            {
                if (_animationStates[i].TryGetTarget(out AnimationState? state))
                {
                    state.Update();
                }
                else
                {
                    _animationStates.RemoveAt(i);
                }
            }
        }

        internal static void ResetAnimationStates()
        {
            for (int i = _animationStates.Count - 1; i >= 0; i--)
            {
                if (_animationStates[i].TryGetTarget(out AnimationState? state))
                {
                    state.ResetUpdate();
                }
                else
                {
                    _animationStates.RemoveAt(i);
                }
            }
        }
    }
}
