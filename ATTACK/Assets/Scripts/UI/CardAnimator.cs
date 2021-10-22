using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardAnimator : MonoBehaviour
{
    [Range(0.1f, 10)]
    [SerializeField]
    private float Period = 1.0f;

    [Range(0f, 1f)]
    [SerializeField]
    private float StartRotation = 0.0f;

    private Color DefaultMaterialColor;
    public Color _Color;
    public bool _Rotating;  

    Material material;

    void Start()
    {
        material = Instantiate(GetComponent<Image>().material);
        GetComponent<Image>().material = material;
        DefaultMaterialColor = material.GetColor("_Color");
        _Color = DefaultMaterialColor;
    }

    void Update()
    {
        float Ang01 = StartRotation + (Time.time / Period);
        Ang01 -= (int)Ang01;
        material.SetFloat("_Angle", Ang01);
        material.SetColor("_Color", _Color);
        material.SetFloat("_Rotating", _Rotating ? 1.0f : 0.0f);
    }
}
