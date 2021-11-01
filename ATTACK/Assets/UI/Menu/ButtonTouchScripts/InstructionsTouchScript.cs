
using System;
using UnityEngine;
using TouchScript.Gestures;

public class InstructionsTouchScript : MonoBehaviour
{
    MenuScript menu;
    PressGesture press;
    // Start is called before the first frame update
    void Start()
    {
        menu = GameObject.Find("Menu").GetComponent<MenuScript>();
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
        menu.ShowInstructions();
        press.Cancel(true, true);
    }
}
