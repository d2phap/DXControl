/*
MIT License
Copyright (C) 2022 - 2024 DUONG DIEU PHAP
Project & license info: https://github.com/d2phap/DXControl
*/
namespace D2Phap;


/// <summary>
/// Provides the data for <see cref="DXControl.Render"/> event.
/// </summary>
public class RenderEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref='IGraphics'/> object used to draw.
    /// </summary>
    public IGraphics Graphics { get; init; }


    public RenderEventArgs(IGraphics g)
    {
        Graphics = g;
    }
}

