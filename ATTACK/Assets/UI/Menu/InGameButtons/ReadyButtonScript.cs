using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButtonScript : MonoBehaviour
{
    Text text;
    Image image;
    bool activated;

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponentInChildren<Text>();
        image = gameObject.GetComponentInChildren<Image>();
        activated = false;
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

    public bool IsActive() { return activated; }

    void Activate()
    {
        activated = true;
        text.color = Color.black;
        image.color = Color.white;
    }

    public void Deactivate()
    {
        activated = false;
        text.color = Color.white;
        image.color = new Color(0.4392157f, 0.2588235f, 0.07843138f);
    }
}
