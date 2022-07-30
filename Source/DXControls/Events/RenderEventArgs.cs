
namespace DXControls;

public class RenderEventArgs : EventArgs
{
    public DXGraphics Graphics { get; init; }

    public RenderEventArgs(DXGraphics g)
    {
        Graphics = g;
    }
}


public class FrameEventArgs : EventArgs
{
    public long Ticks { get; init; }

    public FrameEventArgs(long ticks)
    {
        Ticks = ticks;
    }
}

