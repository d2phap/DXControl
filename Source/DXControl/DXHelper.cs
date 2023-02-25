/*
MIT License
Copyright (C) 2022 - 2023 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using DirectN;
using WicNet;

namespace D2Phap;

public static class DXHelper
{
    /// <summary>
    /// Disposes the Direct2D1 bitmap.
    /// </summary>
    public static void DisposeD2D1Bitmap(ref IComObject<ID2D1Bitmap1>? bmp)
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
            rect.Left,
            rect.Top,
            rect.Right,
            rect.Bottom);
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


    /// <summary>
    /// Converts <see cref="D2D_RECT_F"/> to <see cref="RectangleF"/>
    /// </summary>
    public static RectangleF ToRectangle(D2D_RECT_F rect)
    {
        return new RectangleF(rect.left, rect.top, rect.Width, rect.Height);
    }


    /// <summary>
    /// Converts <see cref="D2D_SIZE_F"/> to <see cref="SizeF"/>
    /// </summary>
    public static SizeF ToSize(D2D_SIZE_F size)
    {
        return new SizeF(size.width, size.height);
    }


    /// <summary>
    /// Converts <see cref="D2D_POINT_2F"/> to <see cref="PointF"/>
    /// </summary>
    public static PointF ToPoint(D2D_POINT_2F point)
    {
        return new PointF(point.x, point.y);
    }


    /// <summary>
    /// Creates default <see cref="D2D1_BITMAP_PROPERTIES"/>.
    /// </summary>
    public static D2D1_BITMAP_PROPERTIES1 CreateDefaultBitmapProps()
    {
        return new D2D1_BITMAP_PROPERTIES1()
        {
            pixelFormat = new D2D1_PIXEL_FORMAT()
            {
                alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED,
                format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
            },
            dpiX = 96.0f,
            dpiY = 96.0f,
        };
    }



    /// <summary>
    /// Converts <see cref="WicBitmapSource"/> to <see cref="ID2D1Bitmap"/> COM object.
    /// </summary>
    public static IComObject<ID2D1Bitmap1>? ToD2D1Bitmap(IComObject<ID2D1DeviceContext6>? dc, WicBitmapSource? wicSrc, D2D1_BITMAP_PROPERTIES1? bitmapProps = null)
    {
        if (dc == null || wicSrc == null)
        {
            return null;
        }

        wicSrc.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);

        // create D2D1Bitmap from WICBitmapSource
        bitmapProps ??= new D2D1_BITMAP_PROPERTIES1()
        {
            pixelFormat = new D2D1_PIXEL_FORMAT()
            {
                alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED,
                format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
            },
            dpiX = 96.0f,
            dpiY = 96.0f,
        };

        var comBmp = dc.CreateBitmapFromWicBitmap(wicSrc.ComObject, bitmapProps);

        return comBmp;
    }

}
