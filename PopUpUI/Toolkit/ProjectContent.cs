using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;
using System;

namespace PopUpUI.Toolkit
{
    internal class ProjectContent
    {
        private bool isCreated;
        private object contentReference; // reference to pin the content
        private readonly WeakReference weakReference = new WeakReference(null);

        public object CreationInfo { get; }
        public bool IsPinned { get; }
        public ContentType Type { get; }

        public bool IsCreated => isCreated;
        public bool IsActive => IsPinned ? contentReference != null : weakReference.Target != null;

        public object Content
        {
            get
            {
                if (IsPinned)
                {
                    if (!isCreated) CreateContent();

                    return contentReference;
                }
                else
                {
                    if (!weakReference.IsAlive)
                    {
                        CreateContent();
                    }

                    return weakReference.Target;
                }
            }
        }

        public ProjectContent(object creationInfo, bool isPinned, bool instantCreate, ContentType type)
        {
            IsPinned = isPinned;
            CreationInfo = creationInfo;
            Type = type;

            if (instantCreate) CreateContent();
        }

        public ProjectContent(object content)
        {
            isCreated = true;
            IsPinned = true;

            contentReference = content;
        }

        public void Dispose()
        {
            if (isCreated && Content is IDisposable disposable) disposable.Dispose();

            DisposeReferences();
        }

        public void DisposeReferences()
        {
            contentReference = null;
            weakReference.Target = null;
        }

        private void CreateContent()
        {
            isCreated = true;
            object createdObj = Type switch
            {
                ContentType.Texture2D => DrawHelper.FromFile(CreationInfo as string),
                ContentType.Song => Song.FromUri(System.IO.Path.GetFileName(CreationInfo as string), new Uri(CreationInfo as string, UriKind.Absolute)),
                ContentType.SoundEffect => SoundEffect.FromFile(CreationInfo as string),
                ContentType.AnimationState => AnimationState.FromFile(CreationInfo as string),
                ContentType.ColorBrush => new ColorBrush((Color)CreationInfo),

                _ => throw new ArgumentException("Invalid ContentType")
            };

            if (IsPinned)
            {
                contentReference = createdObj;
            }
            else
            {
                weakReference.Target = createdObj;
            }
        }
    }
}
