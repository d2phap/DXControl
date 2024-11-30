/*
MIT License
Copyright (C) 2022-2024 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
using DirectN;
using System.ComponentModel;
using WicNet.Utilities;

namespace D2Phap.DXControl;


/// <summary>
/// Defines the base class for hybrid control with Direct2D and GDI+ graphics support.
/// </summary>
public class DXCanvas : Control
{
    // Internal properties
    #region Internal properties

    protected bool _isControlLoaded = false;
    protected float _dpi = 96.0f;
    protected readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(10));

    // Protected properties
    protected IComObject<ID2D1Factory1>? _d2DFactory = null;
    protected IComObject<IDWriteFactory5>? _dWriteFactory = null;
    protected IComObject<ID2D1HwndRenderTarget>? _renderTarget;
    protected IComObject<ID2D1DeviceContext6>? _device;
    protected DXGraphics? _graphicsD2d;


    protected bool _useHardwardAcceleration = true;
    protected bool _firstPaintBackground = true;
    protected int _currentFps = 0;
    protected int _lastFps = 0;
    protected DateTime _lastFpsUpdate = DateTime.UtcNow;

    const uint D2DERR_RECREATE_TARGET = 0x8899000C;

    #endregion // Internal properties


    // Public properties
    #region Public properties

    /// <summary>
    /// Gets Direct2D factory.
    /// </summary>
    [Browsable(false)]
    public IComObject<ID2D1Factory1>? Direct2DFactory => _d2DFactory;


    /// <summary>
    /// Gets DirectWrite factory.
    /// </summary>
    [Browsable(false)]
    public IComObject<IDWriteFactory5>? DirectWriteFactory => _dWriteFactory;


    /// <summary>
    /// Gets render target for this control.
    /// </summary>
    [Browsable(false)]
    public IComObject<ID2D1HwndRenderTarget>? RenderTarget => _renderTarget;


    /// <summary>
    /// Gets Direct2D device context.
    /// </summary>
    [Browsable(false)]
    public IComObject<ID2D1DeviceContext6> Device => _device!;


    /// <summary>
    /// Gets the <see cref='DXGraphics'/> object used to draw in <see cref="Render"/>.
    /// </summary>
    [Browsable(false)]
    public DXGraphics? D2Graphics => _graphicsD2d;


    /// <summary>
    /// Gets the value indicates if control is fully loaded
    /// </summary>
    [Browsable(false)]
    public bool IsReady => !DesignMode && Created;


    /// <summary>
    /// Gets, sets the DPI for drawing when using <see cref="DXGraphics"/>.
    /// </summary>
    [Browsable(false)]
    public float BaseDpi
    {
        get => _dpi;
        set
        {
            if (_device == null) return;

            _dpi = value;
            _device.Object.SetDpi(_dpi, _dpi);
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
            var enableAnimationOldValue = EnableAnimation;
            var hasChange = _useHardwardAcceleration != value;

            _useHardwardAcceleration = value;

            // re-create device
            if (hasChange)
            {
                EnableAnimation = false;
                CreateDevice(DeviceCreatedReason.UseHardwareAccelerationChanged);

                EnableAnimation = enableAnimationOldValue;
            }
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
    public bool EnableAnimation { get; set; } = true;


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
    /// Occurs when the Device is created.
    /// </summary>
    public event EventHandler<DeviceCreatedEventArgs>? DeviceCreated;


    /// <summary>
    /// Occurs when the control is being rendered by <see cref="DXGraphics"/>.
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
    public DXCanvas()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }


    // New / Virtual functions
    #region New / Virtual functions

    /// <summary>
    /// Initializes value for <see cref="Direct2DFactory"/> and <see cref="DirectWriteFactory"/>.
    /// </summary>
    protected virtual void CreateFactories()
    {
        _d2DFactory = D2D1Functions.D2D1CreateFactory<ID2D1Factory1>(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED);

        _dWriteFactory = DWriteFunctions.DWriteCreateFactory<IDWriteFactory5>(DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED);
    }


    /// <summary>
    /// Disposes <see cref="Direct2DFactory"/> and <see cref="DirectWriteFactory"/>.
    /// </summary>
    protected virtual void DisposeFactories()
    {
        _d2DFactory?.Dispose();
        _d2DFactory = null;

        _dWriteFactory?.Dispose();
        _dWriteFactory = null;
    }


    /// <summary>
    /// Initializes value for <see cref="Device"/>, <see cref="RenderTarget"/> and <see cref="D2Graphics"/>.
    /// </summary>
    protected virtual void CreateDevice(DeviceCreatedReason reason)
    {
        // dispose the current device
        DisposeDevice();

        if (_d2DFactory == null)
        {
            throw new NullReferenceException($"{nameof(Direct2DFactory)} is null");
        }
        if (_dWriteFactory == null)
        {
            throw new NullReferenceException($"{nameof(DirectWriteFactory)} is null");
        }


        var rtType = UseHardwareAcceleration
                ? D2D1_RENDER_TARGET_TYPE.D2D1_RENDER_TARGET_TYPE_DEFAULT
                : D2D1_RENDER_TARGET_TYPE.D2D1_RENDER_TARGET_TYPE_SOFTWARE;
        var renderTargetProps = new D2D1_RENDER_TARGET_PROPERTIES()
        {
            dpiX = _dpi,
            dpiY = _dpi,
            type = rtType,
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
            presentOptions = D2D1_PRESENT_OPTIONS.D2D1_PRESENT_OPTIONS_IMMEDIATELY,
        };


        // create new render target
        _renderTarget = _d2DFactory.CreateHwndRenderTarget(hwndRenderTargetProps, renderTargetProps);
        _renderTarget.Object.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
        _renderTarget.Object.SetDpi(BaseDpi, BaseDpi);
        _renderTarget.Resize(new((uint)ClientSize.Width, (uint)ClientSize.Height));


        // create devide and graphics
        _device = _renderTarget.AsComObject<ID2D1DeviceContext6>();
        _graphicsD2d = new DXGraphics(_device, _d2DFactory, _dWriteFactory);


        OnDeviceCreated(reason);
    }


    /// <summary>
    /// Disposes <see cref="Device"/> and <see cref="RenderTarget"/> objects.
    /// </summary>
    protected virtual void DisposeDevice()
    {
        _graphicsD2d?.Dispose();
        _graphicsD2d = null;

        _device?.Dispose();
        _device = null;

        _renderTarget?.Dispose();
        _renderTarget = null;

        GC.Collect();
    }


    /// <summary>
    /// Triggers <see cref="DeviceCreated"/> event.
    /// </summary>
    protected virtual void OnDeviceCreated(DeviceCreatedReason reason)
    {
        DeviceCreated?.Invoke(this, new DeviceCreatedEventArgs(reason));
    }


    /// <summary>
    /// Triggers <see cref="Render"/> event to paint the control.
    /// </summary>
    protected virtual void OnRender(DXGraphics g)
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



    // Override functions
    #region Override functions

    protected override void CreateHandle()
    {
        base.CreateHandle();
        if (DesignMode) return;

        // disable GDI+
        DoubleBuffered = false;

        // create factories
        if (_d2DFactory == null || _dWriteFactory == null)
        {
            CreateFactories();
        }

        // create device
        if (_renderTarget == null || _device == null)
        {
            CreateDevice(DeviceCreatedReason.FirstTime);
        }


        // start framing timer
        _ = StartFramingTimerAsync();
    }


    protected override void DestroyHandle()
    {
        base.DestroyHandle();

        DisposeDevice();
        DisposeFactories();
    }


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _timer.Dispose();

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

                    _device?.BeginDraw();
                    _device?.Clear(BackColor.ToD3DCOLORVALUE());
                    _device?.EndDraw();

                    //if (!_useHardwardAcceleration)
                    //{
                    //    base.WndProc(ref m);
                    //}
                    //else
                    //{
                    //    _device?.BeginDraw();
                    //    _device?.Clear(BackColor.ToD3DCOLORVALUE());
                    //    _device?.EndDraw();
                    //}
                }
                break;

            case WM_SIZE:
                base.WndProc(ref m);

                _renderTarget?.Resize(new((uint)ClientSize.Width, (uint)ClientSize.Height));
                break;

            case WM_DESTROY:
                DisposeDevice();
                DisposeFactories();

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
        // handled in OnPaint event

        // base.OnPaintBackground(e);
    }


    /// <summary>
    /// <b>Do use</b> <see cref="OnRender(DXGraphics)"/> if you want to draw on the control.
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


        // draw
        if (_device != null && _graphicsD2d != null)
        {
            _device.BeginDraw();
            _device.Clear(BackColor.ToD3DCOLORVALUE());
            OnRender(_graphicsD2d);

            try
            {
                _device.EndDraw();
            }
            catch (Win32Exception ex)
            {
                // handle device lost
                if (ex.ErrorCode == unchecked((int)D2DERR_RECREATE_TARGET))
                {
                    CreateDevice(DeviceCreatedReason.DeviceLost);
                }
                else
                {
                    throw;
                }
            }
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
    }


    #endregion // Override functions



    // Private functions
    #region Private functions

    private async Task StartFramingTimerAsync()
    {
        while (await _timer.WaitForNextTickAsync())
        {
            if (EnableAnimation && RequestUpdateFrame)
            {
                var e = new FrameEventArgs();

#if NET8_0_OR_GREATER
                e.Period = _timer.Period;
#endif

                OnFrame(e);
                Invalidate();

#if NET8_0_OR_GREATER
                _timer.Period = e.Period;
#endif
            }
        }
    }


    #endregion // Private functions


}
