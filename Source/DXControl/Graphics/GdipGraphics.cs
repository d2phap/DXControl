/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace D2Phap;


/// <summary>
/// Encapsulates a GDI+ drawing surface.
/// </summary>
public class GdipGraphics : IGraphics
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

    ~GdipGraphics()
    {
        Dispose(false);
    }

    #endregion


    private bool _useAntialias = true;
    private Graphics _g;


    #region Public properties

    /// <summary>
    /// Gets, sets original GDI+ graphics object
    /// </summary>
    public Graphics Graphics
    {
        get => _g;
        set
        {
            _g = value;

            // update _g.SmoothingMode
            UseAntialias = _useAntialias;
        }
    }


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
            _g.SmoothingMode = value ? SmoothingMode.AntiAlias : SmoothingMode.Default;
            _g.TextRenderingHint = TextRenderingHint.AntiAlias;
        }
    }

    #endregion // Public properties



    /// <summary>
    /// Initialize new instance of <see cref="GdipGraphics"/>.
    /// </summary>
    public GdipGraphics(Graphics graphics)
    {
        _g = graphics;
        UseAntialias = true;
    }



    #region Draw bitmap

    public void DrawBitmap(object? bitmap, RectangleF? destRect = null, RectangleF? srcRect = null, InterpolationMode interpolation = InterpolationMode.NearestNeighbor, float opacity = 1)
    {
        if (bitmap is not Bitmap bmp) return;

        srcRect ??= new(0, 0, bmp.Width, bmp.Height);
        destRect ??= new(0, 0, bmp.Width, bmp.Height);


        _g.DrawImage(bmp, destRect.Value, srcRect.Value, GraphicsUnit.Pixel);
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
        var rect = new RectangleF(x, y, width, height);

        if (fillColor != null)
        {
            using var bgBrush = new SolidBrush(fillColor.Value);
            _g.FillEllipse(bgBrush, rect);
        }

        using var borderPen = new Pen(borderColor, strokeWidth);
        _g.DrawEllipse(borderPen, rect);
    }

    #endregion // Draw/Fill ellipse


    #region Draw lines

    public void DrawLine(float x1, float y1, float x2, float y2, Color c, float strokeWidth = 1)
    {
        DrawLine(new PointF(x1, y1), new PointF(x2, y2), c, strokeWidth);
    }

    public void DrawLine(PointF p1, PointF p2, Color c, float strokeWidth = 1)
    {
        using var pen = new Pen(c, strokeWidth);

        _g.DrawLine(pen, p1, p2);
    }

    #endregion // Draw lines


    #region Draw/Fill Rectangle

    public void DrawRectangle(float x, float y, float width, float height, float radius, Color borderColor, Color? fillColor = null, float strokeWidth = 1)
    {
        DrawRectangle(new RectangleF(x, y, width, height), radius, borderColor, fillColor, strokeWidth);
    }


    public void DrawRectangle(RectangleF rect, float radius, Color borderColor, Color? fillColor = null, float strokeWidth = 1)
    {
        var path = GetRoundRectanglePath(rect, radius);

        if (fillColor != null)
        {
            using var bgBrush = new SolidBrush(fillColor.Value);
            Graphics.FillPath(bgBrush, path);
        }

        using var borderPen = new Pen(borderColor, strokeWidth);
        Graphics.DrawPath(borderPen, path);
    }

    #endregion // Draw/Fill Rectangle


    #region Draw / Measure text

    public void DrawText(string text, string fontFamilyName, float fontSize, float x, float y, Color c, float? textDpi = null, StringAlignment hAlign = StringAlignment.Near, StringAlignment vAlign = StringAlignment.Near, bool isBold = false, bool isItalic = false)
    {
        var rect = new RectangleF(x, y, 0, 0);

        DrawText(text, fontFamilyName, fontSize, rect, c, textDpi, hAlign, vAlign, isBold, isItalic);
    }


    public void DrawText(string text, string fontFamilyName, float fontSize, RectangleF rect, Color c, float? textDpi = null, StringAlignment hAlign = StringAlignment.Near, StringAlignment vAlign = StringAlignment.Near, bool isBold = false, bool isItalic = false)
    {
        // fix DPI
        if (textDpi != null)
        {
            var dpiFactor = textDpi.Value / 96.0f;
            fontSize *= dpiFactor * dpiFactor;
        }

        // Create a StringFormat object with the each line of text, and the block
        // of text centered on the page.
        var stringFormat = new StringFormat
        {
            Alignment = hAlign,
            LineAlignment = vAlign,
        };

        var style = FontStyle.Regular;
        if (isBold) style |= FontStyle.Bold;
        if (isItalic) style |= FontStyle.Italic;

        
        using var font = new Font(fontFamilyName, fontSize, style, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(c);

        _g.DrawString(text, font, brush, rect, stringFormat);
    }

    public SizeF MeasureText(string text, string fontFamilyName, float fontSize, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue, float textDpi = 96, bool isBold = false, bool isItalic = false)
    {
        var size = new SizeF(maxWidth, maxHeight);

        return MeasureText(text, fontFamilyName, fontSize, size, textDpi, isBold, isItalic);
    }


    public SizeF MeasureText(string text, string fontFamilyName, float fontSize, SizeF size, float textDpi = 96, bool isBold = false, bool isItalic = false)
    {
        var style = FontStyle.Regular;
        if (isBold) style |= FontStyle.Bold;
        if (isItalic) style |= FontStyle.Italic;


        using var font = new Font(fontFamilyName, fontSize, style, GraphicsUnit.Pixel);

        return _g.MeasureString(text, font, size);
    }

    #endregion // Draw / Measure text


    #region Others

    public void Flush()
    {
        _g.Flush();
    }


    public void ClearBackground(Color color)
    {
        _g.Clear(color);
    }


    #endregion // Others



    /// <summary>
    /// Gets rounded rectangle graphic path
    /// </summary>
    /// <param name="bounds">Input rectangle</param>
    /// <param name="radius">Border radius</param>
    private static GraphicsPath GetRoundRectanglePath(RectangleF bounds, float radius)
    {
        var diameter = Math.Abs(radius * 2);
        var size = new SizeF(diameter, diameter);
        var arc = new RectangleF(bounds.Location, size);
        var path = new GraphicsPath();

        if (radius == 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        // top left arc  
        path.AddArc(arc, 180, 90);

        // top right arc  
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        // bottom right arc  
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // bottom left arc 
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }

}
