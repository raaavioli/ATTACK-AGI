using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LaserAttack : Attack
{
    public Material LaserMaterial;

    private GameObject Laser;
    private MaterialPropertyBlock Mpb;
    protected override void InstantiateProjectile()
    {
        Mpb = new MaterialPropertyBlock();
        Laser = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Laser.GetComponent<Renderer>().material = LaserMaterial;
        UpdateTransform(0f);
        Laser.SetActive(false);
    }

    protected override void StartProjectile()
    {
        Laser.SetActive(true);
    }
    protected override void UpdateProjectile()
    {
        if (Type == AttackType.Weak)
        {
            Mpb.SetColor("_CoreColor", new Color(1f, 0.67f, 0.30f));
            Mpb.SetColor("_OuterSwirlColor", new Color(0.95f, 0.74f, 0.18f));
            Mpb.SetColor("_InnerSwirlColor", new Color(0.83f, 0.31f, 0f));
            Laser.GetComponent<Renderer>().SetPropertyBlock(Mpb);
        } else if (Type == AttackType.Strong)
        {
            Mpb.SetColor("_CoreColor", new Color(0.19f, 0.764f, 0.80f));
            Mpb.SetColor("_OuterSwirlColor", new Color(0.024f, 0.37f, 0.46f));
            Mpb.SetColor("_InnerSwirlColor", new Color(0.30f, 0.21f, 0.90f));
            Laser.GetComponent<Renderer>().SetPropertyBlock(Mpb);
        }

        float MaxRadius = 0.7f;
        float t = (SimulationTime - FireStartTime) / MaxFireTime;
        float tStart = Utils.Remap(0, 0.2f, 0, 1, t);
        float tEnd = Utils.Remap(0.2f, 1, 1, 0, t);
        t = tStart * tEnd;
        float Radius = (float) Math.Sin(t * (float)Math.PI / 2) * MaxRadius;
        UpdateTransform(Radius);
    }

    protected override void StopProjectile()
    {
        Laser.SetActive(false);
    }

    private void UpdateTransform (float Radius)
    {
        float Length = (transform.position - TargetPosition).magnitude / 2.0f;
        Vector3 Rotation = Laser.transform.rotation.eulerAngles;
        Rotation.x = 90;
        Rotation.y = gameObject.transform.eulerAngles.y;
        Laser.transform.localScale = new Vector3(Radius, Length, Radius);
        Laser.transform.eulerAngles = Rotation;
        Laser.transform.position = gameObject.transform.position + Laser.transform.up * (Length + 0.5f);
    }

    private void OnDisable()
    {
        if (Laser != null)
            Laser.SetActive(false);
    }

}
