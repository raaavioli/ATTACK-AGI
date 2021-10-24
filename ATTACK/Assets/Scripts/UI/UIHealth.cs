using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour {
    private Material healthMaterial;

    void Start() {
        Image image = GetComponent<Image>();
        healthMaterial = Instantiate(image.material);
        image.material = healthMaterial;
    }

    public void SetHealth(float value) {
        if (value < 0)
            value = 0f;
        if (value > 1)
            value = 1f;
        healthMaterial.SetFloat("_Health", value);
    }
}
