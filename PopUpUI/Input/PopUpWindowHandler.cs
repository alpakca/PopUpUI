using PopUpUI.Surface;

namespace PopUpUI.Input
{
    /// <summary>Represents the method that used for events of <see cref="PopUpWindow"/>.</summary>
    /// <param name="sender">Sender <see cref="PopUpWindow"/>.</param>
    /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
    public delegate void PopUpWindowHandler(PopUpWindow sender, PopUpWindowEventArgs e);
    /// <summary>Represents the method that used for events of <see cref="PopUpWindow"/>.</summary>
    /// <typeparam name="TExtraArgs"></typeparam>
    /// <param name="sender">Sender <see cref="PopUpWindow"/>.</param>
    /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
    /// <param name="args">Extra args that events contain.</param>
    public delegate void PopUpWindowHandler<in TExtraArgs>(PopUpWindow sender, PopUpWindowEventArgs e, TExtraArgs args);
}
