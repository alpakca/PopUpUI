using PopUpUI.Elements;

namespace PopUpUI.Input
{
    /// <summary>Represents the method that used for events of <see cref="PopUpElement"/>.</summary>
    /// <param name="sender">Sender <see cref="PopUpElement"/>.</param>
    /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
    public delegate void PopUpElementHandler(PopUpElement sender, PopUpWindowEventArgs e);
    /// <summary>Represents the method that used for events of <see cref="PopUpElement"/>.</summary>
    /// <typeparam name="TExtraArgs"></typeparam>
    /// <param name="sender">Sender <see cref="PopUpElement"/>.</param>
    /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
    /// <param name="args">Extra args that events contain.</param>
    public delegate void PopUpElementHandler<in TExtraArgs>(PopUpElement sender, PopUpWindowEventArgs e, TExtraArgs args);
}
