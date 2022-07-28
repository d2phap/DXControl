

using DirectN;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DXControls;



/// <summary>
/// Direct2D graphics
/// </summary>
public class DXGraphics : IDisposable
{

    #region IDisposable Disposing

    private bool _isDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.

            DWriteFactory.Dispose();
            Marshal.ReleaseComObject(DeviceContext);
        }

        // Free any unmanaged objects here.
        _isDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DXGraphics()
    {
        Dispose(false);
    }

    #endregion



    public ID2D1DeviceContext DeviceContext { get; init; }
    public IComObject<IDWriteFactory> DWriteFactory { get; init; }


    /// <summary>
    /// Initialize new instance of <see cref="ID2D1DeviceContext"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public DXGraphics(ID2D1DeviceContext? dc, IComObject<IDWriteFactory>? wf)
    {
        if (dc == null)
        {
            throw new ArgumentNullException(nameof(dc), "ID2D1DeviceContext is null.");
        }

        if (wf == null)
        {
            throw new ArgumentNullException(nameof(wf), "IComObject<IDWriteFactory> is null.");
        }

        DeviceContext = dc;
        DeviceContext.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
        DeviceContext.SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE);

        DWriteFactory = wf;
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
    public void BeginDraw(_D3DCOLORVALUE c)
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
    public void ClearBackground(_D3DCOLORVALUE color)
    {
        var ptr = color.StructureToPtr();

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

    #endregion


    #region Draw Line

    /// <summary>
    /// Draw a line.
    /// </summary>
    public void DrawLine(float x1, float y1, float x2, float y2,
        _D3DCOLORVALUE color, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        DrawLine(x1, x2, y1, y2, color, strokeWidth, strokeStyle);
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
    public void DrawLine(D2D_POINT_2F point1, D2D_POINT_2F point2, _D3DCOLORVALUE color,
        float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
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
    public void DrawRectangle(D2D_RECT_F rect, _D3DCOLORVALUE color, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
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
    public void FillRectangle(D2D_RECT_F rect, _D3DCOLORVALUE color)
    {
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
    public void DrawRoundedRectangle(D2D_RECT_F rect, float radius, _D3DCOLORVALUE color,
        float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        DrawRoundedRectangle(rect, radius, radius, color, strokeWidth, strokeStyle);
    }


    /// <summary>
    /// Draw a rounded rectangle.
    /// </summary>
    public void DrawRoundedRectangle(D2D_RECT_F rect, float radiusX, float radiusY,
        _D3DCOLORVALUE color, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
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
    public void FillRoundedRectangle(D2D_RECT_F rect, float radius, _D3DCOLORVALUE color)
    {
        FillRoundedRectangle(rect, radius, radius, color);
    }


    /// <summary>
    /// Fill a rounded rectangle.
    /// </summary>
    public void FillRoundedRectangle(D2D_RECT_F rect, float radiusX, float radiusY, _D3DCOLORVALUE color)
    {
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
    public void DrawEllipse(D2D_RECT_F rect, _D3DCOLORVALUE color, float strokeWidth = 1,
        D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
        DrawEllipse(rect.left, rect.top, rect.Width, rect.Height, color, strokeWidth, strokeStyle);
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
        _D3DCOLORVALUE color, float strokeWidth = 1, D2D1_STROKE_STYLE_PROPERTIES? strokeStyle = null)
    {
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
    public void FillEllipse(D2D_RECT_F rect, _D3DCOLORVALUE color)
    {
        FillEllipse(rect.left, rect.top, rect.Width / 2, rect.Height / 2, color);
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
    public void FillEllipse(float x, float y, float width, float height, _D3DCOLORVALUE color)
    {
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


    #region Draw text

    /// <summary>
    /// Measure text.
    /// </summary>
    public D2D_SIZE_F MeasureText(string text, string fontFamilyName, float fontSize,
        DWRITE_FONT_WEIGHT fontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_REGULAR,
        float textDpi = 96.0f)
    {
        // fix DPI
        fontSize *= textDpi / 96.0f;

        using var format = DWriteFactory.CreateTextFormat(fontFamilyName, fontSize,
            weight: fontWeight);


        using var layout = DWriteFactory.CreateTextLayout(format, text);
        layout.Object.GetMetrics(out var metrics);

        return new D2D_SIZE_F(metrics.width, metrics.height);
    }


    /// <summary>
    /// Draw text.
    /// </summary>
    public void DrawText(string text, string fontFamilyName, float fontSize,
        D2D_RECT_F rect, _D3DCOLORVALUE color, float? textDpi = null,
        DWRITE_TEXT_ALIGNMENT hAlign = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING,
        DWRITE_PARAGRAPH_ALIGNMENT vAlign = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR,
        DWRITE_FONT_WEIGHT fontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_REGULAR,
        D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT)
    {
        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);


        // draw text
        DrawText(text, fontFamilyName, fontSize, rect, brush, textDpi, hAlign, vAlign, fontWeight, options);

        Marshal.FreeHGlobal(brushStylePtr);
    }


    /// <summary>
    /// Draw text.
    /// </summary>
    public void DrawText(string text, string fontFamilyName, float fontSize,
        D2D_RECT_F rect, ID2D1Brush brush, float? textDpi = null,
        DWRITE_TEXT_ALIGNMENT hAlign = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING,
        DWRITE_PARAGRAPH_ALIGNMENT vAlign = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR,
        DWRITE_FONT_WEIGHT fontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_REGULAR,
        D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT)
    {
        // backup base dpi
        DeviceContext.GetDpi(out var baseDpiX, out var baseDpiY);

        // fix DPI
        if (textDpi != null)
        {
            var dpiFactor = textDpi.Value / 96.0f;
            DeviceContext.SetDpi(textDpi.Value, textDpi.Value);

            fontSize *= dpiFactor;

            rect.left /= dpiFactor;
            rect.top /= dpiFactor;
            rect.right /= dpiFactor;
            rect.bottom /= dpiFactor;
        }

        // format text
        using var format = DWriteFactory.CreateTextFormat(fontFamilyName, fontSize,
            weight: fontWeight);
        format.Object.SetTextAlignment(hAlign);
        format.Object.SetParagraphAlignment(vAlign);

        
        DeviceContext.DrawText(text, format.Object, rect, brush, options);

        // restore base dpi
        if (textDpi != null)
        {
            DeviceContext.SetDpi(baseDpiX, baseDpiY);
        }
    }

    #endregion


}
