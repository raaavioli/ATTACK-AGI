using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntensityUpdater : MonoBehaviour
{
    [Range(1, 10)]
    public float Lifetime;

    private float Starttime;

    private void Start()
    {
        Starttime = Time.time;
    }

    void Update()
    {
        float Intensity = 1 - 4 * (Time.time - Starttime) / Lifetime;
        if (Intensity < 0)
            Intensity = 0;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
            r.material.SetFloat("_Intensity", Intensity);
        }
    }
}
