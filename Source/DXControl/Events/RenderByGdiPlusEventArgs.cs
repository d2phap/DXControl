/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
namespace D2Phap;


/// <summary>
/// Provides the data for <see cref="DXControl.RenderByGdiPLus"/> event.
/// </summary>
public class RenderByGdiPlusEventArgs : EventArgs
{
    /// <summary>
    /// Gets the number of ticks
    /// </summary>
    public Graphics Graphics { get; init; }


    public RenderByGdiPlusEventArgs(Graphics g)
    {
        Graphics = g;
    }
}
