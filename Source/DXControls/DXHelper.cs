using DirectN;

namespace DXControl;

public class DXHelper
{
    /// <summary>
    /// Convert <see cref="System.Drawing.Color"/> to <see cref="_D3DCOLORVALUE"/>.
    /// </summary>
    public static _D3DCOLORVALUE ConvertColor(Color color)
    {
        _D3DCOLORVALUE value = new()
        {
            r = color.R / 255.0f,
            g = color.G / 255.0f,
            b = color.B / 255.0f,
            a = color.A / 255.0f,
        };

        return value;
    }


    /// <summary>
    /// Convert <see cref="System.Drawing.PointF"/> to <see cref="D2D_POINT_2F"/>.
    /// </summary>
    public static D2D_POINT_2F ConvertPointF(PointF point)
    {
        return new D2D_POINT_2F(point.X, point.Y);
    }

}
