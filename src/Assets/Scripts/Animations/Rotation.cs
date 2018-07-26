using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {

    public float speed = 3f;

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

    public void GetNextFace(Vector3 axis, float degrees)
    {
        StartCoroutine(RotateCube(axis, Vector3.zero, degrees, 0));
    }

    public void GetNextFace(Vector3 firstAxis, Vector3 secondAxis, float degrees_f, float degrees_s)
    {
        StartCoroutine(RotateCube(firstAxis, secondAxis, degrees_f, degrees_s));
    }

    IEnumerator RotateCube(Vector3 firstAxis, Vector3 secondAxis, float degrees_f, float degrees_s)
    {
        float turningTime = 0;
        Vector3 f_angle = firstAxis * degrees_f;
        Quaternion fromAngle = gameObject.transform.localRotation;
        Quaternion toAngle = Quaternion.Euler(gameObject.transform.localEulerAngles + f_angle);

        while (gameObject.transform.localRotation != toAngle)
        {
            turningTime += Time.deltaTime * 0.3f;
            gameObject.transform.localRotation = Quaternion.Lerp(fromAngle, toAngle, turningTime);

            yield return new WaitForEndOfFrame();
        }
        gameObject.transform.localRotation = toAngle;

        if (!secondAxis.Equals(Vector3.zero))
        {
            turningTime = 0;
            Vector3 s_angle = secondAxis * degrees_s;
            fromAngle = gameObject.transform.localRotation;
            toAngle = Quaternion.Euler(gameObject.transform.localEulerAngles + s_angle);
            
            while (gameObject.transform.localRotation != toAngle)
            {
                turningTime += Time.deltaTime * 0.3f;
                gameObject.transform.localRotation = Quaternion.Lerp(fromAngle, toAngle, turningTime);

                yield return new WaitForEndOfFrame();
            }
            gameObject.transform.localRotation = toAngle;
        }
    }

    public void SetCubeForTutorial()
    {
        StartCoroutine(ChangeCubePosition());
    }

    IEnumerator ChangeCubePosition()
    {
        Vector3 originalPosition = gameObject.transform.localPosition;
        Vector3 newPosition = originalPosition + new Vector3(0, -0.1f, 0);

        float distance = Vector3.Distance(originalPosition, newPosition);

        float time = 0;
        while(gameObject.transform.localPosition != newPosition)
        {
            time += Time.deltaTime / 20;
            gameObject.transform.localPosition = Vector3.Slerp(originalPosition, newPosition, time / distance);
            yield return null;
        }
        gameObject.transform.localPosition = newPosition;

        GetNextFace(Vector3.up, Vector3.right, 30f, -10f);
    }

    public void SetColors(Dictionary<FaceName, List<CubeColor>> colorPattern)
    {
        print("SetColors()");
        cubes = GameObject.FindGameObjectsWithTag("Cube");
        ShuffleCube shuffle = new ShuffleCube();
        shuffle.SetFacesColors(gameObject.transform.localPosition, colorPattern);
    }

    public void StartSolvingAnimations(string moveSeq)
    {
        print("StartSolvingAnimations()");
        char delimiter = ' ';
        string[] moves = moveSeq.Split(delimiter);
        StartCoroutine(Rotate(moves));
    }

    void MoveCubesToParent(GameObject rotatePivot)
    {
        var number = rotatePivot.transform.childCount;
        for (int i = number - 1; i >= 0; i--)
        {
            var cube = rotatePivot.transform.GetChild(i);
            cube.transform.parent = gameObject.transform;
        }
    }

    GameObject SelectFace(int faceIndicator, string axis)
    {
        GameObject rotatePivot = new GameObject("rotatePivot");
        rotatePivot.transform.parent = gameObject.transform;
        rotatePivot.transform.localRotation = Quaternion.identity;
        rotatePivot.transform.localPosition = Vector3.zero;
        rotatePivot.transform.localScale = Vector3.one;

        switch (axis)
        {
            case "x":
                foreach (GameObject cube in cubes)
                {
                    if (Math.Round(cube.transform.localPosition.x, 2) == faceIndicator)
                    {
                        cube.transform.parent = rotatePivot.transform;
                    }
                }
                break;
            case "y":
                foreach (GameObject cube in cubes)
                {
                    if (Math.Round(cube.transform.localPosition.y, 2) == faceIndicator)
                    {
                        cube.transform.parent = rotatePivot.transform;
                    }
                }
                break;
            case "z":
                foreach (GameObject cube in cubes)
                {
                    if (Math.Round(cube.transform.localPosition.z, 2) == faceIndicator)
                    {
                        cube.transform.parent = rotatePivot.transform;
                    }
                }
                break;
            default:
                break;
        }

        return rotatePivot;
    }

    private IEnumerable<WaitForSeconds> RotateFace(int faceIndicator, float targetAngle, string axis)
    {
        Vector3 byAngle = Vector3.zero;
        GameObject rotatePivot = SelectFace(faceIndicator, axis);

        switch(axis)
        {
            case "x":
                if (faceIndicator == 2)
                    byAngle = Vector3.right * targetAngle;
                else
                    byAngle = Vector3.left * targetAngle;
                break;
            case "y":
                if (faceIndicator == 2)
                    byAngle = Vector3.up * targetAngle;
                else
                    byAngle = Vector3.down * targetAngle;
                break;
            case "z":
                if (faceIndicator == 2)
                    byAngle = Vector3.back * targetAngle;
                else
                    byAngle = Vector3.forward * targetAngle;
                break;
            default:
                break;
        }

        Quaternion fromAngle = rotatePivot.transform.localRotation;
        Quaternion toAngle = Quaternion.Euler(rotatePivot.transform.localEulerAngles + byAngle);

        for (var t = 0f; t <= 1; t += Time.deltaTime / speed)
        {
            rotatePivot.transform.localRotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
        rotatePivot.transform.localRotation = toAngle;

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
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "x"))
                    {
                        yield return rotation;
                    }                    
                    break;
                case "R'":
                    faceIndicator = 2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "x"))
                    {
                        yield return rotation;
                    }
                    break;
                case "R2":
                    faceIndicator = 2;
                    targetAngle = -180.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "x"))
                    {
                        yield return rotation;
                    }
                    break;
                case "L":
                    faceIndicator = -2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "x"))
                    {
                        yield return rotation;
                    }
                    break;
                case "L'":
                    faceIndicator = -2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "x"))
                    {
                        yield return rotation;
                    }
                    break;
                case "L2":
                    faceIndicator = -2;
                    targetAngle = -180.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "x"))
                    {
                        yield return rotation;
                    }
                    break;
                case "D":
                    faceIndicator = -2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "y"))
                    {
                        yield return rotation;
                    }
                    break;
                case "D'":
                    faceIndicator = -2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "y"))
                    {
                        yield return rotation;
                    }
                    break;
                case "D2":
                    faceIndicator = -2;
                    targetAngle = -180.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "y"))
                    {
                        yield return rotation;
                    }
                    break;
                case "U":
                    faceIndicator = 2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "y"))
                    {
                        yield return rotation;
                    }
                    break;
                case "U'":
                    faceIndicator = 2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "y"))
                    {
                        yield return rotation;
                    }
                    break;
                case "U2":
                    faceIndicator = 2;
                    targetAngle = -180.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "y"))
                    {
                        yield return rotation;
                    }
                    break;
                case "F":
                    faceIndicator = -2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "z"))
                    {
                        yield return rotation;
                    }
                    break;
                case "F'":
                    faceIndicator = -2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "z"))
                    {
                        yield return rotation;
                    }
                    break;
                case "F2":
                    faceIndicator = -2;
                    targetAngle = 180.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "z"))
                    {
                        yield return rotation;
                    }
                    break;
                case "B":
                    faceIndicator = 2;
                    targetAngle = -90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "z"))
                    {
                        yield return rotation;
                    }
                    break;
                case "B'":
                    faceIndicator = 2;
                    targetAngle = 90.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "z"))
                    {
                        yield return rotation;
                    }
                    break;
                case "B2":
                    faceIndicator = 2;
                    targetAngle = 180.0f;
                    foreach (var rotation in RotateFace(faceIndicator, targetAngle, "z"))
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

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}