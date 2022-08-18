/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
namespace D2Phap;


public enum InterpolationMode
{
    NearestNeighbor = 0,
    Linear = 1,
    Cubic = 2,
    SampleLinear = 3,
    Antisotropic = 4,
    HighQualityBicubic = 5,
}


/// <summary>
/// An interface for both Direct2D and GDI+ graphics
/// </summary>
public interface IGraphics : IDisposable
{
    /// <summary>
    /// Gets, sets the value specifies whether smoothing (antialiasing) is applied
    /// to lines and curves and the edges of filled areas.
    /// </summary>
    bool UseAntialias { get; set; }


    #region Draw bitmap

    /// <summary>
    /// Draw bitmap.
    /// </summary>
    public void DrawBitmap(object? bitmap,
        RectangleF? destRect = null,
        RectangleF? srcRect = null,
        InterpolationMode interpolation = InterpolationMode.NearestNeighbor,
        float opacity = 1);

    #endregion // Draw bitmap


    #region Draw lines

    void DrawLine(float x1, float y1, float x2, float y2, Color c, float strokeWidth = 1f);
    void DrawLine(Point p1, Point p2, Color c, float strokeWidth = 1f);
    void DrawLine(PointF p1, PointF p2, Color c, float strokeWidth = 1f);

    #endregion // Draw lines


    #region Draw/Fill Rectangle

    /// <summary>
    /// Draw a rounded rectangle and fill its background.
    /// </summary>
    public void DrawRectangle(float x, float y, float width, float height,
        float radius, Color borderColor, Color? fillColor = null, float strokeWidth = 1);


    /// <summary>
    /// Draw a rounded rectangle and fill its background.
    /// </summary>
    public void DrawRectangle(RectangleF rect, float radius,
        Color borderColor, Color? fillColor = null, float strokeWidth = 1);

    #endregion // Draw/Fill Rectangle


    #region Draw/Fill ellipse

    /// <summary>
    /// Draw an ellipse.
    /// </summary>
    public void DrawEllipse(float x, float y, float width, float height, Color borderColor, Color? fillColor, float strokeWidth = 1);


    /// <summary>
    /// Draw an ellipse.
    /// </summary>
    public void DrawEllipse(float x, float y, float radius, Color borderColor, Color? fillColor, float strokeWidth = 1);


    /// <summary>
    /// Draw an ellipse.
    /// </summary>
    public void DrawEllipse(RectangleF rect, Color borderColor, Color? fillColor, float strokeWidth = 1);

    #endregion // Draw/Fill ellipse


    #region Draw / Measure text

    /// <summary>
    /// Draw text.
    /// </summary>
    public void DrawText(string text, string fontFamilyName, float fontSize,
        float x, float y, Color c, float? textDpi = null,
        StringAlignment hAlign = StringAlignment.Near,
        StringAlignment vAlign = StringAlignment.Near,
        bool isBold = false, bool isItalic = false);


    /// <summary>
    /// Draw text.
    /// </summary>
    public void DrawText(string text, string fontFamilyName, float fontSize,
        RectangleF rect, Color c, float? textDpi = null,
        StringAlignment hAlign = StringAlignment.Near,
        StringAlignment vAlign = StringAlignment.Near,
        bool isBold = false, bool isItalic = false);


    /// <summary>
    /// Measure text.
    /// </summary>
    public SizeF MeasureText(string text, string fontFamilyName, float fontSize,
        float maxWidth = float.MaxValue, float maxHeight = float.MaxValue, float textDpi = 96.0f,
        bool isBold = false, bool isItalic = false);


    /// <summary>
    /// Measure text.
    /// </summary>
    public SizeF MeasureText(string text, string fontFamilyName, float fontSize,
        SizeF size, float textDpi = 96.0f, bool isBold = false, bool isItalic = false);

    #endregion // Draw / Measure text


    #region Others

    /// <summary>
    /// Executes all pending drawing commands.
    /// </summary>
    void Flush();


    /// <summary>
    /// Clear the background by the given color.
    /// </summary>
    void ClearBackground(Color color);

    #endregion // Others

}
