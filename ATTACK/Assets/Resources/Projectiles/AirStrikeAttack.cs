using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirStrikeAttack : Attack
{
    private GameObject Plane;
    protected override void InstantiateProjectile()
    {
        GameObject PlanePrefab = Resources.Load<GameObject>("Models/colonel/PlanePrefab");
        Plane = Instantiate(PlanePrefab);
        Plane.SetActive(false);
    }

    protected override void StartProjectile()
    {
        SetStartTransform();
        Plane.SetActive(true);
    }

    protected override void StopProjectile()
    {
        Plane.SetActive(false);
    }

    protected override void UpdateProjectile()
    {
        float t = (SimulationTime - (FireStartTime + MaxFireTime / 50f)) / MaxFireTime;
        if (t < 0)
            t = 0;
        t *= (t * t);
        float MetersPerSecond = Time.deltaTime * 200f;
        Plane.transform.position += MetersPerSecond * ((1 - t) * gameObject.transform.forward + t * gameObject.transform.up).normalized;
        Vector3 Rotation = new Vector3(t * -90, gameObject.transform.rotation.y, 0);
        Plane.transform.localEulerAngles = Rotation;
    }

    private void SetStartTransform()
    {
        // Arbitrary distance backward from the attacker, should be off screen.
        Vector3 StartPosition = gameObject.transform.position
        - gameObject.transform.forward * 30
        + gameObject.transform.up * 10;
        Plane.transform.position = StartPosition;
        Plane.transform.rotation = gameObject.transform.rotation;
    }
}
