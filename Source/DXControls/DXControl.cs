using DirectN;
using System.Runtime.InteropServices;

namespace DXControls;

public class DXControl : Control
{
    private float _dpi = 96.0f;

    protected IComObject<ID2D1Factory> Direct2DFactory;
    protected IComObject<IDWriteFactory> DWriteFactory;
    protected ID2D1HwndRenderTarget? RenderTarget;
    protected ID2D1DeviceContext? DeviceContext;
    protected DXGraphics? DGraphics;


    public float BaseDpi
    {
        get => _dpi;
        set
        {
            if (DeviceContext == null) return;

            _dpi = value;
            DeviceContext.SetDpi(_dpi, _dpi);
        }
    }

    public float TextDpi { get; set; } = 96.0f;


    public DXControl()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

        Direct2DFactory = D2D1Functions.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED);
        
        DWriteFactory = DWriteFunctions.DWriteCreateFactory(DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED);
    }


    #region Override functions

    protected override void CreateHandle()
    {
        base.CreateHandle();
        if (DesignMode) return;

        DoubleBuffered = false;

        CreateGraphicsResources();
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_ERASEBKGND = 0x0014;
        const int WM_SIZE = 0x0005;
        const int WM_DESTROY = 0x0002;

        switch (m.Msg)
        {
            //case WM_ERASEBKGND:

            //    // to fix background is delayed to paint on launch
            //    if (_firstPaintBackground)
            //    {
            //        _firstPaintBackground = false;
            //        if (!_useHardwardAcceleration)
            //        {
            //            base.WndProc(ref m);
            //        }
            //        else
            //        {
            //            _graphics?.BeginRender(D2DColor.FromGDIColor(BackColor));
            //            _graphics?.EndRender();
            //        }
            //    }
            //    break;

            case WM_SIZE:
                base.WndProc(ref m);

                //RenderTarget?.Resize(new(ClientSize.Width, ClientSize.Height));
                break;


            default:
                base.WndProc(ref m);
                break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        Direct2DFactory.Dispose();
        DWriteFactory.Dispose();

        DGraphics?.Dispose();
        DGraphics = null;

        if (DeviceContext != null)
        {
            Marshal.ReleaseComObject(DeviceContext);
            DeviceContext = null;
        }

        if (RenderTarget != null)
        {
            Marshal.ReleaseComObject(RenderTarget);
            RenderTarget = null;
        }
    }


    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (DesignMode) return;


        RenderTarget?.Resize(new(ClientSize.Width, ClientSize.Height));

        // update the control once size/windows state changed
        ResizeRedraw = true;
    }


    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
    }


    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (DesignMode)
        {
            base.OnPaintBackground(e);
        }
    }


    [Obsolete("Use 'OnRender' to paint the control.", true)]
    protected override void OnPaint(PaintEventArgs e)
    {
        if (DesignMode)
        {
            e.Graphics.Clear(BackColor);

            using var brush = new SolidBrush(ForeColor);
            e.Graphics.DrawString("This control does not support rendering in design mode.",
                Font, brush, 10, 10);

            return;
        }

        // make sure the 
        CreateGraphicsResources();
        if (DeviceContext == null || DGraphics == null) return;


        // start drawing session
        var bgColor = BackColor.Equals(Color.Transparent) ? Parent.BackColor : BackColor;
        DGraphics.BeginDraw(new(bgColor.ToArgb()));

        OnRender(DGraphics);

        // end drawing session
        DGraphics.EndDraw();
    }


    #endregion


    #region Virtual functions

    /// <summary>
    /// Paints the control using <see cref="DXGraphics"/>.
    /// </summary>
    protected virtual void OnRender(DXGraphics g)
    {
        
    }
    
    #endregion



    #region Private functions

    /// <summary>
    /// Create graphics resources.
    /// </summary>
    private void CreateGraphicsResources()
    {
        if (RenderTarget == null)
        {
            var hwndRenderTargetProps = new D2D1_HWND_RENDER_TARGET_PROPERTIES()
            {
                hwnd = Handle,
                pixelSize = new D2D_SIZE_U((uint)Width, (uint)Height),
            };

            Direct2DFactory.Object.CreateHwndRenderTarget(new D2D1_RENDER_TARGET_PROPERTIES(), hwndRenderTargetProps, out RenderTarget).ThrowOnError();

            RenderTarget.SetDpi(BaseDpi, BaseDpi);
            RenderTarget.Resize(new(ClientSize.Width, ClientSize.Height));
            

            DeviceContext = (ID2D1DeviceContext)RenderTarget;

            DGraphics = new(DeviceContext, DWriteFactory);
        }
    }

    #endregion


    #region Public functions



    #endregion

}
