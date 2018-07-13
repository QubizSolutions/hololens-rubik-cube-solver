using System;
using System.Collections;
using System.Collections.Generic;
using TwoPhase;
using UnityEngine;

public class CubeSolver : MonoBehaviour {

    private static GameObject rubikCube;

    private static CubeSolver instance;
    public static CubeSolver Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
        rubikCube = GameObject.Find("RubikCube");
        //rubikCube.SetActive(false);
    }

    void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }

    public void StartSolving(Dictionary<FaceName, List<CubeColor>> rubikFaces)
    {
        Debug.Log("StartSolving()");
        //rubikCube.SetActive(true);
        Rotation.Instance.SetColors(rubikFaces);

        string front = "";
        string back = "";
        string down = "";
        string left = "";
        string right = "";
        string top = "";

        foreach(var face in rubikFaces)
        {
            switch (face.Key)
            {
                case FaceName.Up:
                    top = Color2Face(face.Value);
                    break;
                case FaceName.Right:
                    right += Color2Face(face.Value);
                    break;
                case FaceName.Back:
                    back += Color2Face(face.Value);
                    break;
                case FaceName.Down:
                    down += Color2Face(face.Value);
                    break;
                case FaceName.Front:
                    front += Color2Face(face.Value);
                    break;
                case FaceName.Left:
                    left += Color2Face(face.Value);
                    break;
            }
        }

        string cube = top + right + front + down + left + back;
        string solvedCube = Search.solution(cube, 25, false);
        Debug.Log("Solved cube moves: " + solvedCube);

        Rotation.Instance.StartSolvingAnimations(solvedCube);
    }

    static string Color2Face(List<CubeColor> colors)
    {
        string face = "";

        foreach (var color in colors)
        {
            switch (color)
            {
                case CubeColor.black:
                    face += "F";
                    break;
                case CubeColor.blue:
                    face += "L";
                    break;
                case CubeColor.green:
                    face += "R";
                    break;
                case CubeColor.orange:
                    face += "D";
                    break;
                case CubeColor.red:
                    face += "U";
                    break;
                case CubeColor.yellow:
                    face += "B";
                    break;
            }
        }

        return face;
    }
}
