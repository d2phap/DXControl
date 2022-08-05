/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using DirectN;

namespace D2Phap;

public static class DXHelper
{
    /// <summary>
    /// Disposes the Direct2D1 bitmap.
    /// </summary>
    public static void DisposeD2D1Bitmap(ref IComObject<ID2D1Bitmap>? bmp)
    {
        if (bmp == null) return;

        Interlocked.Exchange(ref bmp, null)?.Dispose();
    }

    
    /// <summary>
    /// Converts <see cref="Color"/> to <see cref="_D3DCOLORVALUE"/>.
    /// </summary>
    public static _D3DCOLORVALUE FromColor(Color color, byte? alpha = null)
    {
        return _D3DCOLORVALUE.FromArgb(alpha ?? color.A, color.R, color.G, color.B);
    }


    /// <summary>
    /// Converts <see cref="Rectangle"/> to <see cref="D2D_RECT_F"/>.
    /// </summary>
    public static D2D_RECT_F ToD2DRectF(Rectangle rect)
    {
        return new D2D_RECT_F(
            (float)rect.Left,
            (float)rect.Top,
            (float)rect.Right,
            (float)rect.Bottom);
    }


    /// <summary>
    /// Converts <see cref="RectangleF"/> to <see cref="D2D_RECT_F"/>.
    /// </summary>
    public static D2D_RECT_F ToD2DRectF(RectangleF rect)
    {
        return new D2D_RECT_F(
            rect.Left,
            rect.Top,
            rect.Right,
            rect.Bottom);
    }


    /// <summary>
    /// Creates <see cref="D2D_RECT_F"/>.
    /// </summary>
    public static D2D_RECT_F ToD2DRectF(float x, float y, float width, float height)
    {
        return new D2D_RECT_F(x, y, x + width, y + height);
    }

}
