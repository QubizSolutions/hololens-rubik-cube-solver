﻿//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPanel : MonoBehaviour
{
    public RawImage rawImage;

    [HideInInspector]
    public static Queue<Texture2D> imgTextures = new Queue<Texture2D>();

    public void SetResolution(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
        rawImage.texture = texture;
    }

    public void SetBytes(byte[] image)
    {
        var texture = rawImage.texture as Texture2D;
        texture.LoadRawTextureData(image); //TODO: Should be able to do this: texture.LoadRawTextureData(pointerToImage, 1280 * 720 * 4);
        texture.Apply();
        
        if(imgTextures.Count < 5)
        {
            imgTextures.Enqueue(texture);
        }
    }
}