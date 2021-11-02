
using System;
using UnityEngine;
using TouchScript.Gestures;

public class TapToHealTouchScript : MonoBehaviour {
    GameManager gameManager;
    TapGesture tap;
    // Start is called before the first frame update
    void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnEnable() {
        GetComponent<TapGesture>().Tapped += tapHandler;
    }

    private void OnDisable() {
        GetComponent<TapGesture>().Tapped -= tapHandler;
    }

    private void tapHandler(object sender, EventArgs eventArgs) {
        Debug.Log(GetComponent<TapGesture>().NormalizedScreenPosition);
    }
}
