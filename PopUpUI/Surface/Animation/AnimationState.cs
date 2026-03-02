using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PopUpUI.Surface.Animation
{
    /// <summary>Represents a state of <see cref="Animator"/> and creates an animation with <see cref="Texture2D"/>.</summary>
    [DebuggerDisplay("Status: {Status,nq}{Name.Length > 0 ? \", Name: \" + Name : System.String.Empty,nq}")]
    public class AnimationState
    {
        static AnimationState()
        {
            Texture2D txt = DrawHelper.CreateTexture(1, 1);
            txt.SetData([Color.Transparent]);

            Empty = new AnimationState(txt)
            {
                PlayAutomatically = false,
                Name = "Empty State"
            };
        }

        /// <summary>Gets an <see cref="AnimationState"/> that is 1x1 size and transparent.</summary>
        public static AnimationState Empty { get; }

        private int frameIndex;
        private bool _isPlaying;
        private float _elapsedTime;
        private float _remainingExitTime;
        private readonly Texture2D[] Frames;

        private bool _isUpdated;

        /// <summary>The width of the animation. All frames of the animation have the same width.</summary>
        public int Width => Frames[0].Width;
        /// <summary>The height of the animation. All frames of the animation have the same height.</summary>
        public int Height => Frames[0].Height;
        /// <summary>The number of the frames animation has.</summary>
        public int FrameCount => Frames.Length;
        /// <summary>Indicates if animation is playing.</summary>
        public bool IsPlaying => _isPlaying;
        /// <summary>Indicates if animation was created through a single frame.</summary>
        public bool IsSingleTexture => FrameCount == 1;
        /// <summary>Gets the time passed since start of the animation, in milliseconds.</summary>
        public float ElapsedTime => _elapsedTime;
        /// <summary>Gets the remaining time from <see cref="ExitTime"/> when exiting animation.</summary>
        public float RemainingExitTime => _remainingExitTime;
        /// <summary>Gets the current status of the animation.</summary>
        public AnimationStatus Status
        {
            get
            {
                if (!_isPlaying && _elapsedTime == 0) return AnimationStatus.NotStarted;
                else if (!_isPlaying && frameIndex == FrameCount - 1) return AnimationStatus.Finished;
                else if (!_isPlaying && _elapsedTime != 0) return AnimationStatus.Stopped;
                else return AnimationStatus.Playing;
            }
        }

        /// <summary>Gets or sets a bool indicating animation will start again from start when reached to end. Default is <see langword="false"/>.</summary>
        public bool Looping { get; set; } = false;
        /// <summary>Gets or sets a bool indicating whether the animation should start playing 
        /// when it is converted to Texture2D via the implicit operator and wasn't playing. Default is <see langword="true"/>.</summary>
        public bool PlayAutomatically { get; set; } = true;
        /// <summary>Gets or sets a bool indicating <see cref="ExitTime"/> will be used. Default is <see langword="false"/>.</summary>
        public bool HasExitTime { get; set; } = false;
        /// <summary>Gets or sets the speed of the animation. Default is 1.0f.</summary>
        public float Speed { get; set; } = 1;
        /// <summary>Gets or sets the time between frames of the animation, in milliseconds. Default is 150.</summary>
        public int TimeBetweenFrames { get; set; } = 150;
        /// <summary>Gets or sets the time to exit, that is the time animation continues before switching to a new state in <see cref="Animator"/>;
        /// and the time between last and first frames of the animation, if <see cref="Looping"/> is <see langword="true"/>. In milliseconds.</summary>
        public int ExitTime { get; set; } = 0;
        /// <summary>Gets or sets a name for the animation.</summary>
        public string Name { get; set; } = "";

        /// <summary>Raised when animation reached its last frame.</summary>
        public event Action<AnimationState> Finished;

        /// <summary>Initializes a new instance of <see cref="AnimationState"/> class with a single frame.</summary>
        /// <param name="frame">The only frame animation will have.</param>
        public AnimationState(Texture2D frame)
        {
            ArgumentNullException.ThrowIfNull(frame);

            Frames = [frame];

            PopUpUIManager.RegisterAnimationState(this);
        }

        /// <summary>Initializes a new instance of <see cref="AnimationState"/> class with <paramref name="frames"/> of same size.</summary>
        /// <param name="frames">The collection that contains the frames. Frames must have the same size.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public AnimationState(IEnumerable<Texture2D> frames)
        {
            ArgumentNullException.ThrowIfNull(frames);
            if (!frames.Any()) throw new ArgumentException("Collection does not contain any frames.", nameof(frames));

            Frames = [.. frames];

            foreach (var frame in frames)
            {
                if (frame?.Height != Height || frame?.Width != Width) throw new ArgumentException("Frames must have the same size.", nameof(frames));
            }

            PopUpUIManager.RegisterAnimationState(this);
        }

        /// <summary>Disposes the frames this animation contains.</summary>
        public void DisposeFrames()
        {
            foreach (var frame in Frames) frame.Dispose();
        }

        /// <summary>Continues to play the animation if it was stopped; else starts the animation.</summary>
        public void Play()
        {
            _isPlaying = true;
        }

        /// <summary>Stops the animation from playing.</summary>
        public void Stop()
        {
            _isPlaying = false;
        }

        /// <summary>Returns back to start of the animation.</summary>
        public void Rewind()
        {
            _elapsedTime = 0;
        }

        internal void Update()
        {
            if (_isUpdated) return;
            _isUpdated = true;

            if (_remainingExitTime > 0)
            {
                _remainingExitTime -= Time.DeltaTime * Speed * 1000;
                return;
            }

            if (_isPlaying)
            {
                _elapsedTime += Time.DeltaTime * Speed * 1000;
                frameIndex = (int)(_elapsedTime / TimeBetweenFrames);

                if (frameIndex >= FrameCount - 1) ReachedEnd();
            }
        }

        internal void ResetUpdate()
        {
            _isUpdated = false;
        }

        private void ReachedEnd()
        {
            if (HasExitTime)
            {
                _remainingExitTime = ExitTime;
            }

            if (Looping) Rewind();
            else Stop();

            frameIndex = FrameCount - 1;
            Finished?.Invoke(this);
        }

        public static implicit operator Texture2D(AnimationState animation)
        {                                      // checking if animation is not started
            if (animation.PlayAutomatically && animation._elapsedTime == 0 && !animation._isPlaying) animation.Play();

            return animation.Frames[animation.frameIndex];
        }

        /// <summary>Creates a new <see cref="AnimationState"/> which has the same frames as <paramref name="state"/> but in reverse order.</summary>
        /// <param name="state">The animation to reverse.</param>
        /// <returns>The animation that is reverse version of <paramref name="state"/>.</returns>
        public static AnimationState GetReversed(AnimationState state)
        {
            return new AnimationState(state.Frames.Reverse());
        }

        /// <summary>Loads a animation from file at <paramref name="path"/>, which can have a json file associated with it to create animation from one texture.</summary>
        /// <remarks>
        /// The json file attached to image should be in the same directory and have same name with path of the image. 
        /// Otherwise animation will be loaded as a single-frame animation. <br/>
        /// Json file must specify a property, FrameHeight: The height of the frames. It must be exactly divisible to the height of the image at the path.
        /// </remarks>
        /// <param name="path">The path of the texture.</param>
        /// <returns>The creared animation.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static AnimationState FromFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path + " is not found or accessable.");

            string jsonFilePath = Path.ChangeExtension(path, ".json");
            Texture2D animationTexture = DrawHelper.FromFile(path);

            if (!File.Exists(jsonFilePath))
            {
                return new AnimationState(animationTexture);
            }

            int frameHeight = 0;
            bool looping = false;
            bool playAutomatically = true;
            bool hasExitTime = false;
            float speed = 1;
            int timeBetweenFrames = 150;
            int exitTime = 0;
            string name = "";

            Utf8JsonReader reader = new Utf8JsonReader(File.ReadAllBytes(jsonFilePath));
            if (!reader.Read()) throw new JsonException("Wrong json format for AnimationState");

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "FrameHeight":
                        frameHeight = reader.GetInt32();
                        break;
                    case "Looping":
                        looping = reader.GetBoolean();
                        break;
                    case "PlayAutomatically":
                        playAutomatically = reader.GetBoolean();
                        break;
                    case "HasExitTime":
                        hasExitTime = reader.GetBoolean();
                        break;
                    case "Speed":
                        speed = reader.GetSingle();
                        break;
                    case "TimeBetweenFrames":
                        timeBetweenFrames = reader.GetInt32();
                        break;
                    case "ExitTime":
                        exitTime = reader.GetInt32();
                        break;
                    case "Name":
                        name = reader.GetString();
                        break;
                }
            }

            if (frameHeight == 0) throw new ArgumentOutOfRangeException("Frame height must be greater than zero");
            if (animationTexture.Height % frameHeight != 0) throw new ArgumentException("Main texture size does not support equal sized frames", path);
            int count = animationTexture.Height / frameHeight;

            List<Texture2D> frames = new List<Texture2D>();
            for (int i = 0; i < count; i++)
            {
                frames.Add(animationTexture.CropTexture(new Rectangle(0, i * frameHeight, animationTexture.Width, frameHeight)));
            }

            return new AnimationState(frames)
            {
                Looping = looping,
                PlayAutomatically = playAutomatically,
                HasExitTime = hasExitTime,
                Speed = speed,
                TimeBetweenFrames = timeBetweenFrames,
                ExitTime = exitTime,
                Name = name
            };
        }
    }
}
