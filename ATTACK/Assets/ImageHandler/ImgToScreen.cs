using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class ImgToScreen : MonoBehaviour
{
    private Sur40ToImg instance;
    private byte[] img;
    Texture2D imageTexture;
    private Image displayImage;

    void Start()
    {
        imageTexture = new Texture2D(1920, 1080);
        instance = new Sur40ToImg();
        displayImage = gameObject.GetComponent<Image>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            img = instance.getImage();
            if (img != null)
            {
                imageTexture.LoadRawTextureData(img);
                imageTexture.Apply();
                displayImage.image = imageTexture;
            }
        }
    }
}
