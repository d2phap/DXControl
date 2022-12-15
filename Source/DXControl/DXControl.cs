/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
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
    // Internal properties
    #region Internal properties

    protected bool _isControlLoaded = false;
    protected float _dpi = 96.0f;
    protected readonly VerticalBlankTicker _ticker = new();

    // Protected properties
    protected readonly IComObject<ID2D1Factory> _d2DFactory = D2D1Functions.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED);
    protected readonly IComObject<IDWriteFactory> _dWriteFactory = DWriteFunctions.DWriteCreateFactory(DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED);
    protected ID2D1HwndRenderTarget? _renderTarget;
    protected ID2D1DeviceContext? _device;
    protected D2DGraphics? _graphicsD2d;
    protected GdipGraphics? _graphicsGdi;


    protected bool _useHardwardAcceleration = true;
    protected bool _firstPaintBackground = true;
    protected bool _enableAnimation = true;
    protected int _currentFps = 0;
    protected int _lastFps = 0;
    protected DateTime _lastFpsUpdate = DateTime.UtcNow;

    #endregion // Internal properties


    // Public properties
    #region Public properties

    /// <summary>
    /// Gets Direct2D factory.
    /// </summary>
    [Browsable(false)]
    public IComObject<ID2D1Factory> Direct2DFactory => _d2DFactory;


    /// <summary>
    /// Gets DirectWrite factory.
    /// </summary>
    [Browsable(false)]
    public IComObject<IDWriteFactory> DirectWriteFactory => _dWriteFactory;


    /// <summary>
    /// Gets render target for this control.
    /// </summary>
    [Browsable(false)]
    public ID2D1HwndRenderTarget? RenderTarget => _renderTarget;


    /// <summary>
    /// Gets Direct2D device.
    /// </summary>
    [Browsable(false)]
    public ID2D1DeviceContext Device
    {
        get
        {
            if (_renderTarget == null || _device == null)
            {
                DisposeDevice();
                CreateDevice();
            }

#pragma warning disable CS8603 // Possible null reference return.
            return _device;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }


    /// <summary>
    /// Gets the <see cref='D2DGraphics'/> object used to draw in <see cref="Render"/>.
    /// </summary>
    [Browsable(false)]
    public D2DGraphics? D2Graphics => _graphicsD2d;


    /// <summary>
    /// Gets the value indicates if control is fully loaded
    /// </summary>
    [Browsable(false)]
    public bool IsReady => !DesignMode && Created;


    /// <summary>
    /// Gets, sets the DPI for drawing when using <see cref="D2DGraphics"/>.
    /// </summary>
    [Browsable(false)]
    public float BaseDpi
    {
        get => _dpi;
        set
        {
            if (_device == null) return;

            _dpi = value;
            _device.SetDpi(_dpi, _dpi);
        }
    }


    /// <summary>
    /// Gets, sets a value indicating whether this control should draw its surface
    /// using Direct2D or GDI+.
    /// </summary>
    [Category("Graphics")]
    [DefaultValue(true)]
    public virtual bool UseHardwareAcceleration
    {
        get => _useHardwardAcceleration;
        set
        {
            _useHardwardAcceleration = value;
            DoubleBuffered = !_useHardwardAcceleration;
        }
    }


    /// <summary>
    /// Request to update logics of the current frame in the <see cref="OnFrame"/> event.
    /// </summary>
    [Browsable(false)]
    public bool RequestUpdateFrame { get; set; } = false;


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
    /// Enable FPS measurement.
    /// </summary>
    [Browsable(false)]
    public bool CheckFPS { get; set; } = false;


    /// <summary>
    /// Gets FPS info when the <see cref="CheckFPS"/> is set to <c>true</c>.
    /// </summary>
    [Browsable(false)]
    public int FPS => _lastFps;


    /// <summary>
    /// Occurs when the control is loaded and ready to use.
    /// </summary>
    public event EventHandler<EventArgs>? Loaded;


    /// <summary>
    /// Occurs when the control is being rendered by <see cref="IGraphics"/>.
    /// </summary>
    public event EventHandler<RenderEventArgs>? Render;


    /// <summary>
    /// Occurs when the animation frame logics need to update.
    /// </summary>
    public event EventHandler<FrameEventArgs>? Frame;


    #endregion


    /// <summary>
    /// Initializes new instance of <see cref="DXControl"/>.
    /// </summary>
    public DXControl()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }


    // Override functions
    #region Override functions

    protected override void CreateHandle()
    {
        base.CreateHandle();
        if (DesignMode) return;

        DoubleBuffered = false;

        if (_renderTarget == null || _device == null)
        {
            DisposeDevice();
            CreateDevice();
        }


        // animation initiation
        _ticker.Tick += Ticker_Tick;
        _ticker.WaitError += Ticker_WaitError;
        _ticker.Start();
    }


    protected override void DestroyHandle()
    {
        base.DestroyHandle();

        _d2DFactory.Dispose();
        _dWriteFactory.Dispose();

        DisposeDevice();
    }


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _ticker.Stop(1000);
        _ticker.Tick -= Ticker_Tick;
        _ticker.WaitError -= Ticker_WaitError;

        _graphicsD2d?.Dispose();
        _graphicsD2d = null;

        // '_device' must be disposed in DestroyHandle()
    }


    protected override void WndProc(ref Message m)
    {
        const int WM_SIZE = 0x0005;
        const int WM_ERASEBKGND = 0x0014;
        const int WM_DESTROY = 0x0002;

        switch (m.Msg)
        {
            case WM_ERASEBKGND:

                // to fix background is delayed to paint on launch
                if (_firstPaintBackground)
                {
                    _firstPaintBackground = false;
                    if (!_useHardwardAcceleration)
                    {
                        base.WndProc(ref m);
                    }
                    else
                    {
                        _device?.BeginDraw();
                        _device?.Clear(_D3DCOLORVALUE.FromColor(BackColor));
                        _device?.EndDraw();
                    }
                }
                break;

            case WM_SIZE:
                base.WndProc(ref m);

                _renderTarget?.Resize(new((uint)ClientSize.Width, (uint)ClientSize.Height));
                break;

            case WM_DESTROY:
                _d2DFactory.Dispose();
                _dWriteFactory.Dispose();
                DisposeDevice();

                base.WndProc(ref m);
                break;

            default:
                base.WndProc(ref m);
                break;
        }
    }


    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (DesignMode) return;


        _renderTarget?.Resize(new((uint)ClientSize.Width, (uint)ClientSize.Height));

        // update the control once size/windows state changed
        ResizeRedraw = true;
    }


    protected override void OnSizeChanged(EventArgs e)
    {
        // detect if control is loaded
        if (!DesignMode && Created)
        {
            // control is loaded
            if (!_isControlLoaded)
            {
                _isControlLoaded = true;

                OnLoaded();
            }

            base.OnSizeChanged(e);
        }
    }


    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (!_useHardwardAcceleration)
        {
            base.OnPaintBackground(e);
        }
        else
        {
            // handled in OnPaint event
        }
    }


    /// <summary>
    /// <b>Do use</b> <see cref="OnRender(IGraphics)"/> if you want to draw on the control.
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        if (DesignMode)
        {
            e.Graphics.Clear(BackColor);

            using var brush = new SolidBrush(ForeColor);
            e.Graphics.DrawString(
                $"{GetType().FullName} does not support rendering in design mode.",
                Font, brush, 10, 10);

            return;
        }


        // use hardware acceleration
        if (UseHardwareAcceleration && _device != null)
        {
            DoubleBuffered = false;

            _graphicsD2d ??= new D2DGraphics(_device, _d2DFactory, _dWriteFactory);

            _device.BeginDraw();
            _device.Clear(_D3DCOLORVALUE.FromColor(BackColor));
            OnRender(_graphicsD2d);
            _device.EndDraw();
        }

        // use GDI+ graphics
        else
        {
            DoubleBuffered = true;

            if (_graphicsGdi == null)
            {
                _graphicsGdi = new GdipGraphics(e.Graphics);
            }
            else
            {
                _graphicsGdi.Graphics = e.Graphics;
            }

            OnRender(_graphicsGdi);
        }


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


        // Start animation
        if (_enableAnimation && !_ticker.IsRunning)
        {
            _ticker.Start();
        }
    }


    #endregion // Override functions


    // New / Virtual functions
    #region New / Virtual functions

    /// <summary>
    /// Triggers <see cref="Render"/> event to paint the control.
    /// </summary>
    protected virtual void OnRender(IGraphics g)
    {
        if (!IsReady) return;
        Render?.Invoke(this, new(g));
    }


    /// <summary>
    /// Triggers <see cref="Frame"/> event to process animation logic when frame changes.
    /// </summary>
    protected virtual void OnFrame(FrameEventArgs e)
    {
        if (!IsReady) return;

        Frame?.Invoke(this, e);
    }


    /// <summary>
    /// Triggers <see cref="Loaded"/> event when the control is ready.
    /// </summary>
    protected virtual void OnLoaded()
    {
        Loaded?.Invoke(this, new());
    }


    /// <summary>
    /// Invalidates client retangle of the control and causes a paint message to the control. This does not apply to child controls.
    /// </summary>
    public new void Invalidate()
    {
        Invalidate(false);
    }

    #endregion


    // Private functions
    #region Private functions


    private void Ticker_Tick(object? sender, VerticalBlankTickerEventArgs e)
    {
        if (EnableAnimation && RequestUpdateFrame)
        {
            OnFrame(new(e.Ticks));
            Invalidate();
        }
    }

    private void Ticker_WaitError(object? sender, VerticalBlankTickerErrorEventArgs e)
    {
        const uint UNKNOWN_ERROR = 0xc01e0006;

        // happens when the screen is off, we handle this error and put the thread
        // into sleep so that it will auto-recover when the screen is on again
        // https://github.com/smourier/DirectN/issues/29
        if (e.Error == unchecked((int)UNKNOWN_ERROR))
        {
            Thread.Sleep(1000);
            e.Handled = true;
        }
    }

    #endregion // Private functions


    // Public functions
    #region Public functions

    /// <summary>
    /// Initializes value for <see cref="Device"/>, <see cref="RenderTarget"/> and <see cref="D2Graphics"/>.
    /// </summary>
    public void CreateDevice()
    {
        var renderTargetProps = new D2D1_RENDER_TARGET_PROPERTIES()
        {
            dpiX = _dpi,
            dpiY = _dpi,
            type = D2D1_RENDER_TARGET_TYPE.D2D1_RENDER_TARGET_TYPE_DEFAULT,
            usage = D2D1_RENDER_TARGET_USAGE.D2D1_RENDER_TARGET_USAGE_NONE,
            minLevel = D2D1_FEATURE_LEVEL.D2D1_FEATURE_LEVEL_DEFAULT,
            pixelFormat = new D2D1_PIXEL_FORMAT()
            {
                alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED,
                format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
            },
        };

        var hwndRenderTargetProps = new D2D1_HWND_RENDER_TARGET_PROPERTIES()
        {
            hwnd = Handle,
            pixelSize = new D2D_SIZE_U((uint)Width, (uint)Height),
            presentOptions = D2D1_PRESENT_OPTIONS.D2D1_PRESENT_OPTIONS_NONE,
        };

        _d2DFactory.Object.CreateHwndRenderTarget(renderTargetProps, hwndRenderTargetProps, out _renderTarget).ThrowOnError();

        _renderTarget.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
        _renderTarget.SetDpi(BaseDpi, BaseDpi);
        _renderTarget.Resize(new((uint)ClientSize.Width, (uint)ClientSize.Height));

        _device = (ID2D1DeviceContext)_renderTarget;
        _graphicsD2d = new D2DGraphics(_device, _d2DFactory, _dWriteFactory);
    }


    /// <summary>
    /// Dispose <see cref="Device"/> and <see cref="RenderTarget"/> objects.
    /// </summary>
    public void DisposeDevice()
    {
        if (_device != null)
        {
            Marshal.ReleaseComObject(_device);
            _device = null;
        }

        if (_renderTarget != null)
        {
            Marshal.ReleaseComObject(_renderTarget);
            _renderTarget = null;
        }

        GC.Collect();
    }

    #endregion // Public functions

}
