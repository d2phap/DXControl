using DirectN;
using ImageMagick;
using Microsoft.Win32.SafeHandles;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using WicNet;

namespace Demo;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        using var imgM = new MagickImage(Environment.GetCommandLineArgs()[1]);
        canvas.Image = FromBitmapSource(imgM.ToBitmapSource());
    }

    public static WicBitmapSource? FromBitmapSource(BitmapSource bmp)
    {
        if (bmp == null)
            return null;


        var prop = bmp.GetType().GetProperty("WicSourceHandle",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var srcHandle = (SafeHandleZeroOrMinusOneIsInvalid?)prop?.GetValue(bmp);
        if (srcHandle == null) return null;


        var obj = Marshal.GetObjectForIUnknown(srcHandle.DangerousGetHandle());

        var wicSrc = new WicBitmapSource(obj);
        wicSrc.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);

        return wicSrc;
    }
}