using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    public VisualElement[] leftCardBoxes;
    public VisualElement[] rightCardBoxes;
    public int test;

    // Start is called before the first frame update
    void Start() {
        var root = GetComponent<UIDocument>().rootVisualElement;

        leftCardBoxes = new VisualElement[6];
        rightCardBoxes = new VisualElement[6];

        for(int i = 0; i < 6; i++) {
            leftCardBoxes[i] = root.Q<VisualElement>("LCardBox" + (i + 1));
            rightCardBoxes[i] = root.Q<VisualElement>("RCardBox" + (i + 1));
        }

        test = 0;
    }

    // Update is called once per frame
    void Update() {
        if (test % 100 == 0) {
            if ((test / 100) % 12 < 6) {
                updateBorderColor(leftCardBoxes[(test / 100) % 6]);

            }
            else {
                updateBorderColor(rightCardBoxes[(test / 100) % 6]);
            }
        }
        //Debug.Log(test++);
        test++;
    }

    void updateBorderColor(VisualElement e) {
        float oldColor = e.style.borderLeftColor.value.r;
        Color newColor = new Color((oldColor + 1) % 2, (oldColor + 1) % 2, (oldColor + 1) % 2, 1f);
        e.style.borderLeftColor = newColor;
        e.style.borderTopColor = newColor;
        e.style.borderRightColor = newColor;
        e.style.borderBottomColor = newColor;
    }
}
