using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {

    public GameObject rubix;
    public float inTime = 3f;

    private Vector3 origin;
    private GameObject[] cubes;

    private static Rotation instance;
    public static Rotation Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    //static string sequenceMoves = "R F D L U U U U L L L D D D F F F R R R B B B B";
    //static string sequenceMoves = "U2 D2 R2 L2 F2 B2";
    //static string sequenceMoves = "U U D D R R L L F F B B";
    //static string sequenceMoves = "F F' B B' U U' D D' R R' L L'"; //D2 R2 L2 B2 F2";
    //static char delimiter = ' ';
    //string[] moves = sequenceMoves.Split(delimiter);

    private Dictionary<FaceName, List<CubeColor>> shuffledDict = new Dictionary<FaceName, List<CubeColor>>()
        {
            { FaceName.Front , new List<CubeColor>() { CubeColor.black, CubeColor.yellow, CubeColor.black, CubeColor.yellow, CubeColor.black, CubeColor.yellow, CubeColor.black, CubeColor.yellow, CubeColor.black} },
            { FaceName.Right , new List<CubeColor>() { CubeColor.green, CubeColor.blue, CubeColor.green, CubeColor.blue, CubeColor.green, CubeColor.blue, CubeColor.green, CubeColor.blue, CubeColor.green } },
            { FaceName.Left , new List<CubeColor>() { CubeColor.blue, CubeColor.green, CubeColor.blue, CubeColor.green, CubeColor.blue, CubeColor.green, CubeColor.blue, CubeColor.green, CubeColor.blue } },
            { FaceName.Back , new List<CubeColor>() { CubeColor.yellow, CubeColor.black, CubeColor.yellow, CubeColor.black, CubeColor.yellow, CubeColor.black, CubeColor.yellow, CubeColor.black, CubeColor.yellow } },
            { FaceName.Down , new List<CubeColor>() { CubeColor.orange, CubeColor.red, CubeColor.orange, CubeColor.red, CubeColor.orange, CubeColor.red, CubeColor.orange, CubeColor.red, CubeColor.orange } },
            { FaceName.Up , new List<CubeColor>() { CubeColor.red, CubeColor.orange, CubeColor.red, CubeColor.orange, CubeColor.red, CubeColor.orange, CubeColor.red, CubeColor.orange, CubeColor.red } }
        };

    //void Start ()
    //{
    //    SetColors(shuffledDict);
    //    string moveSeq = "U2 D2 R2 L2 F2 B2";
    //    StartSolvingAnimations(moveSeq);
    //}

    public void SetColors(Dictionary<FaceName, List<CubeColor>> colorPattern)
    {
        print("SetColors()");
        cubes = GameObject.FindGameObjectsWithTag("Cube");
        origin = rubix.transform.position;
        ShuffleCube shuffle = new ShuffleCube(origin, colorPattern);
    }

    public void StartSolvingAnimations(string moveSeq)
    {
        print("StartSolvingAnimations()");
        char delimiter = ' ';
        string[] moves = moveSeq.Split(delimiter);
        foreach(var move in moves)
        {
            print(move);
        }
        StartCoroutine(Rotate(moves));
    }

    GameObject SelectFaceOnXAxis(int faceIndicator)
    {
        var rotatePivot = new GameObject("rotatePivot");
        rotatePivot.transform.position = origin;
        //Debug.Log("Get cubes from axis X with value of: " + (origin.x + faceIndicator));
        foreach (GameObject cube in cubes)
        {
            if (Math.Round(cube.transform.localPosition.x,2) == faceIndicator)
            {
                cube.transform.parent = rotatePivot.transform;
            }
        }
        return rotatePivot;
    }

    GameObject SelectFaceOnYAxis(int faceIndicator)
    {
        var rotatePivot = new GameObject("rotatePivot");
        rotatePivot.transform.position = origin;
        //Debug.Log("Get cubes from axis Y with value of: " + (origin.y + faceIndicator));
        foreach (GameObject cube in cubes)
        {
            if (Math.Round(cube.transform.localPosition.y,2) == faceIndicator)
            {            
                cube.transform.parent = rotatePivot.transform;               
            }
        }
        return rotatePivot;
    }

    GameObject SelectFaceOnZAxis(int faceIndicator)
    {
        var rotatePivot = new GameObject("rotatePivot");
        rotatePivot.transform.position = origin;
        //Debug.Log("Get cubes from axis Z with value of: " + (origin.z + faceIndicator));
        foreach (GameObject cube in cubes)
        {
            if (Math.Round(cube.transform.localPosition.z,2) == faceIndicator)
            {
                cube.transform.parent = rotatePivot.transform;
            }
        }

        return rotatePivot;
    }

    void MoveCubesToParent(GameObject rotatePivot)
    {
        var number = rotatePivot.transform.childCount;
        for (int i = number - 1; i >= 0; i--)
        {
            var cube = rotatePivot.transform.GetChild(i);
            cube.transform.parent = rubix.transform;
        }
        //Debug.Log("Put back X cubes");
    }

    private IEnumerable<WaitForSeconds> RotateX(int faceIndicator, float targetAngle)
    {
        var rotatePivot = SelectFaceOnXAxis(faceIndicator);
        Vector3 byAngle;
        if(faceIndicator == 2)
            byAngle = Vector3.right * targetAngle;   
        else
            byAngle = Vector3.left * targetAngle;
            
        var fromAngle = rotatePivot.transform.rotation;
        var toAngle = Quaternion.Euler(rotatePivot.transform.eulerAngles + byAngle);
        for(var t = 0f; t <= 1; t += Time.deltaTime/inTime)
        {      
            rotatePivot.transform.rotation = Quaternion.Slerp(fromAngle, toAngle,t);
            yield return null;
        }
        rotatePivot.transform.rotation = toAngle;

        //Debug.Log("Rotate on X axis");
        MoveCubesToParent(rotatePivot);
        Destroy(rotatePivot);
        yield return new WaitForSeconds(1f);
        yield break;
    }

    private IEnumerable<WaitForSeconds> RotateY(int faceIndicator, float targetAngle)
    {
        var rotatePivot = SelectFaceOnYAxis(faceIndicator);
        Vector3 byAngle;
        var fromAngle = rotatePivot.transform.rotation;

        if (faceIndicator == 2)
            byAngle = Vector3.up * targetAngle;
        else
            byAngle = Vector3.down * targetAngle;

        var toAngle = Quaternion.Euler(rotatePivot.transform.eulerAngles + byAngle);
        for (var t = 0f; t <= 1; t += Time.deltaTime / inTime)
        {
            rotatePivot.transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
        rotatePivot.transform.rotation = toAngle;

        //Debug.Log("Rotate on Y axis");
        MoveCubesToParent(rotatePivot);
        Destroy(rotatePivot);
        yield return new WaitForSeconds(1f);
        yield break;
    }

    private IEnumerable<WaitForSeconds> RotateZ(int faceIndicator, float targetAngle)
    {
        var rotatePivot = SelectFaceOnZAxis(faceIndicator);
        Vector3 byAngle;
        var fromAngle = rotatePivot.transform.rotation;

        if (faceIndicator == 2)
            byAngle = Vector3.back * targetAngle;
        else
            byAngle = Vector3.forward * targetAngle;

        var toAngle = Quaternion.Euler(rotatePivot.transform.eulerAngles + byAngle);
        for (var t = 0f; t <= 1; t += Time.deltaTime / inTime)
        {
            rotatePivot.transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
        rotatePivot.transform.rotation = toAngle;
        //Debug.Log("Rotate on Z axis");
        MoveCubesToParent(rotatePivot);
        Destroy(rotatePivot);        
        yield return new WaitForSeconds(1f);
        yield break;        
    }

    IEnumerator Rotate(string[] moves)
    {
        print("Cubes: " + cubes.Length);
        yield return new WaitForSeconds(2f);

        int faceIndicator;
        float targetAngle;

        foreach (string move in moves)
        {
            switch (move)
            {
                case "R":
                    faceIndicator = 2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateX(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }                    
                    break;
                case "R'":
                    faceIndicator = 2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateX(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "R2":
                    faceIndicator = 2;
                    targetAngle = -180.0f;
                    foreach (var rotation in RotateX(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "L":
                    faceIndicator = -2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateX(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "L'":
                    faceIndicator = -2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateX(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "L2":
                    faceIndicator = -2;
                    targetAngle = 180.0f;
                    foreach (var rotation in RotateX(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "D":
                    faceIndicator = -2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateY(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "D'":
                    faceIndicator = -2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateY(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "D2":
                    faceIndicator = -2;
                    targetAngle = 180.0f;
                    foreach (var rotation in RotateY(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "U":
                    faceIndicator = 2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateY(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "U'":
                    faceIndicator = 2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateY(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "U2":
                    faceIndicator = 2;
                    targetAngle = -180.0f;
                    foreach (var rotation in RotateY(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "F":
                    faceIndicator = -2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateZ(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "F'":
                    faceIndicator = -2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateZ(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "F2":
                    faceIndicator = -2;
                    targetAngle = 180.0f;
                    foreach (var rotation in RotateZ(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "B":
                    faceIndicator = 2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateZ(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "B'":
                    faceIndicator = 2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateZ(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                case "B2":
                    faceIndicator = 2;
                    targetAngle = -180.0f;
                    foreach (var rotation in RotateZ(faceIndicator, targetAngle))
                    {
                        yield return rotation;
                    }
                    break;
                default:
                    break;
            }
        }

        print("Reached the target");
        yield return new WaitForSeconds(3f);
        print("Rotation is now finished");
    }
}
