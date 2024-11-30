/*
MIT License
Copyright (C) 2022 - 2025 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/

namespace D2Phap.DXControl;


/// <summary>
/// Provides the data for <see cref="DXCanvas.Render"/> event.
/// </summary>
public class RenderEventArgs(DXGraphics g) : EventArgs
{
    /// <summary>
    /// Gets the <see cref='DXGraphics'/> object used to draw.
    /// </summary>
    public DXGraphics Graphics { get; init; } = g;
}



/// <summary>
/// Provides the data for <see cref="DXCanvas.DeviceCreated"/> event.
/// </summary>
public class DeviceCreatedEventArgs(DeviceCreatedReason reason) : EventArgs
{
    /// <summary>
    /// Gets reason why the device is created.
    /// </summary>
    public DeviceCreatedReason Reason { get; init; } = reason;
}



/// <summary>
/// Provides the data for <see cref="DXCanvas.Frame"/> event.
/// </summary>
public class FrameEventArgs : EventArgs
{
#if NET8_0_OR_GREATER

    /// <summary>
    /// Gets, sets the period for <see cref="PeriodicTimer"/>.
    /// </summary>
    public TimeSpan Period { get; set; } = TimeSpan.FromMilliseconds(10);

#endif

    public FrameEventArgs() { }
}

