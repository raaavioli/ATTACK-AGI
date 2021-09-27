using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglePropertyUpdater : MonoBehaviour
{
    [Range(0, 1)]
    public float AngleOffset;
    [Range(0.1f, 10)]
    public float Period;

    MaterialPropertyBlock Mpb;

    void Start()
    {
        Mpb = new MaterialPropertyBlock();
        Period = 1;
    }

    void Update()
    {
        float Ang01 = (AngleOffset + Time.time) / Period;
        Ang01 -= (int)Ang01;
        Mpb.SetFloat("_Angle", Ang01);
        GetComponent<Renderer>().SetPropertyBlock(Mpb);
    }
}
