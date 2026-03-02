namespace PopUpUI.Surface.Animation
{
    /// <summary>Indicates the current state of a <see cref="AnimationState"/>.</summary>
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
