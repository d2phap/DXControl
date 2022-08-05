/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
namespace D2Phap;


/// <summary>
/// Provides the data for <see cref="DXControl.RenderByDirect2D"/> event.
/// </summary>
public class RenderByDirect2DEventArgs : EventArgs
{
    /// <summary>
    /// Gets the number of ticks
    /// </summary>
    public DXGraphics Graphics { get; init; }


    public RenderByDirect2DEventArgs(DXGraphics g)
    {
        Graphics = g;
    }
}
