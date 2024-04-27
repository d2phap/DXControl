/*
MIT License
Copyright (C) 2022-2024 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using DirectN;
using ImageMagick;
using Microsoft.Win32.SafeHandles;
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
        var filePath = @"photo.png";

        if (!string.IsNullOrEmpty(filePath))
        {
            using var imgM = new MagickImage(filePath);
            canvas.Image = FromBitmapSource(imgM.ToBitmapSource());

            canvas.Bitmap = new Bitmap(filePath, true);
        }
    }


    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
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

    private void canvas_DragDrop(object sender, DragEventArgs e)
    {
        // Drag file from DESKTOP to APP
        if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);

        if (filePaths.Length > 0)
        {
            Text = filePaths[0];

            using var imgM = new MagickImage(Text);
            canvas.Image = FromBitmapSource(imgM.ToBitmapSource());
        }
    }

    private void canvas_DragOver(object sender, DragEventArgs e)
    {
        e.Effect = DragDropEffects.Copy;
    }

    private void chkD2D_CheckedChanged(object sender, EventArgs e)
    {
        canvas.UseHardwareAcceleration = chkD2D.Checked;
    }

    private void ChkAnimation_CheckedChanged(object sender, EventArgs e)
    {
        canvas.EnableAnimation = chkAnimation.Checked;
    }
}