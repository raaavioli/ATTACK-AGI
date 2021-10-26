using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButtonScript : MonoBehaviour
{
    Text text;
    Image image;
    public bool rightPlayer;
    bool activated;
    public GameManager gameManager;
    Color previousTextColor;
    Color previousImageColor;

    private void SwitchColor()
    {
        Color temp = previousTextColor;
        previousTextColor = text.color;
        text.color = temp;
        temp = previousImageColor;
        previousImageColor = image.color;
        image.color = temp;
    }

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponentInChildren<Text>();
        image = gameObject.GetComponentInChildren<Image>();
        previousTextColor = Color.black;
        previousImageColor = Color.white;
        activated = false;
    }

    void Update()
    {
        if(activated && gameManager.ResetReadyButtons())
        {
            Deactivate();
        }
    }

    public void OnClick()
    {
        if(!activated)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    void Activate()
    {
        activated = true;
        gameManager.PlayerReady(rightPlayer, true);
        SwitchColor();
    }

    void Deactivate()
    {
        activated = false;
        gameManager.PlayerReady(rightPlayer, false);
        SwitchColor();
    }
}
