/*
MIT License
Copyright (C) 2022 - 2024 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using DirectN;

namespace D2Phap;


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
    /// <param name="bitmap">
    /// The bitmap object is either <see cref="ID2D1Bitmap"/> for <see cref="D2DGraphics"/>, or <see cref="Bitmap"/> for <see cref="GdipGraphics"/>.
    /// </param>
    /// <param name="destRect"></param>
    /// <param name="srcRect"></param>
    /// <param name="interpolation"></param>
    /// <param name="opacity"></param>
    void DrawBitmap(object? bitmap,
        RectangleF? destRect = null,
        RectangleF? srcRect = null,
        InterpolationMode interpolation = InterpolationMode.NearestNeighbor,
        float opacity = 1);

    #endregion // Draw bitmap


    #region Draw lines

    /// <summary>
    /// Draw line.
    /// </summary>
    void DrawLine(float x1, float y1, float x2, float y2, Color c, float strokeWidth = 1f);


    /// <summary>
    /// Draw line.
    /// </summary>
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
    /// Forces immediate execution of all operations currently on the stack.
    /// </summary>
    void Flush();


    /// <summary>
    /// Fills the entire drawing surface with the specified color.
    /// </summary>
    void ClearBackground(Color color);

    #endregion // Others

}


/// <summary>
/// Interpolation mode for <see cref="IGraphics"/>.
/// </summary>
public enum InterpolationMode
{
    NearestNeighbor = 0,
    Linear = 1,
    Cubic = 2,
    SampleLinear = 3,
    Antisotropic = 4,
    HighQualityBicubic = 5,
}
