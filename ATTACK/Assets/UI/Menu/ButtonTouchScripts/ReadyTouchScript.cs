
using System;
using System.Collections;
using UnityEngine;
using TouchScript.Gestures;

public class ReadyTouchScript : MonoBehaviour
{
    ReadyButtonScript ready;
    PressGesture press;
    bool delay;
    // Start is called before the first frame update
    void Start()
    {
        ready = gameObject.GetComponent<ReadyButtonScript>();
        delay = false;
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
        if (!delay) {
            ready.OnClick();
            StartCoroutine(WaitUntilReady(0.1f));
        }
        press.Cancel(true, true);
    }

    IEnumerator WaitUntilReady(float value) {
        delay = true;
        yield return new WaitForSeconds(value);
        delay = false;
    }
}
