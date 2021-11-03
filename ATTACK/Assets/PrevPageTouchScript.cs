
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TouchScript.Gestures;

public class PrevPageTouchScript : MonoBehaviour {
    [SerializeField]
    GameObject textBox;

    TextBoxScript textBoxScript;
    PressGesture press;
    bool delay;
    // Start is called before the first frame update
    void Start() {
        textBoxScript = textBox.GetComponent<TextBoxScript>();
        delay = false;
    }

    // Update is called once per frame
    void Update() {

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
            textBoxScript.PrevPage();
            StartCoroutine(WaitUntilReady(0.5f));
        }
        press.Cancel(true, true);
    }

    IEnumerator WaitUntilReady(float value) {
        delay = true;
        yield return new WaitForSeconds(value);
        delay = false;
    }
}
