/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using DirectN;
using System.Drawing;
using System.Runtime.InteropServices;

namespace D2Phap;


/// <summary>
/// Encapsulates a Direct2D drawing surface.
/// </summary>
public class D2DGraphics : IGraphics
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

    ~D2DGraphics()
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
                DeviceContext.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
                DeviceContext.SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE);
            }
            else
            {
                DeviceContext.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);
                DeviceContext.SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_ALIASED);
            }
        }
    }


    /// <summary>
    /// Gets the <see cref="ID2D1DeviceContext"></see> object used to control the drawing.
    /// </summary>
    public ID2D1DeviceContext DeviceContext { get; init; }


    /// <summary>
    /// Gets the <see cref="ID2D1Factory"/> object wrapped into the <see cref="IComObject{T}"/> for drawing.
    /// </summary>
    public IComObject<ID2D1Factory> D2DFactory { get; init; }


    /// <summary>
    /// Gets the <see cref="IDWriteFactory"/> object wrapped into the <see cref="IComObject{T}"/> for drawing text.
    /// </summary>
    public IComObject<IDWriteFactory> DWriteFactory { get; init; }

    #endregion // Public properties



    /// <summary>
    /// Initialize new instance of <see cref="D2DGraphics"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public D2DGraphics(ID2D1DeviceContext? dc, IComObject<ID2D1Factory>? d2dF, IComObject<IDWriteFactory>? wf)
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

    public void DrawBitmap(object? bitmap, RectangleF? destRect = null, RectangleF? srcRect = null, InterpolationMode interpolation = InterpolationMode.NearestNeighbor, float opacity = 1)
    {
        if (bitmap is not ID2D1Bitmap bmp) return;

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
            destinationRect = new D2D_RECT_F(
                destRect.Value.Left,
                destRect.Value.Top,
                destRect.Value.Right,
                destRect.Value.Bottom);
        }

        DeviceContext.DrawBitmap(bmp, opacity, (D2D1_INTERPOLATION_MODE)interpolation, destinationRect, sourceRect);
    }


    #endregion // Draw bitmap


    #region Draw/Fill ellipse

    public void DrawEllipse(float x, float y, float radius, Color borderColor, Color? fillColor, float strokeWidth = 1)
    {
        var rect = new RectangleF(x - radius, y - radius, radius * 2, radius * 2);

        DrawEllipse(rect, borderColor, fillColor, strokeWidth);
    }


    public void DrawEllipse(RectangleF rect, Color borderColor, Color? fillColor, float strokeWidth = 1)
    {
        DrawEllipse(rect.X, rect.Y, rect.Width, rect.Height, borderColor, fillColor, strokeWidth);
    }


    public void DrawEllipse(float x, float y, float width, float height, Color borderColor, Color? fillColor, float strokeWidth = 1)
    {
        var ellipse = new D2D1_ELLIPSE(x + width / 2, y + height / 2, width / 2, height / 2);

        // draw background color -----------------------------------
        if (fillColor != null)
        {
            // create solid brush for background
            var bgColor = DXHelper.FromColor(fillColor.Value);
            var bgBrushStylePtr = new D2D1_BRUSH_PROPERTIES()
            {
                opacity = 1f,
            }.StructureToPtr();
            DeviceContext.CreateSolidColorBrush(bgColor, bgBrushStylePtr, out var bgBrush);

            // draw background
            DeviceContext.FillEllipse(ellipse, bgBrush);
            Marshal.FreeHGlobal(bgBrushStylePtr);
        }


        // draw ellipse border ------------------------------------
        // create solid brush for border
        var bdColor = DXHelper.FromColor(borderColor);
        var bdBrushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(bdColor, bdBrushStylePtr, out var bdBrush);

        // draw border
        DeviceContext.DrawEllipse(ellipse, bdBrush, strokeWidth);
        Marshal.FreeHGlobal(bdBrushStylePtr);
    }

    #endregion // Draw/Fill ellipse


    #region Draw lines

    public void DrawLine(float x1, float y1, float x2, float y2, Color c, float strokeWidth = 1)
    {
        DrawLine(new PointF(x1, y1), new PointF(x2, y2), c, strokeWidth);
    }

    public void DrawLine(PointF p1, PointF p2, Color c, float strokeWidth = 1)
    {
        var point1 = new D2D_POINT_2F(p1.X, p1.Y);
        var point2 = new D2D_POINT_2F(p2.X, p2.Y);
        var color = DXHelper.FromColor(c);

        // create solid brush
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);


        // start drawing the line
        DeviceContext.DrawLine(point1, point2, brush, strokeWidth);

        Marshal.FreeHGlobal(brushStylePtr);
    }

    #endregion // Draw lines


    #region Draw/Fill Rectangle

    public void DrawRectangle(float x, float y, float width, float height, float radius, Color borderColor, Color? fillColor = null, float strokeWidth = 1)
    {
        DrawRectangle(new RectangleF(x, y, width, height), radius, borderColor, fillColor, strokeWidth);
    }


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
            var bgBrushStylePtr = new D2D1_BRUSH_PROPERTIES()
            {
                opacity = 1f,
            }.StructureToPtr();
            DeviceContext.CreateSolidColorBrush(bgColor, bgBrushStylePtr, out var bgBrush);

            // draw background
            DeviceContext.FillRoundedRectangle(roundedRect, bgBrush);
            Marshal.FreeHGlobal(bgBrushStylePtr);
        }


        // draw rectangle border ------------------------------------
        // create solid brush for border
        var bdColor = DXHelper.FromColor(borderColor);
        var bdBrushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(bdColor, bdBrushStylePtr, out var bdBrush);

        // draw border
        DeviceContext.DrawRoundedRectangle(roundedRect, bdBrush, strokeWidth);
        Marshal.FreeHGlobal(bdBrushStylePtr);
    }

    #endregion // Draw/Fill Rectangle


    #region Draw / Fill Geometry

    public GeometryObject GetCombinedRectanglesGeometry(RectangleF rect1, RectangleF rect2,
        float rect1Radius, float rect2Radius, CombineMode combineMode)
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
        shape1Geo.Object.CombineWithGeometry(shape22Geo.Object, (D2D1_COMBINE_MODE)combineMode, IntPtr.Zero, 0, pathGeoSink).ThrowOnError();

        pathGeoSink.Close();
        Marshal.ReleaseComObject(pathGeoSink);
        shape1Geo.Dispose();
        shape22Geo.Dispose();

        return new GeometryObject() { D2DGeometry = pathGeo };
    }


    public GeometryObject GetCombinedEllipsesGeometry(RectangleF rect1, RectangleF rect2, CombineMode combineMode)
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
        shape1Geo.Object.CombineWithGeometry(shape2Geo.Object, (D2D1_COMBINE_MODE)combineMode, IntPtr.Zero, 0, pathGeoSink).ThrowOnError();

        pathGeoSink.Close();
        Marshal.ReleaseComObject(pathGeoSink);
        shape1Geo.Dispose();
        shape2Geo.Dispose();


        return new GeometryObject() { D2DGeometry = pathGeo };
    }


    public void DrawGeometry(GeometryObject geoObj, Color borderColor, Color? fillColor = null, float strokeWidth = 1f)
    {
        if (geoObj.D2DGeometry is not IComObject<ID2D1Geometry> geometry) return;

        // draw background color -----------------------------------
        if (fillColor != null)
        {
            var bgColor = DXHelper.FromColor(fillColor.Value);
            var bgBrushStylePtr = new D2D1_BRUSH_PROPERTIES()
            {
                opacity = 1f,
            }.StructureToPtr();
            DeviceContext.CreateSolidColorBrush(bgColor, bgBrushStylePtr, out var bgBrush);

            // fill the combined geometry
            DeviceContext.FillGeometry(geometry.Object, bgBrush);

            Marshal.FreeHGlobal(bgBrushStylePtr);
        }


        // draw border color ----------------------------------------
        if (borderColor != Color.Transparent)
        {
            var bdColor = DXHelper.FromColor(borderColor);
            var borderBrushStylePtr = new D2D1_BRUSH_PROPERTIES()
            {
                opacity = 1f,
            }.StructureToPtr();
            DeviceContext.CreateSolidColorBrush(bdColor, borderBrushStylePtr, out var borderBrush);

            // draw the combined geometry
            DeviceContext.DrawGeometry(geometry.Object, borderBrush, strokeWidth);

            Marshal.FreeHGlobal(borderBrushStylePtr);
        }
    }

    #endregion // Draw / Fill Geometry


    #region Draw / Measure text

    public void DrawText(string text, string fontFamilyName, float fontSize, float x, float y, Color c, float? textDpi = null, StringAlignment hAlign = StringAlignment.Near, StringAlignment vAlign = StringAlignment.Near, bool isBold = false, bool isItalic = false)
    {
        var rect = new RectangleF(x, y, int.MaxValue, int.MaxValue);

        DrawText(text, fontFamilyName, fontSize, rect, c, textDpi, hAlign, vAlign, isBold, isItalic);
    }


    public void DrawText(string text, string fontFamilyName, float fontSize, RectangleF rect, Color c, float? textDpi = null, StringAlignment hAlign = StringAlignment.Near, StringAlignment vAlign = StringAlignment.Near, bool isBold = false, bool isItalic = false)
    {
        // backup base dpi
        DeviceContext.GetDpi(out var baseDpiX, out var baseDpiY);
        var region = new D2D_RECT_F(rect.Left, rect.Top, rect.Right, rect.Bottom);

        // fix DPI
        if (textDpi != null)
        {
            var dpiFactor = textDpi.Value / 96.0f;
            DeviceContext.SetDpi(textDpi.Value, textDpi.Value);

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
        var brushStylePtr = new D2D1_BRUSH_PROPERTIES()
        {
            opacity = 1f,
        }.StructureToPtr();
        DeviceContext.CreateSolidColorBrush(color, brushStylePtr, out var brush);


        // draw text
        DeviceContext.DrawText(text, format.Object, region, brush, D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
        Marshal.FreeHGlobal(brushStylePtr);

        // restore base dpi
        if (textDpi != null)
        {
            DeviceContext.SetDpi(baseDpiX, baseDpiY);
        }
    }


    public SizeF MeasureText(string text, string fontFamilyName, float fontSize, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue, float textDpi = 96, bool isBold = false, bool isItalic = false)
    {
        var size = new SizeF(maxWidth, maxHeight);

        return MeasureText(text, fontFamilyName, fontSize, size, textDpi, isBold, isItalic);
    }


    public SizeF MeasureText(string text, string fontFamilyName, float fontSize, SizeF size, float textDpi = 96, bool isBold = false, bool isItalic = false)
    {
        // fix DPI
        var dpiScale = textDpi / 96.0f;
        fontSize += fontSize * dpiScale;

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


    #region Others

    public void Flush()
    {
        DeviceContext.Flush(IntPtr.Zero, IntPtr.Zero);
    }


    public void ClearBackground(Color color)
    {
        var ptr = DXHelper.FromColor(color).StructureToPtr();

        DeviceContext.Clear(ptr);
        Marshal.FreeHGlobal(ptr);
    }


    #endregion // Others

}
