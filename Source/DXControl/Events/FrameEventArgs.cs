/*
MIT License
Copyright (C) 2022 - 2024 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
namespace D2Phap;


/// <summary>
/// Provides the data for <see cref="DXControl.Frame"/> event.
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
