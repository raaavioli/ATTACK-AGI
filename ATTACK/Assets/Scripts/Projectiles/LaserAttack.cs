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

    protected override int GetMaxTargets()
    {
        return 1;
    }
    protected override void InstantiateProjectile()
    {
        Mpb = new MaterialPropertyBlock();
        Laser = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Laser.GetComponent<Renderer>().material = LaserMaterial;
        Laser.SetActive(false);
    }

    protected override void StartProjectile()
    {
        if (Mode == CharacterMode.Defensive)
        {
            // Blue-ish
            Mpb.SetColor("_CoreColor", new Color(0.19f, 0.764f, 0.80f));
            Mpb.SetColor("_OuterSwirlColor", new Color(0.024f, 0.37f, 0.46f));
            Mpb.SetColor("_InnerSwirlColor", new Color(0.30f, 0.21f, 0.90f));
        }
        else if (Mode == CharacterMode.Offensive)
        {
            // Red-ush
            Mpb.SetColor("_CoreColor", new Color(1f, 0.67f, 0.30f));
            Mpb.SetColor("_OuterSwirlColor", new Color(0.95f, 0.74f, 0.18f));
            Mpb.SetColor("_InnerSwirlColor", new Color(0.83f, 0.31f, 0f));
        }
        Laser.GetComponent<Renderer>().SetPropertyBlock(Mpb);
        Laser.SetActive(true);
    }
    protected override void UpdateProjectile()
    {
        float MaxRadius = 0.7f;
        float t = (SimulationTime - ChargeTime) / MaxFireTime;
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
        Vector3 Direction = TargetPositions[0] - AttackSource.transform.position;
        Vector3 Rotation = Quaternion.LookRotation(Direction).eulerAngles;
        Rotation.x = 90;
        Laser.transform.eulerAngles = Rotation;
        float Length = Direction.magnitude / 2.0f;
        Laser.transform.localScale = new Vector3(Radius, Length, Radius);
        Laser.transform.position = AttackSource.transform.position + Laser.transform.up * (Length + 0.5f);
    }

    private void OnDisable()
    {
        if (Laser != null)
            Laser.SetActive(false);
    }

}
