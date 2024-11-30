/*
MIT License
Copyright (C) 2022 - 2024 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/

namespace D2Phap.DXControl;


/// <summary>
/// Reason when the device is created.
/// </summary>
public enum DeviceCreatedReason
{
    FirstTime,
    UseHardwareAccelerationChanged,
    DeviceLost,
}


/// <summary>
/// Interpolation mode for <see cref="DXGraphics"/>.
/// </summary>
public enum InterpolationMode
{
    NearestNeighbor = 0,
    Linear = 1,
    Cubic = 2,
    SampleLinear = 3,
    Antisotropic = 4,
    HighQualityBicubic = 5,
}

