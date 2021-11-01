
using System;
using UnityEngine;
using TouchScript.Gestures;

public class PauseTouchScript : MonoBehaviour
{
    GameManager gameManager;
    PressGesture press;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable() {
        press = GetComponent<PressGesture>();
        press.Pressed += pressHandler;
    }

    private void OnDisable() {
        press.Pressed -= pressHandler;
    }

    private void pressHandler(object sender, EventArgs eventArgs) {
        gameManager.PauseGame();
        press.Cancel(true, true);
    }
}
