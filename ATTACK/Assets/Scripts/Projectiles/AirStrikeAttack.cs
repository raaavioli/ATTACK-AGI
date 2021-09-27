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
        float t = (SimulationTime - FireStartTime) / MaxFireTime;
        if (t < 0)
            t = 0;
        float MetersPerSecond = Time.deltaTime * 200f;
        Vector3 Direction = (TargetPosition - Plane.transform.position).normalized;
        Plane.transform.position += MetersPerSecond * Direction;
        Plane.transform.rotation = Quaternion.LookRotation(Direction);
    }

    private void SetStartTransform()
    {
        // Arbitrary distance backward from the attacker, should be off screen.
        Vector3 StartPosition = gameObject.transform.position
        - gameObject.transform.forward * 30
        + gameObject.transform.up * 30;
        Plane.transform.position = StartPosition;
        Plane.transform.rotation = Quaternion.LookRotation(TargetPosition - StartPosition);
    }
}
