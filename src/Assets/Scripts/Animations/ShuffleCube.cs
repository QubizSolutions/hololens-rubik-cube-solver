using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ShuffleCube {

    public Dictionary<FaceName, List<CubeColor>> rubikFaces;
    private Dictionary<FaceName, Color> defaultMatDict;

    public ShuffleCube(Vector3 origin, Dictionary<FaceName, List<CubeColor>> shuffledSeq)
    {
        rubikFaces = shuffledSeq;
        GameObject cube0 = GameObject.Find("Cube0");
        var mat = cube0.GetComponentInChildren<Renderer>().materials;             
        defaultMatDict = new Dictionary<FaceName, Color>()
        {
            {FaceName.Front, mat[5].color },
            {FaceName.Back, mat[3].color },
            {FaceName.Right, mat[4].color },
            {FaceName.Left, mat[2].color },
            {FaceName.Up, mat[1].color },
            {FaceName.Down, mat[0].color }
        };     

        SetMaterial(FaceName.Front, origin);
        SetMaterial(FaceName.Back, origin);
        SetMaterial(FaceName.Right, origin);
        SetMaterial(FaceName.Left, origin);
        SetMaterial(FaceName.Up, origin);
        SetMaterial(FaceName.Down, origin);
    }

    private void SetMaterial(FaceName faceName, Vector3 origin)
    {
        List<GameObject> cubes = GetFaceCubes(faceName, origin);
        for (int i = 0; i < rubikFaces[faceName].Count; i++)
        {
            GameObject cube = cubes[i];
            Material[] cubeMat = cube.GetComponentInChildren<Renderer>().materials; 
            var cubeColor = rubikFaces[faceName][i];
            //Debug.Log(cube.name + cubeColor);
            SetCubeColor(cubeMat, cubeColor, faceName);
        }
    }

    private List<GameObject> GetFaceCubes(FaceName faceName, Vector3 origin)
    {
        GameObject[] faceCubes = GameObject.FindGameObjectsWithTag("Cube");
        List<GameObject> listCubes = new List<GameObject>();
        switch (faceName)
        {
            case FaceName.Front:
                int faceIndicator = -2;
                foreach(GameObject cube in faceCubes.Where((c) => c.transform.localPosition.z == -2).OrderBy((c) => c.transform.position.x).OrderByDescending((c) => c.transform.position.y).ToList())
                {
                    listCubes.Add(cube);
                }
                break;
            case FaceName.Back:
                faceIndicator = 2;
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.z == 2).OrderByDescending((c) => c.transform.position.x).OrderByDescending((c) => c.transform.position.y).ToList())
                {                    
                    listCubes.Add(cube);                                            
                }
                break;
            case FaceName.Up:
                faceIndicator = 2;
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.y == 2).OrderBy((c) => c.transform.position.x).OrderByDescending((c) => c.transform.position.z).ToList())
                {
                    listCubes.Add(cube);                       
                }
                break;
            case FaceName.Down:
                faceIndicator = -2;
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.y == -2).OrderBy((c) => c.transform.position.x).OrderBy((c) => c.transform.position.z).ToList())
                {
                    listCubes.Add(cube);                        
                }
                break;
            case FaceName.Right:
                faceIndicator = 2;
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.x == 2).OrderBy((c) => c.transform.position.z).OrderByDescending((c) => c.transform.position.y).ToList())
                {
                    listCubes.Add(cube);                        
                }
                break;
            case FaceName.Left:
                faceIndicator = -2;
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.x == -2).OrderByDescending((c) => c.transform.position.z).OrderByDescending((c) => c.transform.position.y).ToList())
                {
                    listCubes.Add(cube);                        
                }
                break;
            default:
                break;
        }
        return listCubes;
    }

    private void SetCubeColor(Material[] cube, CubeColor color, FaceName face)
    {
        /* In order to set the color face we will take the face on which we wanna set the color*/
        switch (face)
        {
            case FaceName.Front:
                cube[5].color = GetMaterialForColor(color);
                break;
            case FaceName.Right:
                cube[4].color = GetMaterialForColor(color);
                break;
            case FaceName.Back:
                cube[3].color = GetMaterialForColor(color);
                break;
            case FaceName.Left:
                cube[2].color = GetMaterialForColor(color);
                break;
            case FaceName.Up:
                cube[1].color = GetMaterialForColor(color);
                break;
            case FaceName.Down:
                cube[0].color = GetMaterialForColor(color);
                break;
        }
    }

    private Color GetMaterialForColor(CubeColor color)
    {
        Color setColor = Color.white;
        switch (color)
        {
            case CubeColor.black:
                setColor =  defaultMatDict[FaceName.Front];
                break;
            case CubeColor.green:
                setColor = defaultMatDict[FaceName.Right];
                break;
            case CubeColor.yellow:
                setColor = defaultMatDict[FaceName.Back];
                break;
            case CubeColor.blue:
                setColor = defaultMatDict[FaceName.Left];
                break;
            case CubeColor.red:
                setColor = defaultMatDict[FaceName.Up];
                break;
            case CubeColor.orange:
                setColor = defaultMatDict[FaceName.Down];
                break;
        }
        return setColor;
    }
}
