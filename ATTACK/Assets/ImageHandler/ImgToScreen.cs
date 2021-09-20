using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ImgToScreen : MonoBehaviour
{
    [SerializeField]
    private int sqrtSize = 128;

    private byte[] img;
    Texture2D imageTexture;
    private Image displayImage;

    void Start()
    {
        imageTexture = new Texture2D(sqrtSize, sqrtSize, TextureFormat.RGB24, false);
        displayImage = gameObject.GetComponent<Image>();
    }

    void Update()
    {
        if (ServerHandler.instance.rdyToFetchData) {
            byte[] rawBytes = ServerHandler.instance.fetchData();
            img = convertTextureFormat(rawBytes);
            if (img != null)
            {
                try {
                    imageTexture.LoadRawTextureData(img);
                    imageTexture.Apply();
                    displayImage.sprite = Sprite.Create(imageTexture, new Rect(0.0f, 0.0f, sqrtSize, sqrtSize), new Vector2(0.5f, 0.5f), 100.0f);
                } catch (UnityException e) {
                    Debug.Log(e);
				}
            }
        }
    }

    private byte[] convertTextureFormat(byte[] array) {
        byte[] result = new byte[array.Length * 3];
        for (int i = 0; i < array.Length; ++i) {
            result[i * 3] = array[i];
            result[i * 3 + 1] = array[i];
            result[i * 3 + 2] = array[i];
        }
        return result;
	}
}
