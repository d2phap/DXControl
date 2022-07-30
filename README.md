# DXControl

- A WinForms hybrid controls that supports Direct2D and GDI+ drawing thanks to [DirectN](https://github.com/smourier/DirectN) and [WicNet](https://github.com/smourier/WicNet).
- This control is used in [ImageGlass](https://github.com/d2phap/ImageGlass) software since version 9.0.

![Nuget](https://img.shields.io/nuget/dt/D2Phap.DXControl?color=%2300a8d6&logo=nuget)


## Resource links
- Project url: [https://github.com/d2phap/DXControl](https://github.com/d2phap/DXControl)
- Nuget package: [https://www.nuget.org/packages/D2Phap.DXControl](https://www.nuget.org/packages/D2Phap.DXControl)
- About: [https://imageglass.org/about](https://imageglass.org/about)

## Features
- High performance drawing using Direct2D.
- Names and types are exactly the same as the native concepts of Direct2D (interfaces, enums, structures, constants, methods, arguments, guids, etc...). So you can read the official documentation, use existing C/C++ samples, and start coding with .NET right away.
- All native COM interfaces are generated as .NET (COM) interfaces, this makes .NET programming easier, but they are not strictly needed.
- Option to draw by GDI+.
- Supports animation drawing for both Direct2D and GDI+.

## Requirements:
- .NET 6.0

## Installation
Run the command
```bash
Install-Package D2Phap.DXControl
```


## Example
Draw 2 rectangles by Direct2D and GDI+ graphics, then animate it to the right side.

```cs
using D2phap;

// create a WinForms custom control that extends from DXControl
public class DXCanvas : DXControl
{
    private D2D_RECT_F animatableRectangle = new(100f, 100f, new(400, 200));

    public DXCanvas()
    {
        EnableAnimation = true;
    }

    // use Direct2D graphics
    protected override void OnRender(DXGraphics g)
    {
        // draw a yellow rectangle with green border
        g.FillRectangle(rectText, _D3DCOLORVALUE.FromCOLORREF(_D3DCOLORVALUE.Yellow.Int32Value, 100));
        g.DrawRectangle(rectText, _D3DCOLORVALUE.Green);
    }


    // Use GDI+ graphics
    protected override void OnRender(Graphics g)
    {
        // draw a yellow rectangle with green border
        using var pen = new Pen(Color.Red, 5);
        g.DrawRectangle(pen, new Rectangle((int)rectText.left, (int)rectText.top - 50, (int)rectText.Width, (int)rectText.Height));
    }


    // Update frame logics for animation
    protected override void OnFrame(FrameEventArgs e)
    {
        // animate the rectangle to the right
        animatableRectangle.left++;
    }
}

See Demo project for full details.
```

## License
[MIT](LICENSE)

## Support this project
- [GitHub sponsor](https://github.com/sponsors/d2phap)
- [Patreon](https://www.patreon.com/d2phap)
- [PayPal](https://www.paypal.me/d2phap)
- [Wire Transfers](https://donorbox.org/imageglass)

Thanks for your gratitude and finance help!

