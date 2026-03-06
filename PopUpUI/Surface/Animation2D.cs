using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Linq;
using System.Collections.Generic;

#pragma warning disable 8632

namespace PopUpUI.Surface
{
    public class Animation2D
    {
        private int _frameIndex = 0;
        private bool _playing;
        private float _elapsedTime;

        private readonly Texture2D[] _frames;

        public int Width => _frames[0].Width;
        public int Height => _frames[0].Height;
        public int FrameCount => _frames.Length;
        public bool IsPlaying => _playing;
        public float ElapsedTime => _elapsedTime;
        public AnimationStatus Status
        {
            get
            {
                if (_playing)
                    return AnimationStatus.Playing;

                if (_frameIndex == FrameCount - 1)
                    return AnimationStatus.Finished;

                if (_elapsedTime == 0)
                    return AnimationStatus.NotStarted;
                else
                    return AnimationStatus.Stopped;
            }
        }
        public Texture2D CurrentFrame => _frames[_frameIndex];

        public bool Loops { get; set; } = false;
        public bool AutoPlay { get; set; } = true;
        public float Speed { get; set; } = 1f;
        public float FrameTimeGap { get; set; } = 150;
        public string Name { get; set; }

        public event Action<Animation2D>? Finished;

        public Animation2D(IEnumerable<Texture2D> frames) 
        {
            ArgumentNullException.ThrowIfNull(frames);

            if (!frames.Any())
                throw new ArgumentException("Cannot initialize animation without any frames");

            _frames = frames.ToArray();

            foreach (var frame in frames)
            {
                if (frame?.Width != this.Width || frame?.Height != this.Height)
                    throw new ArgumentException("Animation frames must be in same size");
            }
        }

        public Animation2D(Texture2D[] frames) : this((IEnumerable<Texture2D>)frames)
        {
        }

        public void DisposeFrames()
        {
            for (int i = 0; i < _frames.Length; i++)
            {
                _frames[i].Dispose();
            }
        }

        public void Play()
        {
            _playing = true;
        }

        public void Stop()
        {
            _playing = false;
        }

        public void Rewind()
        {
            _elapsedTime = 0;
        }

        public void Update()
        {
            if (AutoPlay && this.Status == AnimationStatus.NotStarted)
            {
                Play();
                return;
            }

            if (_playing)
            {
                _elapsedTime += Time.DeltaTime * Speed;
                _frameIndex = (int)(_elapsedTime / FrameTimeGap);

                if (_frameIndex >= FrameCount - 1)
                {
                    ReachedEnd();
                }
            }
        }

        private void ReachedEnd()
        {
            if (Loops)
                Rewind();
            else
                Stop();

            _frameIndex = FrameCount - 1;
            Finished?.Invoke(this);
        }

        public Animation2D Reverse()
        {
            return new Animation2D(_frames.Reverse());
        }
    }
}
