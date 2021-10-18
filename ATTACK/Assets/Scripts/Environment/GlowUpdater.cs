using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowUpdater : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField]
    private float AngleOffset;
    [Range(0.1f, 10)]
    [SerializeField]
    private float Period;

    public Color DefaultMaterialColor { get; private set; }
    public Color _Color;

    MaterialPropertyBlock Mpb;

    void Start()
    {
        Mpb = new MaterialPropertyBlock();
        DefaultMaterialColor = GetComponent<Renderer>().material.GetColor("_Color");
        _Color = DefaultMaterialColor;
        Period = 1;
    }

    void Update()
    {
        float Ang01 = (AngleOffset + Time.time) / Period;
        Ang01 -= (int)Ang01;
        if (Mpb != null)
        {
            Mpb.SetFloat("_Angle", Ang01);
            Mpb.SetColor("_Color", _Color);
            GetComponent<Renderer>().SetPropertyBlock(Mpb);
        }
    }
}
