/*
MIT License
Copyright (C) 2022 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
namespace D2Phap;


/// <summary>
/// Provides the data for <see cref="DXControl.RenderDX"/> event.
/// </summary>
public class RenderDXEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref='DXGraphics'/> object used to draw.
    /// </summary>
    public DXGraphics Graphics { get; init; }


    public RenderDXEventArgs(DXGraphics g)
    {
        Graphics = g;
    }
}

