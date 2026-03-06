namespace PopUpUI.Surface
{
    /// <summary>Indicates the current state of a <see cref="Animation2D"/>.</summary>
    public enum AnimationStatus
    {
        /// <summary>The animation is currently playing.</summary>
        Playing,
        /// <summary>The animation was started and currently stopped.</summary>
        Stopped,
        /// <summary>The animation is not yet started.</summary>
        NotStarted,
        /// <summary>The animation has been finished.</summary>
        Finished
    }
}
