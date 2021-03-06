﻿using OpenCVForUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class ImageProcess : MonoBehaviour
{
    private static Queue<Texture2D> imgTextures;

    private int _width, _height;

    private Dictionary<FaceName, List<CubeColor>> rubikFaces;

    private static List<CubeColor> colorsList;

    void Start()
    {
        imgTextures = new Queue<Texture2D>();
        rubikFaces = new Dictionary<FaceName, List<CubeColor>>();
        colorsList = new List<CubeColor>()
        {
            CubeColor.blue,
            CubeColor.red,
            CubeColor.yellow,
            CubeColor.green,
            CubeColor.black,
            CubeColor.orange
        };
        StartCoroutine(StartProcessing());
    }

    IEnumerator StartProcessing()
    {
        while (rubikFaces.Keys.Count != 6)
        {
            if (imgTextures.Count > 0)
            {
                ScanPhoto();
            }

            yield return new WaitForSeconds(2);
        }

        VideoPanelApp.Instance.StopVideoMode();

        yield return new WaitForSeconds(2);

        SolveCubeAsync();
    }


    public async Task SolveCubeAsync()
    {
        Rotation.Instance.SetCubeForTutorial();
        string result = await Task.Run(() => CubeSolver.Instance.GetSolvingMoves(rubikFaces));
        Rotation.Instance.StartSolvingAnimations(result);
    }

    public void SetResolution(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public void SetBytes(byte[] image)
    {
        if (imgTextures.Count == 0)
        {
            Texture2D imgTexture = new Texture2D(_width, _height, TextureFormat.BGRA32, false);
            imgTexture.LoadRawTextureData(image);
            imgTexture.Apply();

            imgTextures.Enqueue(imgTexture);
        }
    }

    private void ScanPhoto()
    {
        Texture2D imgTexture = imgTextures.Peek();

        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(imgTexture, imgMat);

        Mat procImage = new Mat();
        Mat eq_img = new Mat();

        Imgproc.cvtColor(imgMat, procImage, Imgproc.COLOR_RGB2HSV);

        List<Mat> maskList = new List<Mat>();
        List<Cubies> cubies = new List<Cubies>();

        GetColorsMask(procImage, maskList);

        for (int i = 0; i < maskList.Count; i++)
        {
            Mat newImage = new Mat();
            Core.bitwise_and(procImage, procImage, newImage, maskList[i]);
            List<MatOfPoint> contours = GetContours(newImage);
            GetCubies(contours, imgMat, i, cubies);
        }

        if (cubies != null)
        {
            GetFaceColors(cubies);
        }
        else
        {
            print("Cubes: " + cubies.Count);
        }

        imgTextures.Dequeue();
        Resources.UnloadUnusedAssets();
    }

    private void StopVideoMode()
    {
        VideoPanelApp.Instance.StopVideoMode();
        GameObject.Find("Video Panel").SetActive(false);
        GameObject.Find("Camera Stream").SetActive(false);
    }

    private void ClaheEqualizer(Mat source, Mat destination, int clipLimit = 4)
    {
        Imgproc.cvtColor(source, destination, Imgproc.COLOR_RGB2Lab);
        List<Mat> img_ch = new List<Mat>(3);
        Core.split(destination, img_ch);
        CLAHE clahe = Imgproc.createCLAHE();
        clahe.setClipLimit(clipLimit);
        Mat light_ch = new Mat();
        clahe.apply(img_ch[0], light_ch);
        light_ch.copyTo(img_ch[0]);
        Core.merge(img_ch, destination);
        Imgproc.cvtColor(destination, destination, Imgproc.COLOR_Lab2RGB);
    }

    private void HistogramEqualizer(Mat source, Mat destinaton)
    {
        Mat ycrcb = new Mat();
        Imgproc.cvtColor(source, ycrcb, Imgproc.COLOR_RGB2YCrCb);

        List<Mat> channels = new List<Mat>();
        Core.split(ycrcb, channels);

        Imgproc.equalizeHist(channels[0], channels[0]);

        Core.merge(channels, ycrcb);
        Imgproc.cvtColor(ycrcb, destinaton, Imgproc.COLOR_YCrCb2RGB);
    }

    private void GetColorsMask(Mat procImage, List<Mat> maskList)
    {
        Mat blueMask = new Mat();
        Mat redMask = new Mat();
        Mat redMaskHigh = new Mat();
        Mat redMaskLow = new Mat();
        Mat orangeMask = new Mat();
        Mat yellowMask = new Mat();
        Mat greenMask = new Mat();
        Mat blackMask = new Mat();

        Core.inRange(procImage, new Scalar(90, 80, 80), new Scalar(130, 255, 255), blueMask);
        Core.inRange(procImage, new Scalar(160, 80, 80), new Scalar(180, 255, 255), redMaskHigh);
        Core.inRange(procImage, new Scalar(0, 80, 80), new Scalar(5, 255, 255), redMaskLow);
        Core.inRange(procImage, new Scalar(20, 80, 80), new Scalar(40, 255, 255), yellowMask);
        Core.inRange(procImage, new Scalar(40, 80, 80), new Scalar(80, 255, 255), greenMask);
        Core.inRange(procImage, new Scalar(0, 0, 0), new Scalar(180, 255, 61), blackMask);
        /*
            white color threshold
            Core.inRange(procImage, new Scalar(0, 0, 235), new Scalar(180, 20, 255), blackMask);
         */
        Core.inRange(procImage, new Scalar(5, 80, 80), new Scalar(20, 255, 255), orangeMask);
        Core.bitwise_or(redMaskHigh, redMaskLow, redMask);
        
        maskList.Add(blueMask);
        maskList.Add(redMask);
        maskList.Add(yellowMask);
        maskList.Add(greenMask);
        maskList.Add(blackMask);
        maskList.Add(orangeMask);
    }
    
    private List<MatOfPoint> GetContours(Mat newImage)
    {
        Mat edges = new Mat();
        Mat hierarchy = new Mat();
        List<MatOfPoint> contours = new List<MatOfPoint>();
        List<MatOfPoint> bigContours = new List<MatOfPoint>();

        Imgproc.GaussianBlur(newImage, newImage, new Size(3, 3), 1);
        Imgproc.Canny(newImage, edges, 75, 255);
        Imgproc.dilate(edges, edges, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(1, 1)), new Point(-1, -1), 2);
        Imgproc.findContours(edges, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

        foreach (var contour in contours)
        {
            double area = Imgproc.contourArea(contour);
            if (area > 500)
                bigContours.Add(contour);
        }

        return bigContours;
    }

    private void GetCubies(List<MatOfPoint> contours, Mat imgMat, int index, List<Cubies> cubies)
    {
        MatOfPoint2f matOfPoint2f = new MatOfPoint2f();
        MatOfPoint2f approxCurve = new MatOfPoint2f();
        MatOfPoint approx = new MatOfPoint();

        foreach (var contour in contours)
        {
            matOfPoint2f.fromList(contour.toList());
            Imgproc.approxPolyDP(matOfPoint2f, approxCurve, 0.1 * Imgproc.arcLength(matOfPoint2f, true), true);

            try
            {
                approxCurve.convertTo(approx, CvType.CV_32S);
                OpenCVForUnity.Rect rect = Imgproc.boundingRect(approx);

                if (approx.total() == 4)
                {
                    cubies.Add(new Cubies(rect.x, rect.y, colorsList[index]));
                    Imgproc.rectangle(imgMat, new Point(rect.x, rect.y), new Point(rect.x + rect.width, rect.y + rect.height), new Scalar(255, 40, 150), 2);
                }
            }
            catch (ArgumentOutOfRangeException e) { }
        }

        print("Number of cubies: " + cubies.Count);
    }

    private void GetFaceColors(List<Cubies> cubies)
    {
        if (cubies.Count == 9)
        {
            List<CubeColor> facesColors = new List<CubeColor>();

            cubies.Sort((cube1, cube2) => cube1.y.CompareTo(cube2.y));
            for (int i = 0; i < cubies.Count; i += 3)
            {
                List<Cubies> sublist = cubies.GetRange(i, 3);
                sublist.Sort((cube1, cube2) => cube1.x.CompareTo(cube2.x));
                sublist.Reverse();
                foreach (var cube in sublist)
                {
                    facesColors.Add(cube.color);
                }
            }

            switch (facesColors[4])
            {
                case CubeColor.red:
                    if (!rubikFaces.ContainsKey(FaceName.Up))
                    {
                        rubikFaces.Add(FaceName.Up, facesColors);
                        GoToNextFace();
                    }
                    break;
                case CubeColor.blue:
                    if (!rubikFaces.ContainsKey(FaceName.Left))
                    {
                        rubikFaces.Add(FaceName.Left, facesColors);
                        GoToNextFace();
                    }
                    break;
                case CubeColor.black:
                    if (!rubikFaces.ContainsKey(FaceName.Front))
                    {
                        rubikFaces.Add(FaceName.Front, facesColors);
                        GoToNextFace();
                    }
                    break;
                case CubeColor.orange:
                    if (!rubikFaces.ContainsKey(FaceName.Down))
                    {
                        rubikFaces.Add(FaceName.Down, facesColors);
                        GoToNextFace();
                    }
                    break;
                case CubeColor.green:
                    if (!rubikFaces.ContainsKey(FaceName.Right))
                    {
                        rubikFaces.Add(FaceName.Right, facesColors);
                        GoToNextFace();
                    }
                    break;
                case CubeColor.yellow:
                    if (!rubikFaces.ContainsKey(FaceName.Back))
                    {
                        rubikFaces.Add(FaceName.Back, facesColors);
                        GoToNextFace();
                    }
                    break;
                default:
                    print("Are you sure you're using a standard rubik cube ?");
                    break;
            }

            print("Number of faces detected: " + rubikFaces.Count);
        }
    }

    public void GoToNextFace()
    {
        Rotation.Instance.SetColors(rubikFaces);

        if (rubikFaces.Count < 4)
        {
            Rotation.Instance.GetNextFace(Vector3.up, 90f);
        }
        else if (rubikFaces.Count == 4)
        {
            Rotation.Instance.GetNextFace(Vector3.up, Vector3.right, 90f, -90f);
        }
        else if (rubikFaces.Count == 5)
        {
            Rotation.Instance.GetNextFace(Vector3.right, 180f);
        }
        else
        {
            Rotation.Instance.GetNextFace(Vector3.right, -90f);
        }
    }
}