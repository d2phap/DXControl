/*
MIT License
Copyright (C) 2022 - 2023 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using D2Phap;
using DirectN;
using WicNet;
using InterpolationMode = D2Phap.InterpolationMode;

namespace Demo;

public class DXCanvas : DXControl
{
    private IComObject<ID2D1Bitmap>? _bitmapD2d = null;
    private Rectangle rectText = new(40, 40, 300, 200);

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

            value.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);
            Device.CreateBitmapFromWicBitmap(value.ComObject.Object, bitmapPropsPtr,
                out ID2D1Bitmap bmp)
                .ThrowOnError();

            _bitmapD2d = new ComObject<ID2D1Bitmap>(bmp);
        }
    }

    public Bitmap? Bitmap { get; set; }


    public DXCanvas()
    {
        CheckFPS = true;
        //EnableAnimation = false;
    }


    protected override void OnRender(IGraphics g)
    {
        var p1 = new Point(0, 0);
        var p2 = new Point(ClientSize.Width, ClientSize.Height);

        // draw X
        g.DrawLine(p1, p2, Color.Blue, 10.0f);
        g.DrawLine(new(ClientSize.Width, 0), new(0, ClientSize.Height), Color.Red, 3.0f);


        // draw D2DBitmap image
        if (UseHardwareAcceleration && _bitmapD2d != null)
        {
            _bitmapD2d.Object.GetSize(out var size);
            g.DrawBitmap(_bitmapD2d.Object,
                destRect: new RectangleF(150, 150, size.width * 5, size.height * 5),
                srcRect: new RectangleF(0, 0, size.width, size.height),
                interpolation: InterpolationMode.NearestNeighbor
                );
        }
        // draw GDI+ image
        else if (!UseHardwareAcceleration && Bitmap != null)
        {
            g.DrawBitmap(Bitmap,
                destRect: new RectangleF(150, 150, Bitmap.Width * 5, Bitmap.Height * 5),
                srcRect: new RectangleF(0, 0, Bitmap.Width, Bitmap.Height),
                interpolation: InterpolationMode.NearestNeighbor
                );
        }


        // draw rectangle border
        g.DrawRectangle(10f, 10f, ClientSize.Width - 20, ClientSize.Height - 20, 0,
            Color.GreenYellow, null, 5f);


        // draw and fill rounded rectangle
        g.DrawRectangle(ClientSize.Width / 1.5f, ClientSize.Height / 1.5f, 300, 100,
            20f, Color.LightCyan, Color.FromArgb(180, Color.Cyan), 3f);

        // draw and fill ellipse
        g.DrawEllipse(200, 200, 300, 200, Color.FromArgb(120, Color.Magenta), Color.Purple, 5);


        // draw geometry D2D only
        if (UseHardwareAcceleration && g is D2DGraphics dg)
        {
            using var geo = dg.GetCombinedRectanglesGeometry(new RectangleF(200, 300, 300, 300),
                new Rectangle(250, 250, 300, 100), 0, 0, D2D1_COMBINE_MODE.D2D1_COMBINE_MODE_INTERSECT);
            dg.DrawGeometry(geo, Color.Transparent, Color.Yellow, 2);


            using var geo2 = dg.GetCombinedEllipsesGeometry(new Rectangle(450, 450, 300, 100), new RectangleF(400, 400, 300, 300), D2D1_COMBINE_MODE.D2D1_COMBINE_MODE_EXCLUDE);
            dg.DrawGeometry(geo2, Color.Transparent, Color.Green, 2f);
        }


        // draw and fill rectangle
        g.DrawRectangle(rectText, 0, Color.Green, Color.FromArgb(100, Color.Yellow));


        // draw text
        var text = "Dương\r\nDiệu\r\nPháp\r\n😵🪺🐷😶‍🌫️🤯🫶🏿";
        var textSize = g.MeasureText(text, Font.Name, 12, textDpi: DeviceDpi, isBold: true, isItalic: true);
        g.DrawText($"{text}\r\n{textSize}", Font.Name, 12, rectText,
            Color.Red, DeviceDpi, StringAlignment.Near, isBold: true, isItalic: true);
        g.DrawRectangle(new RectangleF(rectText.Location, textSize), 0, Color.Red);


        // draw FPD info
        var engine = UseHardwareAcceleration ? "GPU" : "GDI+";
        g.DrawText($"FPS: {FPS} - {engine}", Font.Name, 18, 0, 0, Color.Purple, DeviceDpi);

    }

    
    protected override void OnFrame(FrameEventArgs e)
    {
        base.OnFrame(e);
        //rectText.Width++;
    }
}