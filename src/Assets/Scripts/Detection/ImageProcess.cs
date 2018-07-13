﻿using OpenCVForUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_EDITOR
using System.Threading.Tasks;
#endif

public class ImageProcess : MonoBehaviour
{
#if !UNITY_EDITOR

    private Dictionary<FaceName, List<CubeColor>> rubikFaces = new Dictionary<FaceName, List<CubeColor>>();
    private static List<CubeColor> colorsList = new List<CubeColor>()
    {
        CubeColor.blue,
        CubeColor.red,
        CubeColor.yellow,
        CubeColor.green,
        CubeColor.black,
        CubeColor.orange
    };
    bool done = false;
    
    void Update()
    {
        if (done == true)
            return;
        if (rubikFaces.Keys.Count != 6)
        {
            if (VideoPanel.imgTextures.Count > 0)
            {
                ScanPhoto();
            }
        }
        else
        {
            print(" -  Stop maping colors on HoloCube - ");
            done = true;
            StopVideoMode();
            CubeSolver.Instance.StartSolving(rubikFaces);
        }
    }

    private void ScanPhoto()
    {
        Texture2D imgTexture = VideoPanel.imgTextures.Dequeue();

        Mat imgMat = GetPhoto(imgTexture);
        Mat procImage = new Mat();
        Mat eq_img = new Mat();

        HistogramEqualizer(imgMat, eq_img);
        Imgproc.cvtColor(eq_img, procImage, Imgproc.COLOR_RGB2HSV);

        List<Mat> maskList = GetColorsMask(procImage);
        List<Cubies> cubies = new List<Cubies>();

        for (int i = 0; i < maskList.Count; i++)
        {
            Mat newImage = new Mat();
            Core.bitwise_and(procImage, procImage, newImage, maskList[i]);
            List<MatOfPoint> contours = GetContours(newImage);
            GetCubies(contours, imgMat, i, cubies);
        }

        if (cubies != null)
        {
            Resources.UnloadUnusedAssets();
            GetFaceColors(cubies);
        }
        else
        {
            Debug.Log("Cubes: " + cubies.Count);
        }

        Utils.matToTexture2D(imgMat, imgTexture);
    }

    private void StopVideoMode()
    {
        Camera.main.GetComponent<VideoPanelApp>().enabled = false;
        //VideoPanelApp.Instance.StopVideoMode();
        GameObject.Find("Video Panel").SetActive(false);
        GameObject.Find("Camera Stream").SetActive(false);
    }

    private Mat GetPhoto(Texture2D imgTexture)
    {
        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(imgTexture, imgMat);

        return imgMat;
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

    private List<Mat> GetColorsMask(Mat procImage)
    {
        Mat blueMask = new Mat();
        Mat redMask = new Mat();
        Mat orangeMask = new Mat();
        Mat yellowMask = new Mat();
        Mat greenMask = new Mat();
        Mat blackMask = new Mat();
        List<Mat> maskList = new List<Mat>();

        int sensitivity = 15;
        Core.inRange(procImage, new Scalar(90, 130, 130), new Scalar(130, 255, 255), blueMask);
        Core.inRange(procImage, new Scalar(160, 128, 128), new Scalar(180, 255, 255), redMask);
        Core.inRange(procImage, new Scalar(20, 120, 120), new Scalar(40, 255, 255), yellowMask);
        Core.inRange(procImage, new Scalar(40, 100, 100), new Scalar(80, 255, 255), greenMask);
        Core.inRange(procImage, new Scalar(0, 0, 0), new Scalar(180, 255, 60), blackMask);
        //Core.inRange(procImage, new Scalar(0, 0, 255 - sensitivity), new Scalar(255, sensitivity, 255), blackMask);
        Core.inRange(procImage, new Scalar(0, 128, 128), new Scalar(22, 255, 255), orangeMask);

        maskList.Add(blueMask);
        maskList.Add(redMask);
        maskList.Add(yellowMask);
        maskList.Add(greenMask);
        maskList.Add(blackMask);
        maskList.Add(orangeMask);

        return maskList;
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
            var area = Imgproc.contourArea(contour);
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

        Debug.Log("Number of cubies: " + cubies.Count);
    }

    private void GetFaceColors(List<Cubies> cubies)
    {
        List<CubeColor> facesColors = new List<CubeColor>();

        if (cubies.Count == 9)
        {
            cubies.Sort((cube1, cube2) => cube1.y.CompareTo(cube2.y));
            for (int i = 0; i < cubies.Count; i += 3)
            {
                List<Cubies> sublist = cubies.GetRange(i, 3);
                sublist.Sort();
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
                        rubikFaces.Add(FaceName.Up, facesColors);
                    break;
                case CubeColor.blue:
                    if (!rubikFaces.ContainsKey(FaceName.Left))
                        rubikFaces.Add(FaceName.Left, facesColors);
                    break;
                case CubeColor.black:
                    if (!rubikFaces.ContainsKey(FaceName.Front))
                        rubikFaces.Add(FaceName.Front, facesColors);
                    break;
                case CubeColor.orange:
                    if (!rubikFaces.ContainsKey(FaceName.Down))
                        rubikFaces.Add(FaceName.Down, facesColors);
                    break;
                case CubeColor.green:
                    if (!rubikFaces.ContainsKey(FaceName.Right))
                        rubikFaces.Add(FaceName.Right, facesColors);
                    break;
                case CubeColor.yellow:
                    if (!rubikFaces.ContainsKey(FaceName.Back))
                        rubikFaces.Add(FaceName.Back, facesColors);
                    break;
            }

            Debug.Log("Number of faces detected: " + rubikFaces.Count);
            foreach (var rubick in rubikFaces)
            {
                foreach (var value in rubick.Value)
                {
                    Debug.Log(value);
                }
            }
        }
    }

    // Thread part optimization 20 % :)
    private void ScanPhotoInThread()
    {
        Texture2D imgTexture = null;        
        Mat procImage = new Mat();
        imgTexture = VideoPanel.imgTextures.Dequeue();
        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC3);
        UnityEngine.WSA.Application.InvokeOnAppThread(() => {            
            Utils.texture2DToMat(imgTexture, imgMat);
        }, true);

        Imgproc.cvtColor(imgMat, procImage, Imgproc.COLOR_BGR2HSV);
        Mat blueMask = new Mat();
        Mat redMask = new Mat();
        Mat orangeMask = new Mat();
        Mat yellowMask = new Mat();
        Mat greenMask = new Mat();
        Mat blackMask = new Mat();
        List<Mat> maskList = new List<Mat>();

        Core.inRange(procImage, new Scalar(90, 100, 100), new Scalar(130, 255, 255), blueMask);
        Core.inRange(procImage, new Scalar(160, 100, 100), new Scalar(180, 255, 255), redMask);
        Core.inRange(procImage, new Scalar(22, 80, 80), new Scalar(38, 255, 255), yellowMask);
        Core.inRange(procImage, new Scalar(40, 50, 50), new Scalar(80, 255, 255), greenMask);
        Core.inRange(procImage, new Scalar(0, 0, 0), new Scalar(180, 255, 55), blackMask);
        Core.inRange(procImage, new Scalar(0, 100, 100), new Scalar(22, 255, 255), orangeMask);


        maskList.Add(blueMask);
        maskList.Add(redMask);
        maskList.Add(yellowMask);
        maskList.Add(greenMask);
        maskList.Add(blackMask);
        maskList.Add(orangeMask);
        
        List<Cubies> cubies = new List<Cubies>();
        List<CubeColor> facesColors = new List<CubeColor>();
        
        for (int i = 0; i < maskList.Count; i++)
        {
            Mat newImage = new Mat();
            Core.bitwise_and(procImage, procImage, newImage, maskList[i]);

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
                var area = Imgproc.contourArea(contour);
                if (area > 500)
                    bigContours.Add(contour);
            }

            MatOfPoint2f matOfPoint2f = new MatOfPoint2f();
            MatOfPoint2f approxCurve = new MatOfPoint2f();
            MatOfPoint approx = new MatOfPoint();

            foreach (var contour in bigContours)
            {
                matOfPoint2f.fromList(contour.toList());
                Imgproc.approxPolyDP(matOfPoint2f, approxCurve, 0.1 * Imgproc.arcLength(matOfPoint2f, true), true);

                try
                {
                    approxCurve.convertTo(approx, CvType.CV_32S);
                    OpenCVForUnity.Rect rect = Imgproc.boundingRect(approx);

                    if (approx.total() == 4)
                    {
                        cubies.Add(new Cubies(rect.x, rect.y, colorsList[i]));

                        Imgproc.rectangle(imgMat, new Point(rect.x, rect.y), new Point(rect.x + rect.width, rect.y + rect.height), new Scalar(0, 255, 0), 1);
                    }
                }
                catch (ArgumentOutOfRangeException e) { }
            }
        }

        Debug.Log("Number of cubies: " + cubies.Count);

        if (cubies.Count == 9)
        {
            cubies.Sort((cube1, cube2) => cube1.y.CompareTo(cube2.y));
            for (int i = 0; i < cubies.Count; i += 3)
            {
                var sublist = cubies.GetRange(i, 3);
                sublist.Sort();
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
                        rubikFaces.Add(FaceName.Up, facesColors);
                    break;
                case CubeColor.blue:
                    if (!rubikFaces.ContainsKey(FaceName.Left))
                        rubikFaces.Add(FaceName.Left, facesColors);
                    break;
                case CubeColor.black:
                    if (!rubikFaces.ContainsKey(FaceName.Front))
                        rubikFaces.Add(FaceName.Front, facesColors);
                    break;
                case CubeColor.orange:
                    if (!rubikFaces.ContainsKey(FaceName.Down))
                        rubikFaces.Add(FaceName.Down, facesColors);
                    break;
                case CubeColor.green:
                    if (!rubikFaces.ContainsKey(FaceName.Right))
                        rubikFaces.Add(FaceName.Right, facesColors);
                    break;
                case CubeColor.yellow:
                    if (!rubikFaces.ContainsKey(FaceName.Back))
                        rubikFaces.Add(FaceName.Back, facesColors);
                    break;
            }

            Debug.Log("Number of faces detected: " + rubikFaces.Count);
            foreach(var rubick in rubikFaces)
            {
                foreach(var value in rubick.Value)
                {
                    Debug.Log(value);
                }
            }
        }
        //UnityEngine.WSA.Application.InvokeOnAppThread(() => {
        //    Utils.matToTexture2D(imgMat, imgTexture);
        //}, false);
    }
#endif
}