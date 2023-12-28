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
    /// <summary>
    /// Gets the number of ticks
    /// </summary>
    public long Ticks { get; init; }


    public FrameEventArgs(long ticks)
    {
        Ticks = ticks;
    }
}
