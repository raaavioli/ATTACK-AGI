using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ImgToScreen : MonoBehaviour
{
    private byte[] img;
    Texture2D imageTexture;
    private RawImage displayImage;

    void Start()
    {
        imageTexture = new Texture2D(1920, 1080);
        displayImage = gameObject.GetComponent<RawImage>();
    }

    void Update()
    {
        if(ServerHandler.instance.rdyToFetchData) {
            img = ServerHandler.instance.data;
            if (img != null)
            {
                imageTexture.LoadRawTextureData(img);
                imageTexture.Apply();
                displayImage.texture = imageTexture;
            }
        }
    }
}
