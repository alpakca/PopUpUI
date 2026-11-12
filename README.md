# PopUpUI

A UI and input framework for MonoGame: window-owned UI elements that handle
rendering and input (hold, select, hover, mouse) through abstraction layer.

## Features
- **Windows:** `PopWindow` own elements and manage their order
- **UI Elements:** Ready to use 6 input-based, 5 rendered elements
- **Text Rendering:** Supporting rich text rendering with colors, inline images, layout adjustments
- **PUI Format:** Small interpreter for creating elements from descriptor files (.pui)
- **Animations:** Animation support for rendering of UI elements.
- **Toolkit:** Managers for time & contents

## Getting started

**Requirements**
- MonoGame 3.8.0 or above 
- .NET 10.0

**Installation**
1. Add PopUpUI as a project reference to your MonoGame project.
2. Wire it into your game's three core methods:

```csharp
using PopUpUI.Toolkit;

protected override void Initialize()
{
    PopUpUIManager.Initialize(this, _graphics);
    base.Initialize();
}

protected override void Update(GameTime gameTime)
{
    PopUpUIManager.Update(gameTime);
    base.Update(gameTime);
}

protected override void Draw(GameTime gameTime)
{
    PopUpUIManager.RenderUpdate(gameTime);

    GraphicsDevice.SetRenderTarget(null);
    GraphicsDevice.Clear(Color.CornflowerBlue);

    _spriteBatch.Begin();
    PopUpUIManager.BufferDraw(_spriteBatch);
    _spriteBatch.End();

    base.Draw(gameTime);
}
```

Once `Initialize`, `Update`, and `Draw` are hooked up, you can start
creating and attaching UI elements anywhere in your game code.

## Design notes

- **Ownership:** every UI element is owned by a window, and elements can be
  attached to one another in single or multi-parent hierarchies, so complex
  layouts compose out of simple elements.
- **Input routing:** input is handled through interfaces in `PopUpUI.Input`
  that elements inherit to receive hold, select, hover, and mouse events.
  Input can also be pulled directly via the static `InputManager` class when
  an element needs to read input outside the normal routing path.
- **Rendering:** rendering logic lives in each element's `RenderUpdate()`
  method, and the resulting draw data is exposed through `GetDisplayInfo()`,
  keeping the render step separate from update/input logic.

## Status
Feature-complete and stable. Built from Nov 2024 as part of another project, 
and rewritten from March 2026 (commit history starts here). 
Last updated Sep 2026. Used in production by NoteKeeper and SideKicker. 
Not under active development; maintenance only if a dependent project needs it. 
Known improvement: replace yield return-based iterations with a struct 
enumerators to cut per-frame allocations.

## License
MIT
