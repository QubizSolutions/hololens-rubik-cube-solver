using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectedFace{
    public CubeColor[] Colors { get; set; }
    public FaceName FaceName { get; set; }
}

public enum CubeColor
{
    black = 0, green = 1, yellow = 2, blue = 3, red = 4, orange = 5
}

public enum FaceName
{
    Front, Right, Back, Left, Up, Down
}
