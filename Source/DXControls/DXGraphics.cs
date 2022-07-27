

using DirectN;
using System.Runtime.InteropServices;
using WicNet;

namespace DXControls;



/// <summary>
/// Direct2D graphics
/// </summary>
public class DXGraphics
{
    public ID2D1DeviceContext DeviceContext { get; init; }


    /// <summary>
    /// Initialize new instance of <see cref="ID2D1DeviceContext"/>.
    /// </summary>
    /// <param name="dc">The device context</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DXGraphics(ID2D1DeviceContext? dc)
    {
        if (dc == null)
        {
            throw new ArgumentNullException(nameof(dc), "ID2D1DeviceContext is null.");
        }

        DeviceContext = dc;
        DeviceContext.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
        DeviceContext.SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE);
    }


    /// <summary>
    /// Begins a new drawing session.
    /// </summary>
    public void BeginDraw()
    {
        DeviceContext.BeginDraw();
    }


    /// <summary>
    /// Begins a new drawing session.
    /// </summary>
    public void BeginDraw(Color c)
    {
        DeviceContext.BeginDraw();
        ClearBackground(c);
    }


    /// <summary>
    /// Ends the current drawing session.
    /// </summary>
    public void EndDraw()
    {
        DeviceContext.EndDraw();
    }


    /// <summary>
    /// Executes all pending drawing commands.
    /// </summary>
    public void Flush()
    {
        DeviceContext.Flush(IntPtr.Zero, IntPtr.Zero);
    }



    /// <summary>
    /// Clear the background by the given color.
    /// </summary>
    public void ClearBackground(Color color)
    {
        var value = DXHelper.ConvertColor(color);
        var ptr = value.StructureToPtr();

        DeviceContext.Clear(ptr);
        Marshal.FreeHGlobal(ptr);
    }

    
    /// <summary>
    /// Draw bitmap
    /// </summary>
    public void DrawBitmap(ID2D1Bitmap? bmp,
        D2D_RECT_F? destRect = null,
        D2D_RECT_F? srcRect = null,
        D2D1_INTERPOLATION_MODE interpolation = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR,
        float opacity = 1)
    {
        if (bmp == null) return;

        DeviceContext.DrawBitmap(bmp, opacity, interpolation, destRect, srcRect);
    }

    
    /// <summary>
    /// Draw bitmap
    /// </summary>
    public void DrawBitmap(IWICBitmapSource? bmp,
        D2D_RECT_F? destRect = null,
        D2D_RECT_F? srcRect = null,
        D2D1_INTERPOLATION_MODE interpolation = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR,
        float opacity = 1)
    {
        if (bmp == null) return;

        // create D2DBitmap from WICBitmapSource
        var bitmapProps = new D2D1_BITMAP_PROPERTIES()
        {
            pixelFormat = new D2D1_PIXEL_FORMAT()
            {
                alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED,
                format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
            },
            dpiX = 96.0f,
            dpiY = 96.0f,
        };
        var bitmapPropsPtr = bitmapProps.StructureToPtr();

        DeviceContext.CreateBitmapFromWicBitmap(bmp, bitmapPropsPtr,
            out ID2D1Bitmap? bitmap)
            .ThrowOnError();

        // draw bitmap
        DrawBitmap(bitmap, destRect, srcRect, interpolation, opacity);

        Marshal.FreeHGlobal(bitmapPropsPtr);
    }


    /// <summary>
    /// Draw a line.
    /// </summary>
    public void DrawLine(float x1, float y1, float x2, float y2,
        Color c, float strokeWidth = 1,
        D2D1_BRUSH_PROPERTIES? brushStyle = null,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        var point1 = new D2D_POINT_2F(x1, y1);
        var point2 = new D2D_POINT_2F(x2, y2);

        DrawLine(point1, point2, c, strokeWidth, brushStyle, strokeStyle);
    }


    /// <summary>
    /// Draw a line.
    /// </summary>
    public void DrawLine(D2D_POINT_2F point1, D2D_POINT_2F point2,
        Color c, float strokeWidth = 1,
        D2D1_BRUSH_PROPERTIES? brushStyle = null,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        var color = DXHelper.ConvertColor(c);

        // stroke style
        ID2D1StrokeStyle? stroke = null;
        if (strokeStyle != null)
        {
            stroke = DeviceContext.GetFactory().CreateStrokeStyle(strokeStyle.Value).Object;
        }


        // set default brush stype
        brushStyle ??= new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        };
        var brushStylePtr = brushStyle.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);

        // start drawing the line
        DeviceContext.DrawLine(point1, point2, brush, strokeWidth, stroke);


        Marshal.FreeHGlobal(brushStylePtr);
    }

    
}
