# D2Phap.DXControl
- A WinForms control that supports drawing with Direct2D thanks to [WicNet](https://github.com/smourier/WicNet).
- This control has been used in [ImageGlass](https://github.com/d2phap/ImageGlass) software since version 9.0.

![Nuget](https://img.shields.io/nuget/dt/D2Phap.DXControl?color=%2300a8d6&logo=nuget)


## Resource links
- Nuget package: [https://www.nuget.org/packages/D2Phap.DXControl](https://www.nuget.org/packages/D2Phap.DXControl)
- Project url: [https://github.com/d2phap/DXControl](https://github.com/d2phap/DXControl)
- About: [https://imageglass.org/about](https://imageglass.org/about)


## Features
- High performance drawing using Direct2D.
- Names and types are exactly the same as the native concepts of Direct2D (interfaces, enums, structures, constants, methods, arguments, guids, etc...). So you can read the official documentation, use existing C/C++ samples, and start coding with .NET right away.
- All native COM interfaces are generated as .NET (COM) interfaces, this makes .NET programming easier, but they are not strictly needed.
- Option to use Software or Hardware render target
- Supports animation drawing with Direct2D.

## Requirements:
- .NET 8.0, 9.0

## Installation
Run the command
```bash
Install-Package D2Phap.DXControl
```


## Example


<img src="https://github.com/user-attachments/assets/837414d9-b342-487b-99c9-056b8a24205d" width="500" />

Draws a rectangle, then moves it to the right side.

```cs
using D2Phap.DXControl;

// create a WinForms custom control that extends from DXCanvas
public class DemoCanvas : DXCanvas
{
    private RectangleF animatableRectangle = new(100, 100, 400, 200);

    public DemoCanvas()
    {
        EnableAnimation = true;
        UseHardwareAcceleration = true;
    }

    protected override void OnRender(DXGraphics g)
    {
        // draw a yellow rectangle with green border
        g.FillRectangle(rectText, Color.FromArgb(100, Yellow));
        g.DrawRectangle(rectText, Color.Green);
    }

    // Update frame logics for animation
    protected override void OnFrame(FrameEventArgs e)
    {
        // animate the rectangle to the right
        animatableRectangle.left++;
    }
}

```

See Demo project for full details.

## License
[MIT](LICENSE)

## Support this project
- [GitHub sponsor](https://github.com/sponsors/d2phap)
- [Patreon](https://www.patreon.com/d2phap)
- [PayPal](https://www.paypal.me/d2phap)

Thanks for your gratitude and finance help!

