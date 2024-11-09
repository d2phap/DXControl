/*
MIT License
Copyright (C) 2022 - 2024 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
namespace D2Phap;


/// <summary>
/// Provides the data for <see cref="DXControl.DeviceCreated"/> event.
/// </summary>
public class DeviceCreatedEventArgs(DeviceCreatedReason reason) : EventArgs
{
    /// <summary>
    /// Gets the <see cref='IGraphics'/> object used to draw.
    /// </summary>
    public DeviceCreatedReason Reason { get; init; } = reason;

}



public enum DeviceCreatedReason
{
    FirstTime,
    UseHardwareAccelerationChanged,
}
