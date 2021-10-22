using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowUpdater : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField]
    private float AngleOffset;
    private float RandomOffset;

    [Range(0.1f, 10)]
    [SerializeField]
    private float Period = 1.0f;

    public Color _Color;

    MaterialPropertyBlock Mpb;

    void Start()
    {
        Mpb = new MaterialPropertyBlock();
        _Color = GetComponent<Renderer>().material.GetColor("_Color");
        RandomOffset = Random.value;
    }

    void Update()
    {
        float Ang01 = (RandomOffset + AngleOffset + Time.time) / Period;
        Ang01 -= (int)Ang01;
        if (Mpb != null)
        {
            Mpb.SetFloat("_Angle", Ang01);
            Mpb.SetColor("_Color", _Color);
            GetComponent<Renderer>().SetPropertyBlock(Mpb);
        }
    }
}
