

using DirectN;
using System.Runtime.InteropServices;

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


    #region Draw Bitmap

    /// <summary>
    /// Draw bitmap
    /// </summary>
    public void DrawBitmap(ID2D1Bitmap? bmp,
        D2D_RECT_F? destRect = null,
        D2D_RECT_F? srcRect = null,
        D2D1_INTERPOLATION_MODE interpolation = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR,
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
        D2D1_INTERPOLATION_MODE interpolation = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR,
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

    #endregion


    #region Draw Line

    /// <summary>
    /// Draw a line.
    /// </summary>
    public void DrawLine(float x1, float y1, float x2, float y2,
        Color c, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        DrawLine(x1, x2, y1, y2, c, strokeWidth, strokeStyle);
    }

    /// <summary>
    /// Draw a line.
    /// </summary>
    public void DrawLine(float x1, float y1, float x2, float y2,
        ID2D1Brush brush, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        var point1 = new D2D_POINT_2F(x1, y1);
        var point2 = new D2D_POINT_2F(x2, y2);

        DrawLine(point1, point2, brush, strokeWidth, strokeStyle);
    }


    /// <summary>
    /// Draw a line.
    /// </summary>
    public void DrawLine(D2D_POINT_2F point1, D2D_POINT_2F point2, Color c,
        float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        var color = DXHelper.ConvertColor(c);

        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);


        // start drawing the line
        DrawLine(point1, point2, brush, strokeWidth, strokeStyle);

        Marshal.FreeHGlobal(brushStylePtr);
    }


    /// <summary>
    /// Draw a line.
    /// </summary>
    public void DrawLine(D2D_POINT_2F point1, D2D_POINT_2F point2, ID2D1Brush brush,
        float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        // stroke style
        ID2D1StrokeStyle? stroke = null;
        if (strokeStyle != null)
        {
            stroke = DeviceContext.GetFactory().CreateStrokeStyle(strokeStyle.Value).Object;
        }

        // start drawing the line
        DeviceContext.DrawLine(point1, point2, brush, strokeWidth, stroke);
    }

    #endregion


    #region Draw/Fill Rectangle

    /// <summary>
    /// Draw a rectangle.
    /// </summary>
    public void DrawRectangle(D2D_RECT_F rect, Color c, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        var color = DXHelper.ConvertColor(c);

        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);


        // stroke style
        ID2D1StrokeStyle? stroke = null;
        if (strokeStyle != null)
        {
            stroke = DeviceContext.GetFactory().CreateStrokeStyle(strokeStyle.Value).Object;
        }

        DeviceContext.DrawRectangle(rect, brush, strokeWidth, stroke);

        Marshal.FreeHGlobal(brushStylePtr);
    }


    /// <summary>
    /// Draw a rectangle.
    /// </summary>
    public void DrawRectangle(D2D_RECT_F rect, ID2D1Brush brush, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        // stroke style
        ID2D1StrokeStyle? stroke = null;
        if (strokeStyle != null)
        {
            stroke = DeviceContext.GetFactory().CreateStrokeStyle(strokeStyle.Value).Object;
        }

        DeviceContext.DrawRectangle(rect, brush, strokeWidth, stroke);
    }


    /// <summary>
    /// Fill a rectangle.
    /// </summary>
    public void FillRectangle(D2D_RECT_F rect, Color c)
    {
        var color = DXHelper.ConvertColor(c);

        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);

        // fill retangle
        FillRectangle(rect, brush);

        Marshal.FreeHGlobal(brushStylePtr);
    }


    /// <summary>
    /// Fill a rectangle.
    /// </summary>
    public void FillRectangle(D2D_RECT_F rect, ID2D1Brush brush)
    {
        // fill retangle
        DeviceContext.FillRectangle(rect, brush);
    }


    /// <summary>
    /// Draw a rounded rectangle.
    /// </summary>
    public void DrawRoundedRectangle(D2D_RECT_F rect, float radius, Color c,
        float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        DrawRoundedRectangle(rect, radius, radius, c, strokeWidth, strokeStyle);
    }


    /// <summary>
    /// Draw a rounded rectangle.
    /// </summary>
    public void DrawRoundedRectangle(D2D_RECT_F rect, float radiusX, float radiusY, Color c,
        float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        var color = DXHelper.ConvertColor(c);

        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);

        // draw rectangle
        DrawRoundedRectangle(rect, radiusX, radiusY, brush, strokeWidth, strokeStyle);

        Marshal.FreeHGlobal(brushStylePtr);
    }


    /// <summary>
    /// Draw a rounded rectangle.
    /// </summary>
    public void DrawRoundedRectangle(D2D_RECT_F rect, float radius, ID2D1Brush brush,
        float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        DrawRoundedRectangle(rect, radius, radius, brush, strokeWidth, strokeStyle);
    }


    /// <summary>
    /// Draw a rounded rectangle.
    /// </summary>
    public void DrawRoundedRectangle(D2D_RECT_F rect, float radiusX, float radiusY,
        ID2D1Brush brush, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        // create rounded rectangle
        var roundRect = new D2D1_ROUNDED_RECT()
        {
            rect = rect,
            radiusX = radiusX,
            radiusY = radiusY,
        };

        // stroke style
        ID2D1StrokeStyle? stroke = null;
        if (strokeStyle != null)
        {
            stroke = DeviceContext.GetFactory().CreateStrokeStyle(strokeStyle.Value).Object;
        }

        DeviceContext.DrawRoundedRectangle(roundRect, brush, strokeWidth, stroke);
    }


    /// <summary>
    /// Fill a rounded rectangle.
    /// </summary>
    public void FillRoundedRectangle(D2D_RECT_F rect, float radius, Color c)
    {
        FillRoundedRectangle(rect, radius, radius, c);
    }


    /// <summary>
    /// Fill a rounded rectangle.
    /// </summary>
    public void FillRoundedRectangle(D2D_RECT_F rect, float radiusX, float radiusY, Color c)
    {
        var color = DXHelper.ConvertColor(c);

        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);

        // fill rectangle
        FillRoundedRectangle(rect, radiusX, radiusY, brush);

        Marshal.FreeHGlobal(brushStylePtr);
    }


    /// <summary>
    /// Fill a rounded rectangle.
    /// </summary>
    public void FillRoundedRectangle(D2D_RECT_F rect, float radius, ID2D1Brush brush)
    {
        FillRoundedRectangle(rect, radius, brush);
    }


    /// <summary>
    /// Fill a rounded rectangle.
    /// </summary>
    public void FillRoundedRectangle(D2D_RECT_F rect, float radiusX, float radiusY, ID2D1Brush brush)
    {
        // create rounded rectangle
        var roundRect = new D2D1_ROUNDED_RECT()
        {
            rect = rect,
            radiusX = radiusX,
            radiusY = radiusY,
        };

        DeviceContext.FillRoundedRectangle(roundRect, brush);
    }


    #endregion


    #region Draw/Fill ellipse

    /// <summary>
    /// Draw an ellipse.
    /// </summary>
    public void DrawEllipse(D2D_RECT_F rect, Color c, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        DrawEllipse(rect.left, rect.top, rect.Width, rect.Height, c, strokeWidth, strokeStyle);
    }


    /// <summary>
    /// Draw an ellipse.
    /// </summary>
    public void DrawEllipse(D2D_RECT_F rect, ID2D1Brush brush, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        DrawEllipse(rect.left, rect.top, rect.Width / 2, rect.Height / 2, brush, strokeWidth, strokeStyle);
    }


    /// <summary>
    /// Draw an ellipse.
    /// </summary>
    public void DrawEllipse(float x, float y, float width, float height,
        Color c, float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        var color = DXHelper.ConvertColor(c);

        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);

        // draw ellipse
        DrawEllipse(x, y, width / 2, height / 2, brush, strokeWidth, strokeStyle);

        Marshal.FreeHGlobal(brushStylePtr);
    }


    /// <summary>
    /// Draw an ellipse.
    /// </summary>
    public void DrawEllipse(float x, float y, float radiusX, float radiusY,
        ID2D1Brush brush, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        var ellipse = new D2D1_ELLIPSE(x, y, radiusX, radiusY);

        // stroke style
        ID2D1StrokeStyle? stroke = null;
        if (strokeStyle != null)
        {
            stroke = DeviceContext.GetFactory().CreateStrokeStyle(strokeStyle.Value).Object;
        }

        DeviceContext.DrawEllipse(ellipse, brush, strokeWidth, stroke);
    }


    /// <summary>
    /// Fill an ellipse.
    /// </summary>
    public void FillEllipse(D2D_RECT_F rect, Color c)
    {
        FillEllipse(rect.left, rect.top, rect.Width / 2, rect.Height / 2, c);
    }


    /// <summary>
    /// Fill an ellipse.
    /// </summary>
    public void FillEllipse(D2D_RECT_F rect, ID2D1Brush brush)
    {
        FillEllipse(rect.left, rect.top, rect.Width / 2, rect.Height / 2, brush);
    }


    /// <summary>
    /// Fill an ellipse.
    /// </summary>
    public void FillEllipse(float x, float y, float width, float height, Color c)
    {
        var color = DXHelper.ConvertColor(c);

        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);

        // fill ellipse
        FillEllipse(x, y, width / 2, height / 2, brush);

        Marshal.FreeHGlobal(brushStylePtr);
    }


    /// <summary>
    /// Fill an ellipse.
    /// </summary>
    public void FillEllipse(float x, float y, float radiusX, float radiusY, ID2D1Brush brush)
    {
        var ellipse = new D2D1_ELLIPSE(x, y, radiusX, radiusY);

        DeviceContext.FillEllipse(ellipse, brush);
    }

    #endregion

}
