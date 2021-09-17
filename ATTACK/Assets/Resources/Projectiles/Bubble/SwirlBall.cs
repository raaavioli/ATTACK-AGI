using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwirlBall : MonoBehaviour
{
    [Range(0.1f, 10)]
    public float Radius = 1;
    public bool Inverted = false;

    private float TotalTime = 0;
    void Start()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        TotalTime += Time.deltaTime;
        double Period = 2.0 * Math.PI;
        //float zPos = ((float) Math.Sin(TotalTime * Period / 1.0f) + 1) * 10;
        int Direction = Inverted ? -1 : 1;
        gameObject.transform.position = 1 * new Vector3(Radius * (float) Math.Cos(TotalTime * Period * 5), Radius * (float)Math.Sin(Direction * TotalTime * Period * 5), 0);
    }
}
