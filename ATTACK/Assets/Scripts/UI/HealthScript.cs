using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour {
    private Slider healthBar;

    void Start() {
        healthBar = transform.Find("HealthBar").GetComponent<Slider>();
    }

    public void SetHealth(float value) {
        healthBar.value = value;
    }
}
