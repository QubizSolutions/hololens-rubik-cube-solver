using System;

public class Cubies
{
    public int x { get; set; }

    public int y { get; set; }

    public CubeColor color { get; set; }

    public Cubies(int x, int y, CubeColor color)
    {
        this.x = x;
        this.y = y;
        this.color = color;
    }
}
