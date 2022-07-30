/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControls
*/
using DirectN;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace D2Phap;

/// <summary>
/// Defines the base class for hybrid control with Direct2D and GDI+ graphics support.
/// </summary>
public class DXControl : Control
{
    private float _dpi = 96.0f;
    private readonly VerticalBlankTicker _ticker = new();
    private bool _enableAnimation = true;

    private int _currentFps = 0;
    private int _lastFps = 0;
    private DateTime _lastFpsUpdate = DateTime.UtcNow;


    // Protected properties
    #region Protected properties
    protected IComObject<ID2D1Factory> Direct2DFactory;
    protected IComObject<IDWriteFactory> DWriteFactory;
    protected ID2D1HwndRenderTarget? RenderTarget;
    protected ID2D1DeviceContext? DeviceContext;


    /// <summary>
    /// Gets the <see cref='DXGraphics'/> object used to draw in <see cref="OnRender(DXGraphics)"/>.
    /// </summary>
    protected DXGraphics? D2Graphics;


    /// <summary>
    /// Request to update frame by <see cref="OnFrame"/> event.
    /// </summary>
    protected bool RequestUpdateFrame { get; set; } = true;


    /// <summary>
    /// Enable FPS measurement.
    /// </summary>
    protected bool CheckFPS { get; set; } = false;

    #endregion // Protected properties


    // Public properties
    #region Public properties

    /// <summary>
    /// Gets, sets the DPI for drawing when using <see cref="DXGraphics"/>.
    /// </summary>
    [Browsable(false)]
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


    /// <summary>
    /// Gets, sets the DPI for text drawing when using <see cref="DXGraphics"/>.
    /// </summary>
    [Browsable(false)]
    public float TextDpi { get; set; } = 96.0f;


    /// <summary>
    /// Gets FPS info when the <see cref="CheckFPS"/> is set to <c>true</c>.
    /// </summary>
    [Browsable(false)]
    public int FPS => _lastFps;


    /// <summary>
    /// Enables animation support for the control.
    /// </summary>
    [Category("Animation")]
    [DefaultValue(true)]
    public bool EnableAnimation
    {
        get => _enableAnimation;
        set
        {
            _enableAnimation = value;

            if (!_enableAnimation)
            {
                if (_ticker.IsRunning) _ticker.Stop(1000);
            }
            else
            {
                if (!_ticker.IsRunning)
                {
                    _ticker.ResetTicks();
                    _ticker.Start();
                }
            }
        }
    }


    /// <summary>
    /// Occurs when the control is redrawn with <see cref="DXGraphics"/>.
    /// </summary>
    public event EventHandler<RenderDXEventArgs>? RenderDX;


    /// <summary>
    /// Occurs when the control is redrawn with <see cref="Graphics"/>.
    /// </summary>
    public event EventHandler<PaintEventArgs>? RenderGDIPlus;


    /// <summary>
    /// Occurs when the animation frame logics need to update.
    /// </summary>
    public event EventHandler<FrameEventArgs>? Frame;

    #endregion // Public properties



    /// <summary>
    /// Initializes new instance of <see cref="DXControl"/>.
    /// </summary>
    public DXControl()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

        Direct2DFactory = D2D1Functions.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED);

        DWriteFactory = DWriteFunctions.DWriteCreateFactory(DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED);
    }


    // Override functions
    #region Override functions

    protected override void CreateHandle()
    {
        base.CreateHandle();
        if (DesignMode) return;

        DoubleBuffered = false;
        CreateGraphicsResources();

        _ticker.Tick += Ticker_Tick;
        _ticker.Start();
    }


    protected override void Dispose(bool disposing)
    {
        _ticker.Stop(1000);
        _ticker.Tick -= Ticker_Tick;

        Direct2DFactory.Dispose();
        DWriteFactory.Dispose();

        D2Graphics?.Dispose();
        D2Graphics = null;

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

        GC.Collect();

        base.Dispose(disposing);
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


    /// <summary>
    /// Control background is painted in <see cref="OnPaint(PaintEventArgs)"/>.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (DesignMode)
        {
            base.OnPaintBackground(e);
        }
    }


    /// <summary>
    /// Use <see cref="OnRender(DXGraphics)"/> or <see cref="OnRender(Graphics)"/> to paint.
    /// </summary>
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
        if (DeviceContext == null || D2Graphics == null) return;

        var bgColor = BackColor.Equals(Color.Transparent) ? Parent.BackColor : BackColor;
        DoubleBuffered = false; // must be false


        // start Direct2D graphics drawing session
        DeviceContext.BeginDraw();
        D2Graphics.ClearBackground(new(bgColor.ToArgb()));
        OnRender(D2Graphics);

        // end drawing session
        DeviceContext.EndDraw();


        // Use GDI+ to draw
        OnRender(e.Graphics);


        // calculate FPS
        if (CheckFPS)
        {
            if (_lastFpsUpdate.Second != DateTime.UtcNow.Second)
            {
                _lastFps = _currentFps;
                _currentFps = 0;
                _lastFpsUpdate = DateTime.UtcNow;
            }
            else
            {
                _currentFps++;
            }
        }
    }

    #endregion // Override functions


    // Virtual functions
    #region Virtual functions

    /// <summary>
    /// Paints the control using Direct2D <see cref="DXGraphics"/>.
    /// </summary>
    protected virtual void OnRender(DXGraphics g)
    {
        RenderDX?.Invoke(this, new RenderDXEventArgs(g));
    }


    /// <summary>
    /// Paints the control using GDI+ <see cref="Graphics"/>.
    /// </summary>
    protected virtual void OnRender(Graphics g)
    {
        RenderGDIPlus?.Invoke(this, new(g, ClientRectangle));
    }


    /// <summary>
    /// Process animation logic when frame changes
    /// </summary>
    protected virtual void OnFrame(FrameEventArgs e)
    {
        Frame?.Invoke(this, e);
    }

    #endregion // Virtual functions


    // Private functions
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
            D2Graphics = new DXGraphics(DeviceContext, DWriteFactory);
        }
    }


    private void Ticker_Tick(object? sender, VerticalBlankTickerEventArgs e)
    {
        if (EnableAnimation && RequestUpdateFrame)
        {
            OnFrame(new(e.Ticks));
            Invalidate();
        }
    }

    #endregion // Private functions

}
