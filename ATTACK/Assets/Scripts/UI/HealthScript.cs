using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour {
    private Material healthMaterial;

    void Start() {
        Image image = transform.Find("HealthBar").GetComponent<Image>();
        healthMaterial = Instantiate(transform.Find("HealthBar").GetComponent<Image>().material);
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
