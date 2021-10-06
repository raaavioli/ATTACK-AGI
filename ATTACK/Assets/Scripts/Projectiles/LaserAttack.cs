using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LaserAttack : Attack
{
    public Material LaserMaterial;

    private GameObject Laser;
    protected override void InstantiateProjectile()
    {
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
        float MaxRadius = 0.7f;
        float t = ((SimulationTime - FireStartTime) / MaxFireTime) * (float) Math.PI;
        float Radius = (float) Math.Sin(t) * MaxRadius;
        UpdateTransform(Radius);
    }

    protected override void StopProjectile()
    {
        Laser.SetActive(false);
    }

    private void UpdateTransform (float Radius)
    {
        // Make Length based on target position
        float Length = 25.0f;
        Vector3 Rotation = Laser.transform.rotation.eulerAngles;
        Rotation.x = 90;
        Rotation.y = gameObject.transform.eulerAngles.y;
        Laser.transform.localScale = new Vector3(Radius, Length, Radius);
        Laser.transform.eulerAngles = Rotation;
        Laser.transform.position = gameObject.transform.position + Laser.transform.up * Length;
    }

    private void OnDestroy()
    {
        Destroy(Laser);
    }

    private void OnDisable()
    {
        Laser.SetActive(false);
    }

}
