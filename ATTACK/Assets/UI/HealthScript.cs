using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
    private Slider healthBar;

    // Start is called before the first frame update
    void Start()
    {
        healthBar = transform.Find("HealthBar").GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        //healthBar.value -= 0.001f;
    }
    public void SetHealth(float value)
    {
        healthBar.value = value;
    }
}
