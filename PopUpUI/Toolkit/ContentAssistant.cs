using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Toolkit
{
    /// <summary>Contains methods to load, retrieve and manipulate contents for project.</summary>
    public static class ContentAssistant
    {
        private static bool isInitialized;
        private static ContentManager manager;
        private static readonly Dictionary<string, ProjectContent> LoadedContents = new();

        /// <summary>
        /// If set to <see langword="true"/>, assistant does not make contents eligable for <see cref="GC"/> collection by creating a strong reference to them.
        /// Otherwise contents may be collected by <see cref="GC"/>.
        /// Suggested if a content takes long enough to load. Default is <see langword="false"/>.
        /// </summary>
        public static bool PinContents { get; set; } = false;
        /// <summary>
        /// If set to <see langword="true"/>, assistant loads contents instantly. 
        /// Otherwise, contents are created once they are accessed for first time or they were not pinned and was collected by <see cref="GC"/>.
        /// Default is <see langword="false"/>.
        /// </summary>
        public static bool InstantCreateContents { get; set; } = false;

        /// <summary>Gets the current number of contents that are not collected by <see cref="GC"/> and is created.</summary>
        public static int ActiveContentsCount => LoadedContents.Values.Where(node => node.IsActive).Count();
        /// <summary>Gets the total number of contents that are loaded to assistant.</summary>
        public static int AllContentsCount => LoadedContents.Count;

        internal static void Initialize(Game game)
        {
            if (isInitialized) return;

            manager = game.Content;

            isInitialized = true;
        }

        /// <summary>Loads contents of same type to project with <see cref="ContentManager"/> for a later use. 
        /// These contents are not managed by <see cref="ContentAssistant"/>.</summary>
        /// <typeparam name="T">The type of objects to load.</typeparam>
        /// <param name="contents">
        /// First item is for the name of the content, 
        /// which will be used to load from content pipeline directory, works same as <see cref="ContentManager.Load{T}(string)"/>. <br/>
        /// Second item is a name for the saved content, which can be used to retrieve contents later from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadWithContentManager<T>(params (string assetName, string nameToSave)[] contents)
        {
            foreach (var (assetName, nameToSave) in contents)
            {
                LoadedContents.Add(nameToSave, new ProjectContent(manager.Load<T>(assetName)));
            }
        }

        #region Load Overloads
        /// <summary>Loads <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/>s to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="contents">
        /// First item is path of the <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/>, 
        /// second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadTexture2Ds(params (string path, string savedName)[] contents)
            => Load(ContentType.Texture2D, PinContents, InstantCreateContents, contents);

        /// <summary>Loads <see cref="AnimationState"/>s to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="contents">
        /// First item is path of the <see cref="AnimationState"/>, second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadAnimationStates(params (string path, string savedName)[] contents)
            => Load(ContentType.AnimationState, PinContents, InstantCreateContents, contents);

        /// <summary>Loads <see cref="Song"/>s to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="contents">
        /// First item is path of the <see cref="Song"/>, second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadSongs(params (string path, string savedName)[] contents)
            => Load(ContentType.Song, PinContents, InstantCreateContents, contents);

        /// <summary>Loads <see cref="SoundEffect"/>s to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="contents">
        /// First item is path of the <see cref="SoundEffect"/>, second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadSoundEffects(params (string path, string savedName)[] contents)
            => Load(ContentType.SoundEffect, PinContents, InstantCreateContents, contents);

        /// <summary>Loads <see cref="ColorBrush"/>s to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="contents">
        /// First item is path of the <see cref="ColorBrush"/>, second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadBrushes(params (Color color, string savedName)[] colors)
            => Load(ContentType.ColorBrush, PinContents, InstantCreateContents, colors);

        /// <summary>Loads <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/>s to project for a later use 
        /// with options <paramref name="isPinned"/> and <paramref name="instantCreate"/>.</summary>
        /// <param name="isPinned">If <see langword="true"/>, contents won't be eligable for <see cref="GC"/> by creating strong references.</param>
        /// <param name="instantCreate">If <see langword="true"/>, contents will be created instantly, without waiting their first call.</param>
        /// <param name="contents">
        /// First item is path of the <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/>, 
        /// second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadTexture2Ds(bool isPinned, bool instantCreate, params (string path, string savedName)[] contents)
            => Load(ContentType.Texture2D, isPinned, instantCreate, contents);

        /// <summary>Loads <see cref="AnimationState"/>s to project for a later use 
        /// with options <paramref name="isPinned"/> and <paramref name="instantCreate"/>.</summary>
        /// <param name="isPinned">If <see langword="true"/>, contents won't be eligable for <see cref="GC"/> by creating strong references.</param>
        /// <param name="instantCreate">If <see langword="true"/>, contents will be created instantly, without waiting their first call.</param>
        /// <param name="contents">
        /// First item is path of the <see cref="AnimationState"/>, second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadAnimationStates(bool isPinned, bool instantCreate, params (string path, string savedName)[] contents)
            => Load(ContentType.AnimationState, isPinned, instantCreate, contents);

        /// <summary>Loads <see cref="Song"/>s to project for a later use 
        /// with options <paramref name="isPinned"/> and <paramref name="instantCreate"/>.</summary>
        /// <param name="isPinned">If <see langword="true"/>, contents won't be eligable for <see cref="GC"/> by creating strong references.</param>
        /// <param name="instantCreate">If <see langword="true"/>, contents will be created instantly, without waiting their first call.</param>
        /// <param name="contents">
        /// First item is path of the <see cref="Song"/>, second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadSongs(bool isPinned, bool instantCreate, params (string path, string savedName)[] contents)
            => Load(ContentType.Song, isPinned, instantCreate, contents);

        /// <summary>Loads <see cref="SoundEffect"/>s to project for a later use 
        /// with options <paramref name="isPinned"/> and <paramref name="instantCreate"/>.</summary>
        /// <param name="isPinned">If <see langword="true"/>, contents won't be eligable for <see cref="GC"/> by creating strong references.</param>
        /// <param name="instantCreate">If <see langword="true"/>, contents will be created instantly, without waiting their first call.</param>
        /// <param name="contents">
        /// First item is color of the <see cref="SoundEffect"/>, second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadSoundEffects(bool isPinned, bool instantCreate, params (string path, string savedName)[] contents)
            => Load(ContentType.SoundEffect, isPinned, instantCreate, contents);

        /// <summary>Loads <see cref="ColorBrush"/>s to project for a later use 
        /// with options <paramref name="isPinned"/> and <paramref name="instantCreate"/>.</summary>
        /// <param name="isPinned">If <see langword="true"/>, contents won't be eligable for <see cref="GC"/> by creating strong references.</param>
        /// <param name="instantCreate">If <see langword="true"/>, contents will be created instantly, without waiting their first call.</param>
        /// <param name="colors">
        /// First item is color of the <see cref="ColorBrush"/>, second item is the name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.
        /// </param>
        public static void LoadBrushes(bool isPinned, bool instantCreate, params (Color color, string savedName)[] colors)
            => Load(ContentType.ColorBrush, isPinned, instantCreate, colors);
        #endregion

        private static void Load<T>(ContentType type, bool isPinned, bool instantCreate, (T info, string savedName)[] contents)
        {
            foreach (var (info, savedName) in contents)
            {
                LoadedContents.Add(savedName, new ProjectContent(info, isPinned, instantCreate, type));
            }
        }

        #region TryLoad Overloads
        /// <summary>Tries to load a <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="path">The path of the <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadTexture2D(string path, string savedName)
            => TryLoad(ContentType.Texture2D, PinContents, InstantCreateContents, path, savedName);

        /// <summary>Tries to load a <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> to project for a later use 
        /// with options <paramref name="isPinned"/> and <paramref name="instantCreate"/>.</summary>
        /// <param name="path">The path of the <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadTexture2D(bool isPinned, bool instantCreate, string path, string savedName)
            => TryLoad(ContentType.Texture2D, isPinned, instantCreate, path, savedName);

        /// <summary>Tries to load a <see cref="AnimationState"/> to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="path">The path of the <see cref="AnimationState"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadAnimationState(string path, string savedName)
            => TryLoad(ContentType.AnimationState, PinContents, InstantCreateContents, path, savedName);

        /// <summary>Tries to load a <see cref="AnimationState"/> to project for a later use 
        /// with options <paramref name="isPinned"/> and <paramref name="instantCreate"/>.</summary>
        /// <param name="path">The path of the <see cref="AnimationState"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadAnimationState(bool isPinned, bool instantCreate, string path, string savedName)
            => TryLoad(ContentType.AnimationState, isPinned, instantCreate, path, savedName);

        /// <summary>Tries to load a <see cref="Song"/> to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="Song"/>.</summary>
        /// <param name="path">The path of the <see cref="AnimationState"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadSong(string path, string savedName)
            => TryLoad(ContentType.Song, PinContents, InstantCreateContents, path, savedName);

        /// <summary>Tries to load a <see cref="Song"/> to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="path">The path of the <see cref="Song"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadSong(bool isPinned, bool instantCreate, string path, string savedName)
            => TryLoad(ContentType.Song, isPinned, instantCreate, path, savedName);

        /// <summary>Tries to load a <see cref="SoundEffect"/> to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="path">The path of the <see cref="SoundEffect"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadSoundEffect(string path, string savedName)
            => TryLoad(ContentType.SoundEffect, PinContents, InstantCreateContents, path, savedName);

        /// <summary>Tries to load a <see cref="SoundEffect"/> to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="path">The path of the <see cref="SoundEffect"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadSoundEffect(bool isPinned, bool instantCreate, string path, string savedName)
            => TryLoad(ContentType.SoundEffect, isPinned, instantCreate, path, savedName);

        /// <summary>Tries to load a <see cref="ColorBrush"/> to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="color">The color of the <see cref="ColorBrush"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadBrush(Color color, string savedName)
            => TryLoad(ContentType.ColorBrush, PinContents, InstantCreateContents, color, savedName);

        /// <summary>Tries to load a <see cref="ColorBrush"/> to project for a later use 
        /// with options <see cref="PinContents"/> and <see cref="InstantCreateContents"/>.</summary>
        /// <param name="color">The color of the <see cref="ColorBrush"/> to load.></param>
        /// <param name="savedName">The name for the saved content for retrieving it from <see cref="FromLoaded{T}(string)"/>.</param>
        /// <returns><see langword="true"/> if content was loaded successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryLoadBrush(bool isPinned, bool instantCreate, Color color, string savedName)
            => TryLoad(ContentType.ColorBrush, isPinned, instantCreate, color, savedName);
        #endregion

        private static bool TryLoad(ContentType type, bool isPinned, bool instantCreate, object info, string savedName)
        {
            return LoadedContents.TryAdd(savedName, new ProjectContent(info, isPinned, instantCreate, type));
        }

        /// <summary>Retrieves the content saved before with the name used to save it.</summary>
        /// <typeparam name="T">The type of the content to be retrieved.</typeparam>
        /// <param name="savedName">The name used to save the content.</param>
        /// <returns>Returns the saved content with the name <paramref name="savedName"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static T FromLoaded<T>(string savedName)
        {
            if (LoadedContents.TryGetValue(savedName, out ProjectContent node))
            {
                return (T)node.Content;
            }

            throw new ArgumentException(savedName + " was not loaded");
        }

        /// <summary>Tries to retrieve the content saved before with the name used to save it.</summary>
        /// <typeparam name="T">The type of the content to be retrieved.</typeparam>
        /// <param name="savedName">The name used to save the content.</param>
        /// <param name="content">The retrieved content if it was already loaded.</param>
        /// <returns>Returns <see langword="true"/> if the content was retrieved successfully, else <see langword="false"/>.</returns>
        public static bool TryGetFromLoaded<T>(string savedName, out T content)
        {
            if (LoadedContents.TryGetValue(savedName, out ProjectContent node))
            {
                content = (T)node.Content;
                return true;
            }

            content = default;
            return false;
        }

        /// <summary>Unloads the content saved before with the name used to save it. 
        /// It also disposes the content if implements <see cref="IDisposable"/>, and makes it eligable for <see cref="GC"/> collection.</summary>
        /// <param name="savedName">The name used to save the content.</param>
        /// <returns>Returns <see langword="true"/> if the content was unloaded successfully, else <see langword="false"/> 
        /// It also returns <see langword="false"/> if any content was not found with <paramref name="savedName"/>.</returns>
        public static bool Unload(string savedName)
        {
            if (LoadedContents.TryGetValue(savedName, out ProjectContent node))
            {
                node.Dispose();
                LoadedContents.Remove(savedName);

                return true;
            }

            return false;
        }

        /// <summary>Unloads the content from <see cref="ContentAssistant"/> saved before with the name used to save it 
        /// and gets the content with name <paramref name="savedName"/>. It does NOT dispose the content.</summary>
        /// <typeparam name="T">The type of the content that will be unloaded.</typeparam>
        /// <param name="savedName">The name used to save the content.</param>
        /// <param name="removedContent">The removed content.</param>
        /// <returns>Returns <see langword="true"/> if the content was unloaded successfully, else <see langword="false"/> 
        /// It also returns <see langword="false"/> if any content was not found with <paramref name="savedName"/>.</returns>
        public static bool Unload<T>(string savedName, out T removedContent)
        {
            if (LoadedContents.TryGetValue(savedName, out ProjectContent node))
            {
                node.DisposeReferences();
                LoadedContents.Remove(savedName);

                removedContent = (T)node.Content;
                return true;
            }

            removedContent = default;
            return false;
        }
    }
}
