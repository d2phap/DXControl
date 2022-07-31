using D2Phap;
using DirectN;
using WicNet;

namespace Demo;

public class DXCanvas : DXControl
{
    private ID2D1Bitmap? _d2dBitmap = null;
    
    public WicBitmapSource? Image
    {
        set
        {
            if (DeviceContext == null || value == null)
            {
                _d2dBitmap = null;
                return;
            }
            
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

            DeviceContext.CreateBitmapFromWicBitmap(value.ComObject.Object, bitmapPropsPtr,
                out _d2dBitmap)
                .ThrowOnError();
        }
    }


    public DXCanvas()
    {
        CheckFPS = true;
    }


    protected override void OnRender(DXGraphics g)
    {
        var p1 = new D2D_POINT_2F(0, 0);
        var p2 = new D2D_POINT_2F(ClientSize.Width, ClientSize.Height);

        // draw X
        g.DrawLine(p1, p2, _D3DCOLORVALUE.Blue, 3.0f);
        g.DrawLine(new(ClientSize.Width, 0), new(0, ClientSize.Height), _D3DCOLORVALUE.Red, 3.0f);


        // draw image
        if (_d2dBitmap != null)
        {
            _d2dBitmap.GetSize(out var size);
            g.DrawBitmap(_d2dBitmap,
                new D2D_RECT_F(10f, 10f, size.width * 1, size.height * 1),
                new D2D_RECT_F(0, 0, size.width, size.height));
        }

        // draw rectangle border
        g.DrawRectangle(new(10f, 10f, new(ClientSize.Width - 20, ClientSize.Height - 20)),
            _D3DCOLORVALUE.GreenYellow, 5f);


        // draw and fill rounded rectangle
        g.FillRoundedRectangle(new(ClientSize.Width / 1.5f, ClientSize.Height / 1.5f, new(300, 100)),
            50f, 10f, _D3DCOLORVALUE.FromCOLORREF(_D3DCOLORVALUE.Cyan.Int32Value, 180));
        g.DrawRoundedRectangle(new(ClientSize.Width / 1.5f, ClientSize.Height / 1.5f, new(300, 100)),
            50f, 10f, _D3DCOLORVALUE.LightCyan, 3);


        // draw and fill rectangle
        g.FillRectangle(rectText,
            _D3DCOLORVALUE.FromCOLORREF(_D3DCOLORVALUE.Yellow.Int32Value, 100));
        g.DrawRectangle(rectText, _D3DCOLORVALUE.Green);

        // draw text
        g.DrawText($"{FPS} Dương Diệu Pháp 😛💋", Font.Name, Font.Size * 1.5f, rectText,
            _D3DCOLORVALUE.Lavender, textDpi: DeviceDpi,
            hAlign: DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_TRAILING,
            vAlign: DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,
            fontWeight: DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_BOLD);


        // draw and fill ellipse
        g.FillEllipse(400, 400, 300, 200,
            _D3DCOLORVALUE.FromCOLORREF(_D3DCOLORVALUE.Magenta.Int32Value, 120));
        g.DrawEllipse(400, 400, 300, 200, _D3DCOLORVALUE.Purple, 5);

    }


    protected override void OnRender(Graphics g)
    {
        using var pen = new Pen(Color.Red, 5);
        g.DrawRectangle(pen, new Rectangle(
            (int)rectText.left, (int)rectText.top - 50,
            (int)rectText.Width, (int)rectText.Height));
    }


    private D2D_RECT_F rectText = new(100f, 100f, new(400, 200));
    protected override void OnFrame(FrameEventArgs e)
    {
        base.OnFrame(e);
        rectText.left++;
    }
}