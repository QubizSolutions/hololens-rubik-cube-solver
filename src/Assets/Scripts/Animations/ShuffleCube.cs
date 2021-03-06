﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ShuffleCube {

    public void SetFacesColors(Vector3 origin, Dictionary<FaceName, List<CubeColor>> shuffledSeq)
    { 
        foreach(FaceName faceName in shuffledSeq.Keys)
        {
            SetMaterial(faceName, origin, shuffledSeq);
        }
    }

    private void SetMaterial(FaceName faceName, Vector3 origin, Dictionary<FaceName, List<CubeColor>> rubikFaces)
    {
        List<GameObject> cubes = GetFaceCubes(faceName, origin);
        for (int i = 0; i < rubikFaces[faceName].Count; i++)
        {
            GameObject cube = cubes[i];
            Material[] cubeMat = cube.GetComponentInChildren<Renderer>().materials; 
            var cubeColor = rubikFaces[faceName][i];
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
                foreach(GameObject cube in faceCubes.Where((c) => c.transform.localPosition.z == -2).OrderBy((c) => c.transform.localPosition.x).OrderByDescending((c) => c.transform.localPosition.y).ToList())
                {
                    listCubes.Add(cube);
                }
                break;
            case FaceName.Back:
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.z == 2).OrderByDescending((c) => c.transform.localPosition.x).OrderByDescending((c) => c.transform.localPosition.y).ToList())
                {                    
                    listCubes.Add(cube);                                            
                }
                break;
            case FaceName.Up:
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.y == 2).OrderBy((c) => c.transform.localPosition.x).OrderByDescending((c) => c.transform.localPosition.z).ToList())
                {
                    listCubes.Add(cube);                       
                }
                break;
            case FaceName.Down:
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.y == -2).OrderBy((c) => c.transform.localPosition.x).OrderBy((c) => c.transform.localPosition.z).ToList())
                {
                    listCubes.Add(cube);                        
                }
                break;
            case FaceName.Right:
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.x == 2).OrderBy((c) => c.transform.localPosition.z).OrderByDescending((c) => c.transform.localPosition.y).ToList())
                {
                    listCubes.Add(cube);                   
                }
                break;
            case FaceName.Left:
                foreach (GameObject cube in faceCubes.Where((c) => c.transform.localPosition.x == -2).OrderByDescending((c) => c.transform.localPosition.z).OrderByDescending((c) => c.transform.localPosition.y).ToList())
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
                setColor = new Color(0.26f, 0.26f, 0.26f);
                break;
            case CubeColor.green:
                setColor = Color.green;
                break;
            case CubeColor.yellow:
                setColor = Color.yellow;
                break;
            case CubeColor.blue:
                setColor = Color.blue;
                break;
            case CubeColor.red:
                setColor = Color.red;
                break;
            case CubeColor.orange:
                setColor = new Color(1f, 0.6f, 0f);
                break;
        }
        return setColor;
    }
}
