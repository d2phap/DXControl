/*
MIT License
Copyright (C) 2022 - 2025 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using DirectN;
using System.Runtime.InteropServices;

namespace D2Phap.DXControl;


/// <summary>
/// Encapsulates a Direct2D drawing surface.
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


    private bool _useAntialias = true;


    #region Public properties

    /// <summary>
    /// Gets, sets the value specifies whether smoothing (antialiasing) is applied
    /// to lines and curves and the edges of filled areas.
    /// </summary>
    public bool UseAntialias
    {
        get => _useAntialias;
        set
        {
            _useAntialias = value;
            if (DeviceContext == null) return;

            if (_useAntialias)
            {
                DeviceContext.Object.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
                DeviceContext.Object.SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE);
            }
            else
            {
                DeviceContext.Object.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);
                DeviceContext.Object.SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_ALIASED);
            }
        }
    }


    /// <summary>
    /// Gets the <see cref="ID2D1DeviceContext6"></see> object used to control the drawing.
    /// </summary>
    public IComObject<ID2D1DeviceContext6> DeviceContext { get; init; }


    /// <summary>
    /// Gets the <see cref="ID2D1Factory1"/> object wrapped into the <see cref="IComObject{T}"/> for drawing.
    /// </summary>
    public IComObject<ID2D1Factory1> D2DFactory { get; init; }


    /// <summary>
    /// Gets the <see cref="IDWriteFactory5"/> object wrapped into the <see cref="IComObject{T}"/> for drawing text.
    /// </summary>
    public IComObject<IDWriteFactory5> DWriteFactory { get; init; }

    #endregion // Public properties



    /// <summary>
    /// Initialize new instance of <see cref="DXGraphics"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public DXGraphics(IComObject<ID2D1DeviceContext6>? dc, IComObject<ID2D1Factory1>? d2dF, IComObject<IDWriteFactory5>? wf)
    {
        if (dc == null)
        {
            throw new ArgumentNullException(nameof(dc), "ID2D1DeviceContext is null.");
        }

        if (d2dF == null)
        {
            throw new ArgumentNullException(nameof(d2dF), "IComObject<ID2D1Factory> is null.");
        }

        if (wf == null)
        {
            throw new ArgumentNullException(nameof(wf), "IComObject<IDWriteFactory> is null.");
        }

        DeviceContext = dc;
        D2DFactory = d2dF;
        DWriteFactory = wf;
        UseAntialias = true;
    }



    #region Draw bitmap

    /// <summary>
    /// Draws the specified bitmap after scaling it to the size of the specified rectangle.
    /// </summary>
    public void DrawBitmap(IComObject<ID2D1Bitmap1>? bitmap,
        RectangleF? destRect = null, RectangleF? srcRect = null,
        InterpolationMode interpolation = InterpolationMode.NearestNeighbor,
        float opacity = 1)
    {
        if (bitmap is not IComObject<ID2D1Bitmap1> bmp) return;

        D2D_RECT_F? sourceRect = null;
        D2D_RECT_F? destinationRect = null;


        if (srcRect != null)
        {
            sourceRect = new D2D_RECT_F(
                srcRect.Value.Left,
                srcRect.Value.Top,
                srcRect.Value.Right,
                srcRect.Value.Bottom);
        }

        if (destRect != null)
        {
            // Note: Bitmap will be scaled if the x, y are float numbers
            // https://github.com/d2phap/ImageGlass/issues/1786
            var left = (int)destRect.Value.Left;
            var top = (int)destRect.Value.Top;
            var right = left + destRect.Value.Width;
            var bottom = top + destRect.Value.Height;

            destinationRect = new D2D_RECT_F(left, top, right, bottom);
        }


        DeviceContext.DrawBitmap(bmp, opacity, (D2D1_INTERPOLATION_MODE)interpolation, destinationRect, sourceRect);
    }


    #endregion // Draw bitmap


    #region Draw/Fill ellipse

    /// <summary>
    /// Draws the outline and paints the interior of the specified ellipse.
    /// </summary>
    public void DrawEllipse(float x, float y, float radius, Color borderColor, Color? fillColor, float strokeWidth = 1)
    {
        var rect = new RectangleF(x - radius, y - radius, radius * 2, radius * 2);

        DrawEllipse(rect, borderColor, fillColor, strokeWidth);
    }


    /// <summary>
    /// Draws the outline and paints the interior of the specified ellipse.
    /// </summary>
    public void DrawEllipse(RectangleF rect, Color borderColor, Color? fillColor, float strokeWidth = 1)
    {
        DrawEllipse(rect.X, rect.Y, rect.Width, rect.Height, borderColor, fillColor, strokeWidth);
    }


    /// <summary>
    /// Draws the outline and paints the interior of the specified ellipse.
    /// </summary>
    public void DrawEllipse(float x, float y, float width, float height, Color borderColor, Color? fillColor, float strokeWidth = 1)
    {
        var ellipse = new D2D1_ELLIPSE(x + width / 2, y + height / 2, width / 2, height / 2);

        // draw background color -----------------------------------
        if (fillColor != null)
        {
            // create solid brush for background
            var bgColor = DXHelper.FromColor(fillColor.Value);
            using var bgBrush = DeviceContext.CreateSolidColorBrush(bgColor);

            // draw background
            DeviceContext.FillEllipse(ellipse, bgBrush);
        }


        // draw ellipse border ------------------------------------
        // create solid brush for border
        var bdColor = DXHelper.FromColor(borderColor);
        using var bdBrush = DeviceContext.CreateSolidColorBrush(bdColor);

        // draw border
        DeviceContext.DrawEllipse(ellipse, bdBrush, strokeWidth);
    }

    #endregion // Draw/Fill ellipse


    #region Draw/Fill Rectangle

    /// <summary>
    /// Draws the outline and paints the interior of the specified rectangle.
    /// </summary>
    public void DrawRectangle(float x, float y, float width, float height, float radius, Color borderColor, Color? fillColor = null, float strokeWidth = 1)
    {
        DrawRectangle(new RectangleF(x, y, width, height), radius, borderColor, fillColor, strokeWidth);
    }

    /// <summary>
    /// Draws the outline and paints the interior of the specified rectangle.
    /// </summary>
    public void DrawRectangle(RectangleF rect, float radius, Color borderColor, Color? fillColor = null, float strokeWidth = 1)
    {
        // create rounded rectangle
        var roundedRect = new D2D1_ROUNDED_RECT()
        {
            rect = new D2D_RECT_F(rect.Left, rect.Top, rect.Right, rect.Bottom),
            radiusX = radius,
            radiusY = radius,
        };


        // draw rectangle background -------------------------------
        if (fillColor != null)
        {
            // create solid brush for background
            var bgColor = DXHelper.FromColor(fillColor.Value);
            using var bgBrush = DeviceContext.CreateSolidColorBrush(bgColor);

            // draw background
            DeviceContext.FillRoundedRectangle(roundedRect, bgBrush);
        }


        // draw rectangle border ------------------------------------
        // create solid brush for border
        var bdColor = DXHelper.FromColor(borderColor);
        using var bdBrush = DeviceContext.CreateSolidColorBrush(bdColor);

        // draw border
        DeviceContext.DrawRoundedRectangle(roundedRect, bdBrush, strokeWidth);
    }

    #endregion // Draw/Fill Rectangle


    #region Draw lines

    /// <summary>
    /// Draws a line between the specified points using the specified stroke style.
    /// </summary>
    public void DrawLine(float x1, float y1, float x2, float y2, Color c, float strokeWidth = 1)
    {
        DrawLine(new PointF(x1, y1), new PointF(x2, y2), c, strokeWidth);
    }

    /// <summary>
    /// Draws a line between the specified points using the specified stroke style.
    /// </summary>
    public void DrawLine(PointF p1, PointF p2, Color c, float strokeWidth = 1)
    {
        var point1 = new D2D_POINT_2F(p1.X, p1.Y);
        var point2 = new D2D_POINT_2F(p2.X, p2.Y);
        var color = DXHelper.FromColor(c);

        // create solid brush
        using var brush = DeviceContext.CreateSolidColorBrush(color);

        // start drawing the line
        DeviceContext.DrawLine(point1, point2, brush, strokeWidth);
    }

    #endregion // Draw lines


    #region Draw / Measure text

    /// <summary>
    /// Draws the specified text using the format information provided.
    /// </summary>
    public void DrawText(string text, string fontFamilyName, float fontSize, float x, float y, Color c, float? textDpi = null, StringAlignment hAlign = StringAlignment.Near, StringAlignment vAlign = StringAlignment.Near, bool isBold = false, bool isItalic = false)
    {
        var rect = new RectangleF(x, y, int.MaxValue, int.MaxValue);

        DrawText(text, fontFamilyName, fontSize, rect, c, textDpi, hAlign, vAlign, isBold, isItalic);
    }

    /// <summary>
    /// Draws the specified text using the format information provided.
    /// </summary>
    public void DrawText(string text, string fontFamilyName, float fontSize, RectangleF rect, Color c, float? textDpi = null, StringAlignment hAlign = StringAlignment.Near, StringAlignment vAlign = StringAlignment.Near, bool isBold = false, bool isItalic = false)
    {
        // backup base dpi
        DeviceContext.Object.GetDpi(out var baseDpiX, out var baseDpiY);
        var region = new D2D_RECT_F(rect.Left, rect.Top, rect.Right, rect.Bottom);

        // fix DPI
        if (textDpi != null)
        {
            var dpiFactor = textDpi.Value / 96.0f;
            DeviceContext.Object.SetDpi(textDpi.Value, textDpi.Value);

            fontSize += dpiFactor;

            region.left /= dpiFactor;
            region.top /= dpiFactor;
            region.right /= dpiFactor;
            region.bottom /= dpiFactor;
        }

        // format text
        var fontWeight = isBold
            ? DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_BOLD
            : DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL;
        var fontStyle = isItalic
            ? DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_ITALIC
            : DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL;
        var horzAlign = hAlign switch
        {
            StringAlignment.Near => DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING,
            StringAlignment.Center => DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
            StringAlignment.Far => DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_TRAILING,
            _ => DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING,
        };
        var vertAlign = vAlign switch
        {
            StringAlignment.Near => DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR,
            StringAlignment.Center => DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,
            StringAlignment.Far => DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_FAR,
            _ => DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR,
        };

        using var format = DWriteFactory.CreateTextFormat(fontFamilyName, fontSize,
            weight: fontWeight, style: fontStyle);
        format.Object.SetTextAlignment(horzAlign);
        format.Object.SetParagraphAlignment(vertAlign);
        format.Object.SetWordWrapping(DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WRAP);


        // create solid brush
        var color = DXHelper.FromColor(c);
        using var brush = DeviceContext.CreateSolidColorBrush(color);


        // draw text
        DeviceContext.DrawText(text, format, region, brush, D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);

        // restore base dpi
        if (textDpi != null)
        {
            DeviceContext.Object.SetDpi(baseDpiX, baseDpiY);
        }
    }

    /// <summary>
    /// Measure text.
    /// </summary>
    public SizeF MeasureText(string text, string fontFamilyName, float fontSize, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue, float textDpi = 96, bool isBold = false, bool isItalic = false)
    {
        var size = new SizeF(maxWidth, maxHeight);

        return MeasureText(text, fontFamilyName, fontSize, size, textDpi, isBold, isItalic);
    }

    /// <summary>
    /// Measure text.
    /// </summary>
    public SizeF MeasureText(string text, string fontFamilyName, float fontSize, SizeF size, float textDpi = 96, bool isBold = false, bool isItalic = false)
    {
        // fix DPI
        var dpiScale = textDpi / 96.0f;
        fontSize = (fontSize + dpiScale) * dpiScale;

        // format text
        var fontWeight = isBold
            ? DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_BOLD
            : DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL;
        var fontStyle = isItalic
            ? DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_ITALIC
            : DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL;

        using var format = DWriteFactory.CreateTextFormat(fontFamilyName, fontSize,
            weight: fontWeight, style: fontStyle,
            stretch: DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL);
        format.Object.SetWordWrapping(DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WRAP);

        // create layout
        using var layout = DWriteFactory.CreateTextLayout(format, text, text.Length, size.Width, size.Height);

        // measure text
        layout.Object.GetMetrics(out var metrics);

        return new SizeF(metrics.width, metrics.height);
    }

    #endregion // Draw / Measure text


    #region Draw / Fill Geometry

    /// <summary>
    /// Get geometry from a combined 2 rectangles.
    /// </summary>
    public IComObject<ID2D1Geometry>? GetCombinedRectanglesGeometry(RectangleF rect1, RectangleF rect2,
        float rect1Radius, float rect2Radius, D2D1_COMBINE_MODE combineMode)
    {
        // create rounded rectangle 1
        var roundedRect1 = new D2D1_ROUNDED_RECT()
        {
            rect = new D2D_RECT_F(rect1.Left, rect1.Top, rect1.Right, rect1.Bottom),
            radiusX = rect1Radius,
            radiusY = rect1Radius,
        };

        // create rounded rectangle 2
        var roundedRect2 = new D2D1_ROUNDED_RECT()
        {
            rect = new D2D_RECT_F(rect2.Left, rect2.Top, rect2.Right, rect2.Bottom),
            radiusX = rect2Radius,
            radiusY = rect2Radius,
        };


        // create geometries
        var shape1Geo = D2DFactory.CreateRoundedRectangleGeometry(roundedRect1);
        var shape22Geo = D2DFactory.CreateRoundedRectangleGeometry(roundedRect2);

        // create path geometry to get the combined shape
        var pathGeo = D2DFactory.CreatePathGeometry();
        pathGeo.Object.Open(out var pathGeoSink).ThrowOnError();


        // combine 2 geometry shapes
        shape1Geo.Object.CombineWithGeometry(shape22Geo.Object, combineMode, IntPtr.Zero, 0, pathGeoSink).ThrowOnError();

        pathGeoSink.Close();
        Marshal.ReleaseComObject(pathGeoSink);
        shape1Geo.Dispose();
        shape22Geo.Dispose();

        return pathGeo;
    }


    /// <summary>
    /// Get geometry from a combined 2 ellipses.
    /// </summary>
    public IComObject<ID2D1Geometry>? GetCombinedEllipsesGeometry(RectangleF rect1, RectangleF rect2, D2D1_COMBINE_MODE combineMode)
    {
        // create ellipse 1
        var ellipse1 = new D2D1_ELLIPSE(rect1.X + rect1.Width / 2, rect1.Y + rect1.Height / 2, rect1.Width / 2, rect1.Height / 2);

        // create ellipse 2
        var ellipse2 = new D2D1_ELLIPSE(rect2.X + rect2.Width / 2, rect2.Y + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);


        // create geometries
        var shape1Geo = D2DFactory.CreateEllipseGeometry(ellipse1);
        var shape2Geo = D2DFactory.CreateEllipseGeometry(ellipse2);

        // create path geometry to get the combined shape
        var pathGeo = D2DFactory.CreatePathGeometry();
        pathGeo.Object.Open(out var pathGeoSink).ThrowOnError();


        // combine 2 geometry shapes
        shape1Geo.Object.CombineWithGeometry(shape2Geo.Object, combineMode, IntPtr.Zero, 0, pathGeoSink).ThrowOnError();

        pathGeoSink.Close();
        Marshal.ReleaseComObject(pathGeoSink);
        shape1Geo.Dispose();
        shape2Geo.Dispose();


        return pathGeo;
    }


    /// <summary>
    /// Draw geometry.
    /// </summary>
    public void DrawGeometry(IComObject<ID2D1Geometry>? geo, Color borderColor, Color? fillColor = null, float strokeWidth = 1f)
    {
        if (geo is not IComObject<ID2D1Geometry> geometry) return;

        // draw background color -----------------------------------
        if (fillColor != null)
        {
            var bgColor = DXHelper.FromColor(fillColor.Value);
            using var bgBrush = DeviceContext.CreateSolidColorBrush(bgColor);

            // fill the combined geometry
            DeviceContext.FillGeometry(geometry, bgBrush);
        }


        // draw border color ----------------------------------------
        if (borderColor != Color.Transparent)
        {
            var bdColor = DXHelper.FromColor(borderColor);
            using var borderBrush = DeviceContext.CreateSolidColorBrush(bdColor);

            // draw the combined geometry
            DeviceContext.DrawGeometry(geometry, borderBrush, strokeWidth);
        }
    }

    #endregion // Draw / Fill Geometry


    #region Others

    /// <summary>
    /// Executes all pending drawing commands.
    /// </summary>
    public void Flush()
    {
        DeviceContext.Object.Flush(IntPtr.Zero, IntPtr.Zero);
    }


    /// <summary>
    /// Clears the drawing area to the specified color.
    /// </summary>
    public void ClearBackground(Color color)
    {
        var d3Color = DXHelper.FromColor(color);

        DeviceContext.Clear(d3Color);
    }


    #endregion // Others


}
