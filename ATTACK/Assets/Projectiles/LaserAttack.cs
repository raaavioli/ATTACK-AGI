using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LaserAttack : Attack
{
    public Material LaserMaterial;

    private float Length = 15.0f;
    private GameObject Laser;
    protected override void InstantiateProjectile()
    {
        Laser = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Laser.GetComponent<Renderer>().material = LaserMaterial;
        Vector3 Rotation = Laser.transform.rotation.eulerAngles;
        Rotation.x = 90;
        Rotation.y = gameObject.transform.eulerAngles.y;
        Laser.transform.eulerAngles = Rotation;
        Laser.transform.position = gameObject.transform.position + Laser.transform.up * Length;


        Laser.SetActive(false);
    }

    protected override void StartProjectile()
    {
        Laser.SetActive(true);
    }
    protected override void UpdateProjectile()
    {
        float MaxRadius = 0.7f;
        float t = ((SimulationTime - FireStartTime) / MaxFireTime) * (float) Math.PI;
        float Radius = (float) Math.Sin(t) * MaxRadius;
        Laser.transform.localScale = new Vector3(Radius, Length, Radius);
    }

    protected override void StopProjectile()
    {
        Laser.SetActive(false);
    }

}
