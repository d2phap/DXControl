/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using D2Phap;
using DirectN;
using WicNet;

namespace Demo;

public class DXCanvas : DXControl
{
    private IComObject<ID2D1Bitmap>? _bitmapD2d = null;
    private Rectangle rectText = new(100, 100, 0, 200);

    public WicBitmapSource? Image
    {
        set
        {
            DXHelper.DisposeD2D1Bitmap(ref _bitmapD2d);
            GC.Collect();

            if (Device == null || value == null)
            {
                _bitmapD2d = null;
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

            Device.CreateBitmapFromWicBitmap(value.ComObject.Object, bitmapPropsPtr,
                out ID2D1Bitmap bmp)
                .ThrowOnError();

            _bitmapD2d = new ComObject<ID2D1Bitmap>(bmp);
        }
    }


    public DXCanvas()
    {
        CheckFPS = true;
    }


    protected override void OnRender(IGraphics g)
    {
        var p1 = new Point(0, 0);
        var p2 = new Point(ClientSize.Width, ClientSize.Height);

        // draw X
        g.DrawLine(p1, p2, Color.Blue, 3.0f);
        g.DrawLine(new(ClientSize.Width, 0), new(0, ClientSize.Height), Color.Red, 3.0f);


        //// draw image
        //if (_bitmapD2d != null)
        //{
        //    _bitmapD2d.Object.GetSize(out var size);
        //    g.DrawBitmap(_bitmapD2d.Object,
        //        new RectangleF(10f, 10f, size.width * 1, size.height * 1),
        //        new RectangleF(0, 0, size.width, size.height));
        //}

        // draw rectangle border
        g.DrawRectangle(10f, 10f, ClientSize.Width - 20, ClientSize.Height - 20, 0,
            Color.GreenYellow, null, 5f);


        // draw and fill rounded rectangle
        g.DrawRectangle(ClientSize.Width / 1.5f, ClientSize.Height / 1.5f, 300, 100,
            20f, Color.LightCyan, Color.FromArgb(180, Color.Cyan), 3f);


        // draw and fill rectangle
        g.DrawRectangle(rectText, 0, Color.Green, Color.FromArgb(100, Color.Yellow));

        // draw text
        g.DrawText($"Dương Diệu Pháp 😛💋", Font.Name, Font.Size * 1.5f, rectText,
            Color.Lavender, DeviceDpi, StringAlignment.Center, isBold: true, isItalic: true);


        // draw and fill ellipse
        g.DrawEllipse(400, 400, 300, 200, Color.FromArgb(120, Color.Magenta), Color.Purple, 5);


        var engine = UseHardwareAcceleration ? "GPU" : "GDI+";
        g.DrawText($"FPS: {FPS} - {engine}", Font.Name, Font.Size * 2, 10, 10, Color.Purple, DeviceDpi);

    }

    
    
    protected override void OnFrame(FrameEventArgs e)
    {
        base.OnFrame(e);
        rectText.Width++;
    }
}